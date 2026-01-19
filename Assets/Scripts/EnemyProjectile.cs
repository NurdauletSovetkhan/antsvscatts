using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float damage = 10f;
    public float speed = 8f;
    public float lifetime = 5f;
    
    private Vector2 direction;
    private Rigidbody2D rb;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    void Update() {
        // Двигаем снаряд
        if (rb != null) {
            rb.linearVelocity = direction * speed;
        } else {
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    public void SetTarget(Vector2 targetPosition) {
        // Вычисляем направление к цели
        direction = (targetPosition - (Vector2)transform.position).normalized;
        
        // Поворачиваем снаряд в сторону цели
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D collision) {
        // Попадаем в турель или базу
        if (collision.CompareTag("Turret") || collision.CompareTag("Base")) {
            Health targetHealth = collision.GetComponent<Health>();
            
            if (targetHealth != null) {
                targetHealth.TakeDamage(damage);
                Debug.Log($"[EnemyProjectile] Hit {collision.name} for {damage} damage!");
            }
            
            Destroy(gameObject);
        }
    }
}
