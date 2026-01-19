using UnityEngine;

[CreateAssetMenu(fileName = "New Barricade", menuName = "Game/Barricade Data")]
public class BarricadeData : ScriptableObject
{
    [Header("Basic Info")]
    public string barricadeName = "Cardboard Barricade";
    public BarricadeType barricadeType = BarricadeType.Cardboard;
    
    [Header("Stats")]
    public float maxHealth = 30f;
    public int cost = 10;
    
    [Header("Visual")]
    public Sprite icon; // Иконка для магазина
    public Sprite sprite; // Спрайт здания
    public Color tintColor = Color.white;
    public float scale = 1f;
    
    [TextArea]
    public string description;
}

public enum BarricadeType
{
    Cardboard,  // Картон: 30 HP, 10 биомассы
    Wood,       // Дерево: 80 HP, 25 биомассы
    Stone       // Камень: 150 HP, 50 биомассы
}
