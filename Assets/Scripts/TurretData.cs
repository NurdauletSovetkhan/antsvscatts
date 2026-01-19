using UnityEngine;

[CreateAssetMenu(fileName = "New Turret", menuName = "Game/Turret Data")]
public class TurretData : ScriptableObject
{
    [Header("Basic Info")]
    public string turretName = "Basic Turret";
    public TurretTier tier = TurretTier.Level1;
    
    [Header("Combat Stats")]
    public float attackRange = 5f;
    public float damage = 10f;
    public float fireRate = 1f; // Выстрелов в секунду
    
    [Header("Economy")]
    public int cost = 50;
    
    [Header("Visual")]
    public Sprite icon; // Иконка для магазина
    public Sprite sprite; // Спрайт здания
    public Color tintColor = Color.white;
    public float scale = 1f;
    
    [Header("Projectile")]
    public GameObject projectilePrefab;
    
    [TextArea]
    public string description;
}

public enum TurretTier
{
    Level1,  // Tower 01 Level 01: 10 урона, 1 выстр/сек, 50 биомассы
    Level2,  // Tower 01 Level 02: 20 урона, 1.5 выстр/сек, 100 биомассы
    Level3   // Tower 01 Level 03: 35 урона, 2 выстр/сек, 200 биомассы
}
