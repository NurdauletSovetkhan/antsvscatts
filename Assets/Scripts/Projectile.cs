using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public float damage = 10f;
    public float lifetime = 5f; // Время жизни снаряда

    private Transform target;
    private Vector2 direction;
    private bool hasTarget = false;

    void Start() {
        // Уничтожаем снаряд через заданное время
        Destroy(gameObject, lifetime);
    }

    public void SetTarget(Transform targetTransform, float damageAmount) {
        target = targetTransform;
        damage = damageAmount;
        hasTarget = true;

        if (target != null) {
            // Вычисляем направление к цели
            direction = (target.position - transform.position).normalized;
            
            // Поворачиваем снаряд в сторону цели
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void Update() {
        if (!hasTarget) return;

        // Двигаем снаряд
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // Проверяем, достигли ли цели (если она еще жива)
        if (target != null) {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            
            // Обновляем направление для самонаведения (опционально)
            // direction = (target.position - transform.position).normalized;
            
            if (distanceToTarget < 0.2f) {
                HitTarget();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        // Попадание в любой объект с тегом Enemy
        if (collision.CompareTag("Enemy")) {
            Health enemyHealth = collision.GetComponent<Health>();
            if (enemyHealth != null) {
                enemyHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }

    void HitTarget() {
        if (target != null) {
            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth != null) {
                targetHealth.TakeDamage(damage);
            }
        }
        
        // Можно добавить эффект взрыва
        Destroy(gameObject);
    }
}
