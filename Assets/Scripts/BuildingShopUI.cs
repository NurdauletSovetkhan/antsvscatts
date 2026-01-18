using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// UI для выбора типа постройки (НЕ меню прокачки!)
// Это панель, которая всегда видна на экране
public class BuildingShopUI : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject shopPanel; // Вся панель магазина
    public KeyCode toggleKey = KeyCode.B; // Клавиша показать/скрыть

    [Header("UI Elements")]
    public Transform buttonContainer;
    public TextMeshProUGUI selectedBuildingText;

    private bool isPanelVisible = true;

    void Start() {
        CreateBuildingButtons();
        
        if (shopPanel != null) {
            shopPanel.SetActive(isPanelVisible);
        }
    }

    void Update() {
        UpdateSelectedText();
        
        // Переключение видимости панели
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame) {
            TogglePanel();
        }
    }

    void TogglePanel() {
        isPanelVisible = !isPanelVisible;
        if (shopPanel != null) {
            shopPanel.SetActive(isPanelVisible);
        }
    }

    void CreateBuildingButtons() {
        if (BuildingManager.Instance == null || buttonContainer == null) return;

        // Очищаем старые кнопки
        foreach (Transform child in buttonContainer) {
            Destroy(child.gameObject);
        }

        // Создаем кнопки только для РАЗБЛОКИРОВАННЫХ построек
        for (int i = 0; i < BuildingManager.Instance.availableBuildings.Count; i++) {
            int index = i;
            BuildingData building = BuildingManager.Instance.availableBuildings[i];

            // Проверяем разблокировку
            bool isUnlocked = UpgradeManager.Instance == null || 
                             UpgradeManager.Instance.IsBuildingUnlocked(building.buildingType);

            if (!isUnlocked) continue; // Пропускаем заблокированные

            // Создаем кнопку
            GameObject buttonObj = new GameObject($"Btn_{building.buildingName}");
            buttonObj.transform.SetParent(buttonContainer, false);
            
            // RectTransform для кнопки
            RectTransform btnRect = buttonObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(100, 60); // Фиксированный размер
            
            // Фон кнопки
            Image img = buttonObj.AddComponent<Image>();
            img.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            
            Button button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(() => OnBuildingButtonClicked(index));

            // Текст на кнопке
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            
            buttonText.text = $"{building.buildingName}\n{building.cost}💎";
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontSize = 16;
            buttonText.color = Color.black;
            buttonText.enableWordWrapping = true;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 5);
            textRect.offsetMax = new Vector2(-5, -5);
        }
    }

    void OnBuildingButtonClicked(int index) {
        if (BuildingManager.Instance != null) {
            BuildingManager.Instance.SelectBuilding(index);
            Debug.Log($"Selected building for placement");
        }
    }

    void UpdateSelectedText() {
        if (selectedBuildingText != null && BuildingManager.Instance != null) {
            if (BuildingManager.Instance.selectedBuilding != null) {
                selectedBuildingText.text = $"Building: {BuildingManager.Instance.selectedBuilding.buildingName} | LMB: Place | RMB: Cancel";
            } else {
                selectedBuildingText.text = "Select a building";
            }
        }
    }

    // Вызывается когда разблокируется новое здание
    public void RefreshButtons() {
        CreateBuildingButtons();
    }
}
