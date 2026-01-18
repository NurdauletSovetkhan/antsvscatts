using UnityEngine;

// Баррикада - простая стена для блокировки врагов
public class Barricade : Building
{
    [Header("Barricade Settings")]
    public float maxHealth = 200f;
    
    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color normalColor = Color.gray;
    public Color damagedColor = new Color(0.5f, 0.3f, 0.3f);

    protected override void Start() {
        base.Start();
        
        buildingType = BuildingType.Barricade;
        
        if (spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Устанавливаем здоровье
        if (healthComponent != null) {
            healthComponent.maxHealth = maxHealth;
        }
    }

    void OnEnable() {
        if (healthComponent != null) {
            healthComponent.OnHealthChanged += OnHealthChanged;
        }
    }

    void OnDisable() {
        if (healthComponent != null) {
            healthComponent.OnHealthChanged -= OnHealthChanged;
        }
    }

    void OnHealthChanged(float healthPercent) {
        // Меняем цвет в зависимости от HP
        if (spriteRenderer != null) {
            spriteRenderer.color = Color.Lerp(damagedColor, normalColor, healthPercent);
        }
    }

    public override void OnPlaced() {
        base.OnPlaced();
        Debug.Log("Barricade placed! Blocks enemy movement.");
    }
}
