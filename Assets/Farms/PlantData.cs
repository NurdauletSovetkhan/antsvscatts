using UnityEngine;

[CreateAssetMenu(fileName = "New Plant", menuName = "Farm/Plant")]
public class PlantData : ScriptableObject
{
    public string plantName;       // Название (Морковь)
    public float timeToGrow;       // Время роста между стадиями (сек)
    public Sprite[] growthStages;  // Массив спрайтов (0=Семя, 1=Росток, 2=Готово)
}