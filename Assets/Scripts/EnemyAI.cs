using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    private Transform target; // Цель (обычно холодильник)

    [Header("Attack")]
    public float attackDamage = 5f;
    public float attackCooldown = 1f;
    private float attackTimer;

    [Header("Drops")]
    public int biomassDropAmount = 3;

    private Rigidbody2D rb;
    private Health healthComponent;
    private bool isAttacking = false;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        healthComponent = GetComponent<Health>();

        // Подписываемся на событие смерти
        if (healthComponent != null) {
            healthComponent.OnHealthChanged += OnHealthChanged;
        }

        // Ищем базу (холодильник)
        GameObject baseObject = GameObject.FindGameObjectWithTag("Base");
        if (baseObject != null) {
            target = baseObject.transform;
        } else {
            Debug.LogWarning("Base not found! Enemy has no target.");
        }
    }

    void Update() {
        if (target == null || isAttacking) return;

        // Двигаемся к цели
        Vector2 direction = (target.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision) {
        // Если наткнулся на базу или турель - начинаем атаковать
        if (collision.CompareTag("Base") || collision.CompareTag("Turret")) {
            isAttacking = true;
            target = collision.transform;
        }
    }

    void OnTriggerStay2D(Collider2D collision) {
        // Атакуем объект
        if (isAttacking && (collision.CompareTag("Base") || collision.CompareTag("Turret"))) {
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
            
            // Находим новую цель (базу)
            GameObject baseObject = GameObject.FindGameObjectWithTag("Base");
            if (baseObject != null) {
                target = baseObject.transform;
            }
        }
    }

    void Attack(GameObject targetObject) {
        Health targetHealth = targetObject.GetComponent<Health>();
        
        if (targetHealth != null) {
            targetHealth.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} attacked {targetObject.name} for {attackDamage} damage!");
        }
    }

    void OnHealthChanged(float healthPercent) {
        // Можно добавить визуальный эффект урона
    }

    void OnDestroy() {
        // Дропаем биомассу при смерти
        if (ResourceManager.Instance != null) {
            ResourceManager.Instance.AddBiomass(biomassDropAmount);
        }
    }
}
