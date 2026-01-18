using UnityEngine;

// Кот-помощник - союзный юнит, который патрулирует и атакует врагов
public class AllyUnit : Building
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float patrolRadius = 8f;
    public Transform patrolCenter; // Центр патруля (обычно место спавна)
    
    [Header("Combat")]
    public float attackRange = 6f;
    public float damage = 8f;
    public float fireRate = 0.8f;
    private float fireTimer;
    
    [Header("Visual")]
    public Transform firePoint;
    public GameObject projectilePrefab;
    
    private Rigidbody2D rb;
    private Transform currentTarget;
    private Vector2 patrolTarget;
    private bool isPatrolling = true;

    protected override void Start() {
        base.Start();
        
        buildingType = BuildingType.AllyUnit;
        rb = GetComponent<Rigidbody2D>();
        
        // Центр патруля - место спавна
        if (patrolCenter == null) {
            GameObject center = new GameObject($"{gameObject.name}_PatrolCenter");
            center.transform.position = transform.position;
            patrolCenter = center.transform;
        }
        
        SetNewPatrolPoint();
    }

    void Update() {
        // Проверяем фазу игры
        if (GameManager.Instance == null || GameManager.Instance.CurrentPhase != GameManager.GamePhase.ActionPhase) {
            // Во время PrepPhase просто стоим
            return;
        }

        // Ищем врагов
        FindClosestEnemy();

        if (currentTarget != null) {
            // Режим атаки
            isPatrolling = false;
            AttackEnemy();
        } else {
            // Режим патруля
            if (!isPatrolling) {
                SetNewPatrolPoint();
                isPatrolling = true;
            }
            Patrol();
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

        // Если старая цель вышла из радиуса, ищем новую
        if (currentTarget != null) {
            float distToCurrentTarget = Vector2.Distance(transform.position, currentTarget.position);
            if (distToCurrentTarget > attackRange) {
                currentTarget = null;
            }
        }

        if (closestEnemy != null) {
            currentTarget = closestEnemy;
        }
    }

    void AttackEnemy() {
        if (currentTarget == null) return;

        // Останавливаемся
        rb.linearVelocity = Vector2.zero;

        // Поворачиваемся к врагу
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 если спрайт смотрит вверх

        // Стреляем
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f) {
            Fire();
            fireTimer = 1f / fireRate;
        }
    }

    void Fire() {
        if (currentTarget == null) return;

        if (projectilePrefab != null) {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null) {
                proj.SetTarget(currentTarget, damage);
            }
            
            Debug.Log($"AllyUnit fired at {currentTarget.name}!");
        }
    }

    void Patrol() {
        // Двигаемся к точке патруля
        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // Поворачиваемся в сторону движения
        if (direction != Vector2.zero) {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }

        // Если достигли точки - выбираем новую
        if (Vector2.Distance(transform.position, patrolTarget) < 0.5f) {
            SetNewPatrolPoint();
        }
    }

    void SetNewPatrolPoint() {
        // Случайная точка в радиусе патруля
        Vector2 randomPoint = Random.insideUnitCircle * patrolRadius;
        patrolTarget = (Vector2)patrolCenter.position + randomPoint;
    }

    public override void OnPlaced() {
        base.OnPlaced();
        Debug.Log("Ally Unit deployed! Will patrol and attack enemies.");
    }

    void OnDrawGizmosSelected() {
        // Радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Радиус патруля
        Gizmos.color = Color.blue;
        if (patrolCenter != null) {
            Gizmos.DrawWireSphere(patrolCenter.position, patrolRadius);
        }
    }
}
