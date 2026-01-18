using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

// Менеджер построек - управляет размещением и типами зданий
public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    [Header("Building Types")]
    public List<BuildingData> availableBuildings = new List<BuildingData>();
    
    [Header("Current Selection")]
    public BuildingData selectedBuilding;
    public int selectedBuildingIndex = -1;
    private bool isFromInventory = false;

    [Header("Placement")]
    public LayerMask groundLayer; // Слой, на котором можно строить
    public float gridSize = 1f; // Размер сетки для выравнивания
    public bool snapToGrid = true;

    private GameObject previewObject;
    private SpriteRenderer previewRenderer;
    private Color validColor = new Color(0f, 1f, 0f, 0.5f);
    private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Update() {
        if (selectedBuilding != null) {
            UpdatePreview();
            HandlePlacement();
        }
    }

    public void SelectBuilding(int index) {
        if (index < 0 || index >= availableBuildings.Count) {
            DeselectBuilding();
            return;
        }

        selectedBuildingIndex = index;
        selectedBuilding = availableBuildings[index];
        isFromInventory = false;
        
        CreatePreview();
        Debug.Log($"Selected building: {selectedBuilding.buildingName} (Cost: {selectedBuilding.cost})");
    }

    public void SelectBuildingFromInventory(BuildingData building) {
        if (InventoryManager.Instance == null) return;

        if (!InventoryManager.Instance.HasBuilding(building.buildingType)) {
            Debug.Log($"No {building.buildingName} in inventory!");
            return;
        }

        selectedBuilding = building;
        selectedBuildingIndex = -1;
        isFromInventory = true;
        
        CreatePreview();
        Debug.Log($"Selected {building.buildingName} from inventory for placement");
    }

    public void SelectBuilding(BuildingType buildingType) {
        int index = availableBuildings.FindIndex(b => b.buildingType == buildingType);
        SelectBuilding(index);
    }

    public void DeselectBuilding() {
        selectedBuilding = null;
        selectedBuildingIndex = -1;
        isFromInventory = false;
        DestroyPreview();
        
        // ВКЛЮЧАЕМ обратно блокировку raycast для инвентаря
        if (InventoryUI.Instance != null && InventoryUI.Instance.inventoryPanel != null) {
            CanvasGroup cg = InventoryUI.Instance.inventoryPanel.GetComponent<CanvasGroup>();
            if (cg != null) {
                cg.blocksRaycasts = true;
                Debug.Log("[BuildingManager] Re-enabled blocksRaycasts for inventory");
            }
        }
    }

    void CreatePreview() {
        DestroyPreview();

        if (selectedBuilding.prefab != null) {
            previewObject = Instantiate(selectedBuilding.prefab);
            previewObject.name = "BuildingPreview";
            
            // Отключаем компоненты
            foreach (var component in previewObject.GetComponents<MonoBehaviour>()) {
                component.enabled = false;
            }
            foreach (var collider in previewObject.GetComponents<Collider2D>()) {
                collider.enabled = false;
            }

            previewRenderer = previewObject.GetComponent<SpriteRenderer>();
            if (previewRenderer != null) {
                previewRenderer.color = validColor;
            }
        }
    }

    void UpdatePreview() {
        if (previewObject == null) return;

        // Используем Mouse.current для позиции
        if (Mouse.current == null) return;
        
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0));
        mousePos.z = 0;

        if (snapToGrid) {
            mousePos.x = Mathf.Round(mousePos.x / gridSize) * gridSize;
            mousePos.y = Mathf.Round(mousePos.y / gridSize) * gridSize;
        }

        previewObject.transform.position = mousePos;

        // Проверяем, можно ли здесь строить
        bool canPlace = CanPlaceAt(mousePos);
        if (previewRenderer != null) {
            previewRenderer.color = canPlace ? validColor : invalidColor;
        }
    }

    bool CanPlaceAt(Vector3 position) {
        // Если постройка ИЗ ИНВЕНТАРЯ - не проверяем ресурсы (уже куплено)
        if (!isFromInventory) {
            // Проверяем, достаточно ли ресурсов для покупки
            if (ResourceManager.Instance.GetBiomass() < selectedBuilding.cost) {
                return false;
            }
        }

        // Уменьшенный радиус для точечной проверки
        float checkRadius = 0.3f; // Маленький радиус - только центр постройки
        
        // Проверяем, не занято ли место
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, checkRadius);
        
        foreach (var col in colliders) {
            // Пропускаем превью
            if (col.name.Contains("Preview")) continue;
            
            // Пропускаем триггеры
            if (col.isTrigger) continue;
            
            // Пропускаем игрока
            if (col.CompareTag("Player")) continue;
            
            // Проверяем только ПОСТРОЙКИ (все с компонентом Building)
            Building building = col.GetComponent<Building>();
            if (building != null) {
                Debug.Log($"[BuildingManager] ❌ Cannot place - found {col.name}");
                return false;
            }
        }

        return true;
    }

    void HandlePlacement() {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) { // ЛКМ - разместить
            // Размещаем только если выбрана постройка
            if (selectedBuilding != null && previewObject != null) {
                PlaceBuilding();
            }
        }

        if ((Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) || 
            (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)) { // ПКМ или ESC - отмена
            DeselectBuilding();
        }
    }

    void PlaceBuilding() {
        if (previewObject == null || selectedBuilding == null) {
            Debug.LogWarning("[BuildingManager] Cannot place - preview or building is NULL!");
            return;
        }

        Vector3 placePos = previewObject.transform.position;

        if (!CanPlaceAt(placePos)) {
            Debug.LogWarning("[BuildingManager] ❌ Cannot place building here!");
            return;
        }

        // Если из инвентаря - используем постройку из инвентаря
        if (isFromInventory) {
            if (InventoryManager.Instance != null) {
                if (!InventoryManager.Instance.UseBuilding(selectedBuilding.buildingType)) {
                    Debug.LogWarning("[BuildingManager] No building in inventory!");
                    return;
                }
            }
        } else {
            // Иначе покупаем напрямую (старая система)
            if (!ResourceManager.Instance.SpendBiomass(selectedBuilding.cost)) {
                Debug.LogWarning("[BuildingManager] Not enough biomass!");
                return;
            }
        }

        // Создаем постройку
        if (selectedBuilding.prefab == null) {
            Debug.LogError("[BuildingManager] Building prefab is NULL!");
            return;
        }
        
        GameObject building = Instantiate(selectedBuilding.prefab, placePos, Quaternion.identity);
        
        // Вызываем событие размещения
        Building buildingScript = building.GetComponent<Building>();
        if (buildingScript != null) {
            buildingScript.OnPlaced();
        }

        Debug.Log($"[BuildingManager] ✅ Placed {selectedBuilding.buildingName}!");
        DeselectBuilding();
    }

    void DestroyPreview() {
        if (previewObject != null) {
            Destroy(previewObject);
            previewObject = null;
        }
    }

    void OnDestroy() {
        DestroyPreview();
    }
}
