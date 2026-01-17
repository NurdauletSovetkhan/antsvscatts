using UnityEngine;

public class ResourcePot : MonoBehaviour
{
    [Header("Resource Generation")]
    public int minResourcesPerNight = 2;
    public int maxResourcesPerNight = 5;
    
    [Header("Visual Feedback")]
    public GameObject resourceIndicatorPrefab; // Визуальный индикатор ресурса
    public Transform resourceSpawnPoint; // Точка появления ресурса над горшком
    public SpriteRenderer potRenderer; // Для изменения цвета горшка
    public Color emptyColor = Color.gray;
    public Color fullColor = Color.green;
    
    private int storedResources = 0;
    private bool hasGeneratedTonight = false;

    void OnEnable() {
        GameManager.OnPhaseChanged += OnPhaseChanged;
    }

    void OnDisable() {
        GameManager.OnPhaseChanged -= OnPhaseChanged;
    }

    void OnPhaseChanged(GameManager.GamePhase newPhase) {
        // Генерируем ресурсы в конце ночи (начало дня)
        if (newPhase == GameManager.GamePhase.PrepPhase) {
            if (!hasGeneratedTonight) {
                GenerateResources();
                hasGeneratedTonight = true;
            }
        }
        
        // Сбрасываем флаг в начале ночи
        if (newPhase == GameManager.GamePhase.ActionPhase) {
            hasGeneratedTonight = false;
        }
    }

    void Start() {
        // Генерируем первую партию ресурсов сразу
        GenerateResources();
    }

    void GenerateResources() {
        int generatedAmount = Random.Range(minResourcesPerNight, maxResourcesPerNight + 1);
        storedResources += generatedAmount;
        
        Debug.Log($"{gameObject.name} generated {generatedAmount} resources! Total: {storedResources}");
        
        UpdateVisuals();
    }
    
    void UpdateVisuals() {
        // Меняем цвет горшка
        if (potRenderer != null) {
            potRenderer.color = storedResources > 0 ? fullColor : emptyColor;
        }
        
        // Создаем визуальный индикатор
        if (resourceIndicatorPrefab != null && resourceSpawnPoint != null && storedResources > 0) {
            // Удаляем старые индикаторы
            foreach (Transform child in resourceSpawnPoint) {
                Destroy(child.gameObject);
            }
            
            // Создаем новый
            Instantiate(resourceIndicatorPrefab, resourceSpawnPoint.position, Quaternion.identity, resourceSpawnPoint);
        }
    }

    // Вызывается игроком при сборе
    public int CollectResources() {
        if (storedResources <= 0) {
            Debug.Log($"{gameObject.name} has no resources to collect!");
            return 0;
        }
        
        int amount = storedResources;
        storedResources = 0;
        
        Debug.Log($"Collected {amount} resources from {gameObject.name}");
        
        UpdateVisuals();
        
        return amount;
    }

    public bool HasResources() {
        return storedResources > 0;
    }

    public int GetStoredAmount() {
        return storedResources;
    }

    // Визуализация в редакторе
    void OnDrawGizmos() {
        if (storedResources > 0) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
