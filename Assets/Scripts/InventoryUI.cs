using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour {
    public static InventoryUI Instance { get; private set; }
    
    [Header("UI References")]
    public GameObject inventoryPanel; // Панель справа
    public Transform itemsContainer; // Контейнер для слотов инвентаря

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        Debug.Log("[InventoryUI] Start called");
        
        // Убеждаемся что у панели есть CanvasGroup для блокировки кликов
        if (inventoryPanel != null) {
            CanvasGroup cg = inventoryPanel.GetComponent<CanvasGroup>();
            if (cg == null) {
                cg = inventoryPanel.AddComponent<CanvasGroup>();
                Debug.Log("[InventoryUI] Added CanvasGroup to inventory panel");
            }
            cg.blocksRaycasts = true; // Блокируем клики через UI
            cg.interactable = true;
        }
        
        if (InventoryManager.Instance != null) {
            InventoryManager.Instance.OnInventoryChanged += RefreshInventory;
        }

        RefreshInventory();
    }

    void OnDestroy() {
        if (InventoryManager.Instance != null) {
            InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;
        }
    }

    void RefreshInventory() {
        Debug.Log("[InventoryUI] RefreshInventory called");
        
        if (InventoryManager.Instance == null) {
            Debug.LogError("[InventoryUI] InventoryManager.Instance is NULL!");
            return;
        }
        
        if (itemsContainer == null) {
            Debug.LogError("[InventoryUI] itemsContainer is NULL!");
            return;
        }

        // Очищаем старые слоты
        foreach (Transform child in itemsContainer) {
            Destroy(child.gameObject);
        }

        // Получаем инвентарь
        Dictionary<BuildingType, int> inventory = InventoryManager.Instance.GetInventory();
        Debug.Log($"[InventoryUI] Inventory has {inventory.Count} different building types");

        // Создаём слот для каждой постройки в инвентаре
        int slotCount = 0;
        foreach (var kvp in inventory) {
            BuildingType type = kvp.Key;
            int count = kvp.Value;

            Debug.Log($"[InventoryUI] Creating slot for {type} x{count}");
            
            // Находим BuildingData
            BuildingData buildingData = GetBuildingData(type);
            if (buildingData != null) {
                CreateInventorySlot(buildingData, count);
                slotCount++;
            } else {
                Debug.LogWarning($"[InventoryUI] Could not find BuildingData for {type}");
            }
        }
        
        Debug.Log($"[InventoryUI] ✅ Created {slotCount} inventory slots");
    }

    void CreateInventorySlot(BuildingData building, int count) {
        // Слот в инвентаре
        GameObject slotObj = new GameObject($"InvSlot_{building.buildingName}");
        slotObj.transform.SetParent(itemsContainer, false);

        RectTransform slotRect = slotObj.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(80, 80);

        // Фон слота
        Image slotImg = slotObj.AddComponent<Image>();
        slotImg.color = new Color(0.3f, 0.3f, 0.4f, 1f);
        slotImg.raycastTarget = true; // ВАЖНО для кликов!

        // Кнопка
        Button slotButton = slotObj.AddComponent<Button>();
        slotButton.targetGraphic = slotImg; // Привязываем Image
        slotButton.onClick.AddListener(() => {
            Debug.Log($"[InventoryUI] 🖱️ Lambda called for {building.buildingName}");
            OnSlotClicked(building);
        });

        // Название
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(slotObj.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = building.buildingName;
        nameText.fontSize = 14;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;

        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.offsetMin = new Vector2(5, 0);
        nameRect.offsetMax = new Vector2(-5, -5);

        // Количество
        GameObject countObj = new GameObject("Count");
        countObj.transform.SetParent(slotObj.transform, false);
        TextMeshProUGUI countText = countObj.AddComponent<TextMeshProUGUI>();
        countText.text = $"x{count}";
        countText.fontSize = 16;
        countText.fontStyle = FontStyles.Bold;
        countText.alignment = TextAlignmentOptions.Center;
        countText.color = Color.yellow;

        RectTransform countRect = countObj.GetComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0, 0);
        countRect.anchorMax = new Vector2(1, 0.5f);
        countRect.offsetMin = new Vector2(5, 5);
        countRect.offsetMax = new Vector2(-5, 0);
    }

    void OnSlotClicked(BuildingData building) {
        Debug.Log($"[InventoryUI] 🖱️ INVENTORY SLOT CLICKED for {building.buildingName}");
        
        if (BuildingManager.Instance == null) {
            Debug.LogError("[InventoryUI] BuildingManager.Instance is NULL!");
            return;
        }

        // Выбираем постройку для размещения
        BuildingManager.Instance.SelectBuildingFromInventory(building);
        
        // ОТКЛЮЧАЕМ блокировку raycast для инвентаря, чтобы клики проходили на карту
        if (inventoryPanel != null) {
            CanvasGroup cg = inventoryPanel.GetComponent<CanvasGroup>();
            if (cg != null) {
                cg.blocksRaycasts = false;
                Debug.Log("[InventoryUI] Disabled blocksRaycasts for placement mode");
            }
        }
        
        Debug.Log($"[InventoryUI] ✅ Selected {building.buildingName} from inventory for placement");
    }

    BuildingData GetBuildingData(BuildingType type) {
        if (BuildingManager.Instance == null) return null;

        foreach (BuildingData building in BuildingManager.Instance.availableBuildings) {
            if (building.buildingType == type) {
                return building;
            }
        }

        return null;
    }
}
