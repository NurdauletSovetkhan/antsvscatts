using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Common Ant";
    public EnemyType enemyType = EnemyType.Common;
    
    [Header("Stats")]
    public float maxHealth = 20f;
    public float moveSpeed = 2f;
    public float attackDamage = 5f;
    public float attackCooldown = 1f;
    public float attackRange = 1f; // Для дальнобойных
    
    [Header("Rewards")]
    public int biomassDropMin = 2;
    public int biomassDropMax = 5;
    
    [Header("Special Abilities")]
    public bool canShoot = false; // Для Shooting муравьёв
    public GameObject projectilePrefab; // Снаряд для стреляющих
    public float shootCooldown = 2f;
    public float shootRange = 8f;
    
    [Header("Visual")]
    public Color tintColor = Color.white; // Цветовой оттенок
    public float scale = 1f; // Размер модели
    
    [Header("Boss Settings")]
    public bool isBoss = false;
    public int bossPhase = 1; // Количество фаз у босса
}

public enum EnemyType
{
    Common,      // Базовый муравей
    Shooting,    // Стреляющий
    Tank,        // Танк (много HP)
    Soldier,     // Солдат (быстрый + сильный)
    Boss,        // Босс
    Goblin       // Гоблин (очень быстрый, слабый)
}
