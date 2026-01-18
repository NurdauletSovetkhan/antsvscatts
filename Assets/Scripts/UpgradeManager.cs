using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UpgradeNode
{
    public string upgradeName;
    public string description;
    public int cost;
    public Sprite icon;
    public bool isUnlocked = false;
    
    // Улучшения характеристик
    public float damageBonus = 0f;
    public float rangeBonus = 0f;
    public float fireRateBonus = 0f;
    public float healthBonus = 0f;
    public float speedBonus = 0f;
    
    // Требования
    public List<string> requiredUpgrades = new List<string>();
}

[System.Serializable]
public class BuildingTree
{
    public BuildingType buildingType;
    public string treeName;
    public bool isBaseUnlocked = false;
    public List<UpgradeNode> upgrades = new List<UpgradeNode>();
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Tech Trees")]
    public List<BuildingTree> buildingTrees = new List<BuildingTree>();

    [Header("Current Stats")]
    private Dictionary<BuildingType, BuildingStats> currentStats = new Dictionary<BuildingType, BuildingStats>();

    void Awake() {
        if (Instance == null) {
            Instance = this;
            InitializeStats();
        } else {
            Destroy(gameObject);
        }
    }

    void InitializeStats() {
        // Базовые статы для каждого типа постройки
        currentStats[BuildingType.Turret] = new BuildingStats {
            damage = 10f,
            range = 5f,
            fireRate = 1f,
            health = 100f,
            speed = 0f
        };
        
        currentStats[BuildingType.Barricade] = new BuildingStats {
            damage = 0f,
            range = 0f,
            fireRate = 0f,
            health = 200f,
            speed = 0f
        };
        
        currentStats[BuildingType.AllyUnit] = new BuildingStats {
            damage = 8f,
            range = 6f,
            fireRate = 0.8f,
            health = 50f,
            speed = 3f
        };
    }

    public bool UnlockBuilding(BuildingType type) {
        BuildingTree tree = buildingTrees.Find(t => t.buildingType == type);
        if (tree != null && !tree.isBaseUnlocked) {
            // Проверяем ресурсы (базовая цена = 50)
            if (ResourceManager.Instance.SpendBiomass(50)) {
                tree.isBaseUnlocked = true;
                Debug.Log($"{type} unlocked!");
                return true;
            }
        }
        return false;
    }

    public bool PurchaseUpgrade(BuildingType type, string upgradeName) {
        BuildingTree tree = buildingTrees.Find(t => t.buildingType == type);
        if (tree == null || !tree.isBaseUnlocked) return false;

        UpgradeNode upgrade = tree.upgrades.Find(u => u.upgradeName == upgradeName);
        if (upgrade == null || upgrade.isUnlocked) return false;

        // Проверяем требования
        foreach (string required in upgrade.requiredUpgrades) {
            UpgradeNode requiredUpgrade = tree.upgrades.Find(u => u.upgradeName == required);
            if (requiredUpgrade == null || !requiredUpgrade.isUnlocked) {
                Debug.LogWarning($"Required upgrade '{required}' not unlocked!");
                return false;
            }
        }

        // Покупаем
        if (ResourceManager.Instance.SpendBiomass(upgrade.cost)) {
            upgrade.isUnlocked = true;
            ApplyUpgrade(type, upgrade);
            Debug.Log($"Purchased upgrade: {upgradeName} for {type}");
            return true;
        }

        return false;
    }

    void ApplyUpgrade(BuildingType type, UpgradeNode upgrade) {
        if (!currentStats.ContainsKey(type)) return;

        BuildingStats stats = currentStats[type];
        stats.damage += upgrade.damageBonus;
        stats.range += upgrade.rangeBonus;
        stats.fireRate += upgrade.fireRateBonus;
        stats.health += upgrade.healthBonus;
        stats.speed += upgrade.speedBonus;

        currentStats[type] = stats;
    }

    public BuildingStats GetStats(BuildingType type) {
        return currentStats.ContainsKey(type) ? currentStats[type] : new BuildingStats();
    }

    public bool IsBuildingUnlocked(BuildingType type) {
        BuildingTree tree = buildingTrees.Find(t => t.buildingType == type);
        return tree != null && tree.isBaseUnlocked;
    }

    public bool IsUpgradeUnlocked(BuildingType type, string upgradeName) {
        BuildingTree tree = buildingTrees.Find(t => t.buildingType == type);
        if (tree == null) return false;

        UpgradeNode upgrade = tree.upgrades.Find(u => u.upgradeName == upgradeName);
        return upgrade != null && upgrade.isUnlocked;
    }
}

[System.Serializable]
public struct BuildingStats
{
    public float damage;
    public float range;
    public float fireRate;
    public float health;
    public float speed;
}
