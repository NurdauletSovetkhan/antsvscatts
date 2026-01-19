using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    // События
    public static event Action<GameObject> OnDeath; // Уведомляет о смерти объекта
    public event Action<float> OnHealthChanged; // Для UI (healthbars)

    void Start() {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage) {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth); // Не уходим ниже 0

        Debug.Log($"{gameObject.name} took {damage} damage! HP: {currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(currentHealth / maxHealth); // Процент HP

        if (currentHealth <= 0f) {
            Die();
        }
    }

    public void Heal(float amount) {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Не выше максимума
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    void Die() {
        Debug.Log($"{gameObject.name} died!");
        OnDeath?.Invoke(gameObject);

        // Специальная логика для базы (холодильника)
        if (CompareTag("Base")) {
            GameManager.Instance?.TriggerGameOver();
        }

        // Если это враг - вызываем его метод Die() для анимации
        EnemyAI enemy = GetComponent<EnemyAI>();
        if (enemy != null) {
            enemy.Die();
            return; // EnemyAI сам удалит объект
        }

        Destroy(gameObject);
    }

    public float GetHealthPercentage() {
        return currentHealth / maxHealth;
    }

    public bool IsAlive() {
        return currentHealth > 0f;
    }
}
