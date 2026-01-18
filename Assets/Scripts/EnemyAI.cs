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

    [Header("Animation")]
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Health healthComponent;
    private bool isAttacking = false;
    private bool isDead = false;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        healthComponent = GetComponent<Health>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null) {
            Debug.LogWarning($"[EnemyAI] No Animator on {gameObject.name}!");
        }

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
        
        // Начинаем с Idle анимации
        SetIdleAnimation();
    }

    void Update() {
        if (isDead) return; // Мертвый враг не двигается
        
        if (target == null || isAttacking) {
            SetIdleAnimation();
            return;
        }

        // Двигаемся к цели
        Vector2 direction = (target.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
        
        // Поворачиваем муравья в сторону движения
        FaceDirection(direction);
        
        // Включаем анимацию ходьбы
        SetWalkAnimation();
    }

    void OnTriggerEnter2D(Collider2D collision) {
        // Если наткнулся на базу или турель - начинаем атаковать
        if (collision.CompareTag("Base") || collision.CompareTag("Turret")) {
            isAttacking = true;
            target = collision.transform;
            SetIdleAnimation(); // Останавливаемся
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
            SetIdleAnimation();
            
            // Находим новую цель (базу)
            GameObject baseObject = GameObject.FindGameObjectWithTag("Base");
            if (baseObject != null) {
                target = baseObject.transform;
            }
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
        if (ResourceManager.Instance != null && !isDead) {
            ResourceManager.Instance.AddBiomass(biomassDropAmount);
        }
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
