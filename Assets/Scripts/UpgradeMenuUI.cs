using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class UpgradeMenuUI : MonoBehaviour
{
    [Header("Menu Control")]
    public GameObject upgradeMenuPanel;
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Tree Selection")]
    public Button turretTreeButton;
    public Button barricadeTreeButton;
    public Button allyTreeButton;

    [Header("Current Tree Display")]
    public GameObject treeDisplayPanel;
    public TextMeshProUGUI treeTitleText;
    public Transform upgradeNodesContainer;
    public GameObject upgradeNodePrefab;

    [Header("Unlock Base")]
    public GameObject unlockBasePanel;
    public Button unlockBaseButton;
    public TextMeshProUGUI unlockBaseCostText;

    private BuildingType currentTreeType;
    private bool isMenuOpen = false;

    void Start() {
        // Привязываем кнопки
        if (turretTreeButton != null)
            turretTreeButton.onClick.AddListener(() => ShowTree(BuildingType.Turret));
        
        if (barricadeTreeButton != null)
            barricadeTreeButton.onClick.AddListener(() => ShowTree(BuildingType.Barricade));
        
        if (allyTreeButton != null)
            allyTreeButton.onClick.AddListener(() => ShowTree(BuildingType.AllyUnit));

        if (unlockBaseButton != null)
            unlockBaseButton.onClick.AddListener(UnlockBaseBuilding);

        // Закрываем меню по умолчанию
        if (upgradeMenuPanel != null)
            upgradeMenuPanel.SetActive(false);
    }

    void Update() {
        // Открытие/закрытие меню
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame) {
            ToggleMenu();
        }
    }

    public void ToggleMenu() {
        isMenuOpen = !isMenuOpen;
        if (upgradeMenuPanel != null) {
            upgradeMenuPanel.SetActive(isMenuOpen);
            
            // Паузим игру когда меню открыто
            Time.timeScale = isMenuOpen ? 0f : 1f;
        }

        if (isMenuOpen) {
            // Показываем первое дерево по умолчанию
            ShowTree(BuildingType.Turret);
        }
    }

    void ShowTree(BuildingType type) {
        currentTreeType = type;
        
        if (UpgradeManager.Instance == null) return;

        BuildingTree tree = UpgradeManager.Instance.buildingTrees.Find(t => t.buildingType == type);
        if (tree == null) return;

        // Обновляем заголовок
        if (treeTitleText != null) {
            treeTitleText.text = tree.treeName;
        }

        // Показываем/скрываем панель разблокировки
        if (tree.isBaseUnlocked) {
            if (unlockBasePanel != null) unlockBasePanel.SetActive(false);
            if (treeDisplayPanel != null) treeDisplayPanel.SetActive(true);
            DisplayUpgrades(tree);
        } else {
            if (unlockBasePanel != null) unlockBasePanel.SetActive(true);
            if (treeDisplayPanel != null) treeDisplayPanel.SetActive(false);
            if (unlockBaseCostText != null) {
                unlockBaseCostText.text = $"Unlock {tree.treeName}\nCost: 50 💎";
            }
        }
    }

    void DisplayUpgrades(BuildingTree tree) {
        // Очищаем старые узлы
        foreach (Transform child in upgradeNodesContainer) {
            Destroy(child.gameObject);
        }

        // Создаем узлы для каждого апгрейда
        foreach (UpgradeNode upgrade in tree.upgrades) {
            GameObject nodeObj = Instantiate(upgradeNodePrefab, upgradeNodesContainer);
            
            // Настраиваем узел
            TextMeshProUGUI nameText = nodeObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI costText = nodeObj.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descText = nodeObj.transform.Find("DescText")?.GetComponent<TextMeshProUGUI>();
            Button buyButton = nodeObj.GetComponent<Button>();

            if (nameText != null) nameText.text = upgrade.upgradeName;
            if (costText != null) costText.text = $"{upgrade.cost} 💎";
            if (descText != null) descText.text = upgrade.description;

            // Визуальное состояние
            if (upgrade.isUnlocked) {
                if (buyButton != null) buyButton.interactable = false;
                if (nameText != null) nameText.color = Color.green;
            } else {
                // Проверяем доступность
                bool canPurchase = CanPurchaseUpgrade(tree, upgrade);
                if (buyButton != null) {
                    buyButton.interactable = canPurchase;
                    buyButton.onClick.AddListener(() => PurchaseUpgrade(upgrade.upgradeName));
                }
            }
        }
    }

    bool CanPurchaseUpgrade(BuildingTree tree, UpgradeNode upgrade) {
        // Проверяем ресурсы
        if (ResourceManager.Instance.GetBiomass() < upgrade.cost) return false;

        // Проверяем требования
        foreach (string required in upgrade.requiredUpgrades) {
            UpgradeNode req = tree.upgrades.Find(u => u.upgradeName == required);
            if (req == null || !req.isUnlocked) return false;
        }

        return true;
    }

    void UnlockBaseBuilding() {
        if (UpgradeManager.Instance.UnlockBuilding(currentTreeType)) {
            ShowTree(currentTreeType); // Обновляем отображение
        }
    }

    void PurchaseUpgrade(string upgradeName) {
        if (UpgradeManager.Instance.PurchaseUpgrade(currentTreeType, upgradeName)) {
            ShowTree(currentTreeType); // Обновляем отображение
        }
    }
}
