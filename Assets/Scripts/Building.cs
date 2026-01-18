using UnityEngine;

// Базовый класс для всех построек
public enum BuildingType
{
    Turret,
    Barricade,
    AllyUnit
}

[System.Serializable]
public class BuildingData
{
    public string buildingName;
    public BuildingType buildingType;
    public int cost;
    public GameObject prefab;
    public Sprite icon;
    
    [TextArea]
    public string description;
}

public class Building : MonoBehaviour
{
    [Header("Building Info")]
    public BuildingType buildingType;
    public int buildCost;
    
    protected Health healthComponent;

    protected virtual void Start() {
        healthComponent = GetComponent<Health>();
    }

    // Вызывается при размещении постройки
    public virtual void OnPlaced() {
        Debug.Log($"{buildingType} placed!");
    }

    // Вызывается при уничтожении
    public virtual void OnDestroyed() {
        Debug.Log($"{buildingType} destroyed!");
    }
}
