using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Turret Stats")]
    public float attackRange = 5f;
    public float damage = 10f;
    public float fireRate = 1f; // Выстрелов в секунду
    private float fireTimer;

    [Header("Visual")]
    public Transform firePoint; // Точка, откуда летит снаряд
    public GameObject projectilePrefab; // Префаб снаряда (опционально)

    private Transform currentTarget;
    private Health healthComponent;

    void Start() {
        healthComponent = GetComponent<Health>();
        fireTimer = 1f / fireRate;
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
            Fire();
            fireTimer = 1f / fireRate;
        }
    }

    void Fire() {
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
