using UnityEngine;

// Баррикада - простая стена для блокировки врагов
public class Barricade : Building
{
    [Header("Barricade Data")]
    public BarricadeData barricadeData;
    
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

        // Устанавливаем характеристики из BarricadeData ПЕРЕД Health.Start()
        // Используем Awake для гарантии порядка выполнения
        InitializeFromData();
    }
    
    void InitializeFromData() {
        if (barricadeData != null) {
            if (healthComponent != null) {
                healthComponent.maxHealth = barricadeData.maxHealth;
                healthComponent.currentHealth = barricadeData.maxHealth; // Устанавливаем сразу
            }
            
            // Визуальные настройки
            if (spriteRenderer != null) {
                if (barricadeData.sprite != null) {
                    spriteRenderer.sprite = barricadeData.sprite;
                }
                spriteRenderer.color = barricadeData.tintColor;
            }
            
            transform.localScale = Vector3.one * barricadeData.scale;
            
            Debug.Log($"[Barricade] {barricadeData.barricadeName} placed! HP: {barricadeData.maxHealth}");
        } else {
            // Fallback на старые настройки если нет BarricadeData
            if (healthComponent != null) {
                healthComponent.maxHealth = 200f;
                healthComponent.currentHealth = 200f;
            }
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
