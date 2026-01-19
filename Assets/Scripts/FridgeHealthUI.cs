using UnityEngine;
using TMPro;

public class FridgeHealthUI : MonoBehaviour
{
    [Header("References")]
    public GameObject fridgeObject; // Объект холодильника с тегом "Base"
    public TextMeshProUGUI healthText;
    
    private Health fridgeHealth;

    void Start() {
        // Находим холодильник
        if (fridgeObject == null) {
            fridgeObject = GameObject.FindGameObjectWithTag("Base");
        }
        
        if (fridgeObject != null) {
            fridgeHealth = fridgeObject.GetComponent<Health>();
            
            if (fridgeHealth != null) {
                // Подписываемся на изменение здоровья
                fridgeHealth.OnHealthChanged += UpdateHealthDisplay;
                
                // Первичное обновление
                UpdateHealthDisplay(fridgeHealth.GetHealthPercentage());
            } else {
                Debug.LogError("[FridgeHealthUI] Fridge has no Health component!");
            }
        } else {
            Debug.LogError("[FridgeHealthUI] Fridge not found! Make sure it has 'Base' tag.");
        }
    }

    void UpdateHealthDisplay(float healthPercent) {
        if (fridgeHealth == null || healthText == null) return;
        
        float current = fridgeHealth.currentHealth;
        float max = fridgeHealth.maxHealth;
        
        healthText.text = $"Fish Left: {Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
        
        // Меняем цвет в зависимости от HP
        if (healthPercent > 0.5f) {
            healthText.color = Color.green;
        } else if (healthPercent > 0.25f) {
            healthText.color = Color.yellow;
        } else {
            healthText.color = Color.red;
        }
    }

    void OnDestroy() {
        // Отписываемся от события
        if (fridgeHealth != null) {
            fridgeHealth.OnHealthChanged -= UpdateHealthDisplay;
        }
    }
}
