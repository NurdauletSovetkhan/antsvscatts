using UnityEngine;

public class Farm : MonoBehaviour
{
    [Header("Biomass Generation")]
    public int minBiomassPerNight = 2;
    public int maxBiomassPerNight = 5;
    
    [Header("Visual Feedback")]
    public GameObject biomassIndicatorPrefab; // Визуальный индикатор биомассы
    public Transform biomassSpawnPoint; // Точка появления индикатора над фермой
    public SpriteRenderer farmRenderer; // Для изменения цвета фермы
    public Color emptyColor = Color.gray;
    public Color fullColor = Color.green;
    
    private int storedBiomass = 0;
    private bool hasGeneratedTonight = false;

    void OnEnable() {
        GameManager.OnPhaseChanged += OnPhaseChanged;
    }

    void OnDisable() {
        GameManager.OnPhaseChanged -= OnPhaseChanged;
    }

    void OnPhaseChanged(GameManager.GamePhase newPhase) {
        // Генерируем биомассу в конце ночи (начало дня)
        if (newPhase == GameManager.GamePhase.PrepPhase) {
            if (!hasGeneratedTonight) {
                GenerateBiomass();
                hasGeneratedTonight = true;
            }
        }
        
        // Сбрасываем флаг в начале ночи
        if (newPhase == GameManager.GamePhase.ActionPhase) {
            hasGeneratedTonight = false;
        }
    }

    void Start() {
        // Генерируем первую партию биомассы сразу
        GenerateBiomass();
    }

    void GenerateBiomass() {
        int generatedAmount = Random.Range(minBiomassPerNight, maxBiomassPerNight + 1);
        storedBiomass += generatedAmount;
        
        Debug.Log($"[Farm] {gameObject.name} generated {generatedAmount} biomass! Total: {storedBiomass}");
        
        UpdateVisuals();
    }
    
    void UpdateVisuals() {
        // Меняем цвет фермы
        if (farmRenderer != null) {
            farmRenderer.color = storedBiomass > 0 ? fullColor : emptyColor;
        }
        
        // Создаем визуальный индикатор
        if (biomassIndicatorPrefab != null && biomassSpawnPoint != null && storedBiomass > 0) {
            // Удаляем старые индикаторы
            foreach (Transform child in biomassSpawnPoint) {
                Destroy(child.gameObject);
            }
            
            // Создаем новый
            Instantiate(biomassIndicatorPrefab, biomassSpawnPoint.position, Quaternion.identity, biomassSpawnPoint);
        }
    }

    // Вызывается игроком при сборе
    public int CollectBiomass() {
        if (storedBiomass <= 0) {
            Debug.Log($"[Farm] {gameObject.name} has no biomass to collect!");
            return 0;
        }
        
        int amount = storedBiomass;
        storedBiomass = 0;
        
        Debug.Log($"[Farm] Collected {amount} biomass from {gameObject.name}");
        
        UpdateVisuals();
        
        return amount;
    }

    public bool HasBiomass() {
        return storedBiomass > 0;
    }

    public int GetStoredAmount() {
        return storedBiomass;
    }

    // Визуализация в редакторе
    void OnDrawGizmos() {
        if (storedBiomass > 0) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
