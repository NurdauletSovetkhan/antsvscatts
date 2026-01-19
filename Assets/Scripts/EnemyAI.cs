using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Data")]
    public EnemyData enemyData; // ScriptableObject с характеристиками
    
    [Header("Runtime Stats")]
    private float moveSpeed;
    private float attackDamage;
    private float attackCooldown;
    private Transform target; // Цель (обычно холодильник)
    private float attackTimer;
    private int biomassDropAmount;

    [Header("Shooting (для Shooting типа)")]
    private float shootTimer;
    private bool isShooting = false;

    [Header("Components")]
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Health healthComponent;
    
    [Header("State")]
    private bool isAttacking = false;
    private bool isDead = false;
    private float distanceToTarget;
    
    [Header("Target Priority")]
    private float retargetTimer = 0f;
    private float retargetInterval = 1f; // Переоценка цели каждую секунду

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        healthComponent = GetComponent<Health>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Инициализируем характеристики из EnemyData
        if (enemyData != null) {
            moveSpeed = enemyData.moveSpeed;
            attackDamage = enemyData.attackDamage;
            attackCooldown = enemyData.attackCooldown;
            biomassDropAmount = Random.Range(enemyData.biomassDropMin, enemyData.biomassDropMax + 1);
            
            // Устанавливаем здоровье
            if (healthComponent != null) {
                healthComponent.maxHealth = enemyData.maxHealth;
                healthComponent.currentHealth = enemyData.maxHealth;
            }
            
            // Визуальные настройки
            if (spriteRenderer != null) {
                spriteRenderer.color = enemyData.tintColor;
            }
            transform.localScale = Vector3.one * enemyData.scale;
            
            Debug.Log($"[EnemyAI] {enemyData.enemyName} spawned! HP: {enemyData.maxHealth}, Speed: {moveSpeed}, Damage: {attackDamage}");
        } else {
            Debug.LogError($"[EnemyAI] No EnemyData assigned to {gameObject.name}!");
        }

        if (animator == null) {
            Debug.LogWarning($"[EnemyAI] No Animator on {gameObject.name}!");
        }

        // Подписываемся на событие смерти
        if (healthComponent != null) {
            healthComponent.OnHealthChanged += OnHealthChanged;
        }

        // Ищем первую цель по приоритету
        FindBestTarget();
        
        // Инициализируем таймер атаки
        attackTimer = attackCooldown;
        
        // Начинаем с Idle анимации
        SetIdleAnimation();
    }

    void Update() {
        if (isDead) return; // Мертвый враг не двигается
        
        // Периодически переоцениваем цель
        retargetTimer -= Time.deltaTime;
        if (retargetTimer <= 0f) {
            FindBestTarget();
            retargetTimer = retargetInterval;
        }
        
        if (target == null) {
            SetIdleAnimation();
            return;
        }

        // Рассчитываем расстояние до цели
        distanceToTarget = Vector2.Distance(transform.position, target.position);

        // Если это стреляющий враг и цель в пределах дальности - стреляем
        if (enemyData != null && enemyData.canShoot && distanceToTarget <= enemyData.shootRange) {
            isShooting = true;
            isAttacking = false; // Не идём в ближний бой
            SetIdleAnimation();
            
            // Логика стрельбы
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f) {
                ShootProjectile();
                shootTimer = enemyData.shootCooldown;
            }
            
            // Поворачиваем в сторону цели
            Vector2 direction = (target.position - transform.position).normalized;
            FaceDirection(direction);
            
            return;
        }

        // Если в режиме ближней атаки - не двигаемся
        if (isAttacking) {
            // НЕ вызываем SetIdleAnimation() здесь - пусть играет анимация атаки!
            return;
        }

        // Двигаемся к цели
        Vector2 moveDirection = (target.position - transform.position).normalized;
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.deltaTime);
        
        // Поворачиваем муравья в сторону движения
        FaceDirection(moveDirection);
        
        // Включаем анимацию ходьбы
        SetWalkAnimation();
    }

    void OnTriggerEnter2D(Collider2D collision) {
        // Если наткнулся на цель - начинаем атаковать
        if (collision.CompareTag("Base") || 
            collision.CompareTag("Turret") || 
            collision.CompareTag("Barricade") ||
            collision.CompareTag("Player") ||
            collision.CompareTag("Farm")) {
            
            isAttacking = true;
            target = collision.transform;
            SetIdleAnimation(); // Останавливаемся
        }
    }

    void OnTriggerStay2D(Collider2D collision) {
        // Атакуем объект
        if (isAttacking && (collision.CompareTag("Base") || 
                            collision.CompareTag("Turret") || 
                            collision.CompareTag("Barricade") ||
                            collision.CompareTag("Player") ||
                            collision.CompareTag("Farm"))) {
            
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f) {
                Attack(collision.gameObject);
                attackTimer = attackCooldown;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        // Если цель уничтожена или ушли - продолжаем движение
        if (collision.transform == target) {
            isAttacking = false;
            SetIdleAnimation();
            
            // Ищем новую цель по приоритету
            FindBestTarget();
        }
    }

    void Attack(GameObject targetObject) {
        // Триггерим анимацию атаки
        SetAttackAnimation();
        
        Health targetHealth = targetObject.GetComponent<Health>();
        
        if (targetHealth != null) {
            targetHealth.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} attacked {targetObject.name} for {attackDamage} damage!");
        }
    }

    void OnHealthChanged(float healthPercent) {
        // Анимация получения урона
        if (!isDead) {
            SetHurtAnimation();
        }
    }

    void OnDestroy() {
        // Дропаем биомассу при смерти
        if (ResourceManager.Instance != null && isDead) {
            ResourceManager.Instance.AddBiomass(biomassDropAmount);
            Debug.Log($"[EnemyAI] {gameObject.name} dropped {biomassDropAmount} biomass!");
        }
    }
    
    // === СИСТЕМА ПРИОРИТЕТОВ ЦЕЛЕЙ ===
    
    void FindBestTarget() {
        // Гоблины атакуют только фермы
        if (enemyData != null && enemyData.enemyType == EnemyType.Goblin) {
            FindNearestFarm();
            return;
        }
        
        // Приоритет 1: Баррикада на пути (в радиусе 5 юнитов)
        GameObject[] barricades = GameObject.FindGameObjectsWithTag("Barricade");
        GameObject closestBarricade = null;
        float closestBarricadeDistance = 5f; // Радиус обнаружения баррикады
        
        foreach (GameObject barricade in barricades) {
            if (barricade == null) continue;
            
            float distance = Vector2.Distance(transform.position, barricade.transform.position);
            if (distance < closestBarricadeDistance) {
                closestBarricadeDistance = distance;
                closestBarricade = barricade;
            }
        }
        
        if (closestBarricade != null) {
            target = closestBarricade.transform;
            Debug.Log($"[EnemyAI] {enemyData.enemyName} targeting barricade at distance {closestBarricadeDistance}");
            return;
        }
        
        // Приоритет 2: Игрок (если жив)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null && playerHealth.IsAlive()) {
                target = player.transform;
                Debug.Log($"[EnemyAI] {enemyData.enemyName} targeting player");
                return;
            }
        }
        
        // Приоритет 3: Холодильник (база)
        GameObject baseObject = GameObject.FindGameObjectWithTag("Base");
        if (baseObject != null) {
            target = baseObject.transform;
            Debug.Log($"[EnemyAI] {enemyData.enemyName} targeting base (fridge)");
        } else {
            Debug.LogWarning("[EnemyAI] No valid target found!");
        }
    }
    
    void FindNearestFarm() {
        GameObject[] farms = GameObject.FindGameObjectsWithTag("Farm");
        GameObject closestFarm = null;
        float closestDistance = Mathf.Infinity;
        
        foreach (GameObject farm in farms) {
            if (farm == null) continue;
            
            float distance = Vector2.Distance(transform.position, farm.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestFarm = farm;
            }
        }
        
        if (closestFarm != null) {
            target = closestFarm.transform;
            Debug.Log($"[EnemyAI] Goblin targeting farm at distance {closestDistance}");
        } else {
            Debug.LogWarning("[EnemyAI] Goblin found no farms! Idling...");
            target = null;
        }
    }
    
    // === МЕТОД СТРЕЛЬБЫ ===
    
    void ShootProjectile() {
        if (enemyData == null || enemyData.projectilePrefab == null) return;
        
        SetAttackAnimation(); // Анимация атаки
        
        // Создаём снаряд
        GameObject projectile = Instantiate(enemyData.projectilePrefab, transform.position, Quaternion.identity);
        
        // Настраиваем снаряд
        EnemyProjectile enemyProj = projectile.GetComponent<EnemyProjectile>();
        if (enemyProj != null) {
            enemyProj.damage = attackDamage;
            enemyProj.SetTarget(target.position);
        }
        
        Debug.Log($"[EnemyAI] {enemyData.enemyName} shot projectile at {target.name}!");
    }
    
    // === МЕТОДЫ АНИМАЦИИ ===
    
    void FaceDirection(Vector2 direction) {
        if (spriteRenderer == null) return;
        
        // Определяем направление по оси X
        if (direction.x > 0.1f) {
            // Движется вправо - переворачиваем (спрайт смотрит влево изначально)
            spriteRenderer.flipX = true;
        } else if (direction.x < -0.1f) {
            // Движется влево - нормальное отображение
            spriteRenderer.flipX = false;
        }
        // Если direction.x близко к 0, оставляем текущий поворот
    }
    
    void SetIdleAnimation() {
        if (animator != null) {
            animator.SetBool("isWalking", false);
        }
    }
    
    void SetWalkAnimation() {
        if (animator != null) {
            animator.SetBool("isWalking", true);
        }
    }
    
    void SetAttackAnimation() {
        if (animator != null) {
            animator.SetTrigger("Attack");
        }
    }
    
    void SetHurtAnimation() {
        if (animator != null) {
            animator.SetTrigger("Hurt");
        }
    }
    
    public void Die() {
        if (isDead) return;
        
        isDead = true;
        
        if (animator != null) {
            animator.SetTrigger("Die");
        }
        
        // Отключаем коллайдер и движение
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        // Удаляем через 2 секунды (чтобы анимация смерти доиграла)
        Destroy(gameObject, 2f);
    }
}
