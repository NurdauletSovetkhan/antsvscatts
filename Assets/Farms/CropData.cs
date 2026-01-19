using UnityEngine;

[CreateAssetMenu(fileName = "New Crop", menuName = "Farm/Crop Data")]
public class CropData : ScriptableObject
{
    public string cropName;
    public Sprite icon;
    public int seedCost = 5; // <--- НОВОЕ: Цена покупки семени за биомассу

    [Header("Стадии и Продукция")]
    public GameObject[] growthStages;
    public GameObject harvestVFX;
    public GameObject produce; // Пока можно оставить, но логику сбора мы изменим
    public float timeBetweenStages = 2f;
}