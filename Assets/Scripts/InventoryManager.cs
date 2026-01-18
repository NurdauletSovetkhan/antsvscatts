using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour {
    public static InventoryManager Instance { get; private set; }

    // Инвентарь: тип постройки -> количество
    private Dictionary<BuildingType, int> inventory = new Dictionary<BuildingType, int>();

    public delegate void InventoryChangedDelegate();
    public event InventoryChangedDelegate OnInventoryChanged;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    // Покупка постройки (добавляет в инвентарь)
    public bool BuyBuilding(BuildingData building) {
        Debug.Log($"[InventoryManager] BuyBuilding called for {building.buildingName}");
        
        if (ResourceManager.Instance == null) {
            Debug.LogError("[InventoryManager] ResourceManager.Instance is NULL!");
            return false;
        }

        int currentBiomass = ResourceManager.Instance.GetBiomass();
        Debug.Log($"[InventoryManager] Current biomass: {currentBiomass}, Building cost: {building.cost}");
        
        // Проверяем хватает ли ресурсов
        if (currentBiomass < building.cost) {
            Debug.LogWarning($"[InventoryManager] ❌ Not enough biomass! Need {building.cost}, have {currentBiomass}");
            return false;
        }

        // Списываем ресурсы
        bool spent = ResourceManager.Instance.SpendBiomass(building.cost);
        Debug.Log($"[InventoryManager] SpendBiomass returned: {spent}");

        // Добавляем в инвентарь
        if (inventory.ContainsKey(building.buildingType)) {
            inventory[building.buildingType]++;
        } else {
            inventory[building.buildingType] = 1;
        }

        Debug.Log($"[InventoryManager] ✅ Bought {building.buildingName}. Now have {inventory[building.buildingType]} in inventory");
        OnInventoryChanged?.Invoke();
        return true;
    }

    // Использовать постройку из инвентаря (при размещении)
    public bool UseBuilding(BuildingType type) {
        Debug.Log($"[InventoryManager] UseBuilding called for {type}");
        
        if (!inventory.ContainsKey(type) || inventory[type] <= 0) {
            Debug.LogWarning($"[InventoryManager] ❌ No {type} in inventory!");
            return false;
        }

        inventory[type]--;
        Debug.Log($"[InventoryManager] ✅ Used {type}. Remaining: {inventory[type]}");
        
        if (inventory[type] <= 0) {
            inventory.Remove(type);
            Debug.Log($"[InventoryManager] Removed {type} from inventory (count reached 0)");
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    // Получить количество постройки в инвентаре
    public int GetBuildingCount(BuildingType type) {
        return inventory.ContainsKey(type) ? inventory[type] : 0;
    }

    // Получить все постройки в инвентаре
    public Dictionary<BuildingType, int> GetInventory() {
        return new Dictionary<BuildingType, int>(inventory);
    }

    // Есть ли постройка в инвентаре
    public bool HasBuilding(BuildingType type) {
        return inventory.ContainsKey(type) && inventory[type] > 0;
    }
}
