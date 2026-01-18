using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ShopMenu : MonoBehaviour {
    public static ShopMenu Instance { get; private set; }

    [Header("UI References")]
    public GameObject shopPanel; // Полноэкранная панель магазина
    public Transform itemsContainer; // Контейнер для товаров
    public Button closeButton; // Кнопка закрытия

    private bool isOpen = false;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        Debug.Log("[ShopMenu] Start called");
        
        // Проверяем EventSystem
        if (UnityEngine.EventSystems.EventSystem.current == null) {
            Debug.LogWarning("[ShopMenu] No EventSystem found! Creating one...");
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        } else {
            Debug.Log("[ShopMenu] EventSystem found: " + UnityEngine.EventSystems.EventSystem.current.gameObject.name);
        }
        
        if (closeButton != null) {
            closeButton.onClick.AddListener(CloseShop);
            Debug.Log("[ShopMenu] Close button listener added");
        } else {
            Debug.LogWarning("[ShopMenu] Close button is NULL!");
        }

        if (shopPanel != null) {
            shopPanel.SetActive(false);
            Debug.Log("[ShopMenu] Shop panel deactivated");
        } else {
            Debug.LogWarning("[ShopMenu] Shop panel is NULL!");
        }

        CreateShopItems();
    }

    void Update() {
        // Открываем/закрываем магазин по B
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame) {
            Debug.Log($"[ShopMenu] B key pressed. isOpen={isOpen}");
            if (isOpen) {
                CloseShop();
            } else {
                OpenShop();
            }
        }

        // ESC тоже закрывает
        if (isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) {
            Debug.Log("[ShopMenu] ESC key pressed");
            CloseShop();
        }
    }

    public void OpenShop() {
        Debug.Log("[ShopMenu] OpenShop called");
        
        if (shopPanel == null) {
            Debug.LogError("[ShopMenu] Cannot open - shopPanel is NULL!");
            return;
        }

        shopPanel.SetActive(true);
        isOpen = true;
        Time.timeScale = 0f; // ПАУЗА
        
        // Проверяем Canvas Raycaster
        Canvas canvas = shopPanel.GetComponentInParent<Canvas>();
        if (canvas != null) {
            UnityEngine.UI.GraphicRaycaster raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster == null) {
                Debug.LogWarning("[ShopMenu] No GraphicRaycaster on Canvas! Adding one...");
                canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            } else {
                Debug.Log("[ShopMenu] GraphicRaycaster found on Canvas");
            }
            
            // Добавляем CanvasGroup для блокировки кликов
            CanvasGroup cg = shopPanel.GetComponent<CanvasGroup>();
            if (cg == null) {
                cg = shopPanel.AddComponent<CanvasGroup>();
            }
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }
        
        Debug.Log("[ShopMenu] ✅ Shop opened - Game PAUSED");
    }

    public void CloseShop() {
        Debug.Log("[ShopMenu] CloseShop called");
        
        if (shopPanel == null) {
            Debug.LogError("[ShopMenu] Cannot close - shopPanel is NULL!");
            return;
        }

        shopPanel.SetActive(false);
        isOpen = false;
        Time.timeScale = 1f; // ПРОДОЛЖИТЬ
        Debug.Log("[ShopMenu] ✅ Shop closed - Game RESUMED");
    }

    void CreateShopItems() {
        Debug.Log("[ShopMenu] CreateShopItems called");
        
        if (BuildingManager.Instance == null) {
            Debug.LogError("[ShopMenu] BuildingManager.Instance is NULL!");
            return;
        }
        
        if (itemsContainer == null) {
            Debug.LogError("[ShopMenu] itemsContainer is NULL!");
            return;
        }

        // Очищаем контейнер
        foreach (Transform child in itemsContainer) {
            Destroy(child.gameObject);
        }

        Debug.Log($"[ShopMenu] Creating shop items. Available buildings: {BuildingManager.Instance.availableBuildings.Count}");
        
        // Создаём карточку товара для КАЖДОЙ постройки
        int itemCount = 0;
        foreach (BuildingData building in BuildingManager.Instance.availableBuildings) {
            CreateShopItem(building);
            itemCount++;
        }
        
        Debug.Log($"[ShopMenu] ✅ Created {itemCount} shop items");
    }

    void CreateShopItem(BuildingData building) {
        // Карточка товара
        GameObject itemObj = new GameObject($"ShopItem_{building.buildingName}");
        itemObj.transform.SetParent(itemsContainer, false);

        RectTransform itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(200, 100);

        // Фон карточки
        Image bgImg = itemObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.3f, 1f);

        // Название постройки
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(itemObj.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = building.buildingName;
        nameText.fontSize = 20;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;

        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.6f);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.offsetMin = new Vector2(10, 0);
        nameRect.offsetMax = new Vector2(-10, -5);

        // Описание
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(itemObj.transform, false);
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = GetBuildingDescription(building);
        descText.fontSize = 14;
        descText.alignment = TextAlignmentOptions.Center;
        descText.color = Color.gray;

        RectTransform descRect = descObj.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0.35f);
        descRect.anchorMax = new Vector2(1, 0.6f);
        descRect.offsetMin = new Vector2(10, 0);
        descRect.offsetMax = new Vector2(-10, 0);

        // Кнопка "Buy"
        GameObject btnObj = new GameObject("BuyButton");
        btnObj.transform.SetParent(itemObj.transform, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.2f, 0.05f);
        btnRect.anchorMax = new Vector2(0.8f, 0.3f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        btnImg.raycastTarget = true; // ВАЖНО для кликов!

        Button buyButton = btnObj.AddComponent<Button>();
        buyButton.targetGraphic = btnImg; // Привязываем Image к кнопке
        buyButton.onClick.AddListener(() => {
            Debug.Log($"[ShopMenu] 🛒 Lambda called for {building.buildingName}");
            OnBuyClicked(building);
        });
        Debug.Log($"[ShopMenu] Created buy button for {building.buildingName}");

        // Текст кнопки
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = $"Buy ({building.cost}💎)";
        btnText.fontSize = 18;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;

        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;
    }

    void OnBuyClicked(BuildingData building) {
        Debug.Log($"[ShopMenu] 🛒 BUY BUTTON CLICKED for {building.buildingName}");
        
        if (InventoryManager.Instance == null) {
            Debug.LogError("[ShopMenu] InventoryManager.Instance is NULL!");
            return;
        }

        Debug.Log($"[ShopMenu] Attempting to buy {building.buildingName} for {building.cost} biomass");
        bool success = InventoryManager.Instance.BuyBuilding(building);
        
        if (success) {
            Debug.Log($"[ShopMenu] ✅ Successfully bought {building.buildingName}!");
        } else {
            Debug.LogWarning($"[ShopMenu] ❌ Failed to buy {building.buildingName}!");
        }
    }

    string GetBuildingDescription(BuildingData building) {
        switch (building.buildingType) {
            case BuildingType.Turret:
                return "Auto-attacks\nenemies";
            case BuildingType.Barricade:
                return "Blocks enemy\npath";
            case BuildingType.AllyUnit:
                return "Patrols and\nshoots enemies";
            default:
                return "Unknown building";
        }
    }
}
