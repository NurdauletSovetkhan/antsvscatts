using UnityEngine;

public class Turret : Building
{
    [Header("Turret Data")]
    public TurretData turretData; // ScriptableObject с характеристиками
    
    [Header("Turret Stats")]
    public float attackRange = 5f;
    public float damage = 10f;
    public float fireRate = 1f; // Выстрелов в секунду
    private float fireTimer;

    [Header("Visual")]
    public Transform firePoint; // Точка, откуда летит снаряд
    public GameObject projectilePrefab; // Префаб снаряда (опционально)
    
    [Header("Animation")]
    private Animator animator;
    private bool shouldShoot = false; // Флаг для синхронизации с анимацией

    private Transform currentTarget;

    protected override void Start() {
        base.Start();
        
        buildingType = BuildingType.Turret;
        
        // Инициализируем из TurretData если назначен
        if (turretData != null) {
            attackRange = turretData.attackRange;
            damage = turretData.damage;
            fireRate = turretData.fireRate;
            projectilePrefab = turretData.projectilePrefab;
            
            // Визуальные настройки
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) {
                if (turretData.sprite != null) {
                    sr.sprite = turretData.sprite;
                }
                sr.color = turretData.tintColor;
            }
            
            transform.localScale = Vector3.one * turretData.scale;
            
            Debug.Log($"[Turret] {turretData.turretName} created! Damage: {damage}, FireRate: {fireRate}, Range: {attackRange}");
        }
        
        fireTimer = 1f / fireRate;
        
        animator = GetComponent<Animator>();
        if (animator == null) {
            Debug.LogWarning($"[Turret] No Animator on {gameObject.name}!");
        }
    }

    void Update() {
        // Турели стреляют только в фазе атаки
        if (GameManager.Instance == null || GameManager.Instance.CurrentPhase != GameManager.GamePhase.ActionPhase) {
            return;
        }

        // Ищем врага, если его нет или он умер
        if (currentTarget == null || !currentTarget.gameObject.activeSelf) {
            FindClosestEnemy();
        }

        // Если цель есть и в радиусе - атакуем
        if (currentTarget != null) {
            float distance = Vector2.Distance(transform.position, currentTarget.position);
            
            if (distance <= attackRange) {
                AttackTarget();
            } else {
                currentTarget = null; // Цель вышла из радиуса
            }
        }
    }

    void FindClosestEnemy() {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (GameObject enemy in enemies) {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            
            if (distance < closestDistance && distance <= attackRange) {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        currentTarget = closestEnemy;
    }

    void AttackTarget() {
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f) {
            // Триггерим анимацию стрельбы
            if (animator != null) {
                animator.SetTrigger("Shooting");
            }
            
            // Флаг для вызова из Animation Event
            shouldShoot = true;
            
            // Если нет аниматора - стреляем сразу
            if (animator == null) {
                FireProjectile();
                shouldShoot = false;
            }
            
            fireTimer = 1f / fireRate;
        }
    }
    
    // Этот метод вызывается через Animation Event!
    public void OnShootAnimationEvent() {
        if (shouldShoot) {
            FireProjectile();
            shouldShoot = false;
        }
    }

    void FireProjectile() {
        if (currentTarget == null) {
            Debug.LogWarning("Turret: No target to fire at!");
            return;
        }

        // Если есть префаб снаряда - создаем пулю
        if (projectilePrefab != null) {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            
            Debug.Log($"Projectile created at {spawnPos}. Prefab: {projectilePrefab.name}");
            
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null) {
                proj.SetTarget(currentTarget, damage);
                Debug.Log($"Turret fired projectile at {currentTarget.name}!");
            } else {
                Debug.LogError("Projectile prefab doesn't have Projectile script!");
            }
        } else {
            Debug.LogWarning("No projectile prefab assigned! Using instant damage.");
            // Иначе мгновенный урон (старый вариант)
            Health enemyHealth = currentTarget.GetComponent<Health>();
            if (enemyHealth != null) {
                enemyHealth.TakeDamage(damage);
            }
            Debug.Log($"Turret fired instant damage at {currentTarget.name}!");
        }
    }

    // Визуализация радиуса в редакторе
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
