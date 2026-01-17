using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [Header("Resource Settings")]
    public GameObject resourcePrefab;
    public Transform[] resourceSpawnPoints; // Фиксированные точки спавна
    
    [Header("Random Spawn Settings")]
    public bool useRandomSpawn = true;
    public int resourcesPerDay = 5;
    public float spawnRadius = 15f; // Радиус спавна от этого объекта
    public Vector2 spawnAreaMin; // Минимальные координаты области
    public Vector2 spawnAreaMax; // Максимальные координаты области

    [Header("Resource Drop")]
    public GameObject resourceDropPrefab; // Префаб дропа с врагов
    public int dropChancePercent = 50; // Шанс дропа в %

    void OnEnable() {
        GameManager.OnPhaseChanged += OnPhaseChanged;
        Health.OnDeath += OnEntityDeath;
    }

    void OnDisable() {
        GameManager.OnPhaseChanged -= OnPhaseChanged;
        Health.OnDeath -= OnEntityDeath;
    }

    void OnPhaseChanged(GameManager.GamePhase newPhase) {
        // Спавним ресурсы в начале дня
        if (newPhase == GameManager.GamePhase.PrepPhase) {
            SpawnDailyResources();
        }
    }

    void SpawnDailyResources() {
        if (resourcePrefab == null) {
            Debug.LogWarning("Resource Prefab is not assigned!");
            return;
        }

        int resourcesToSpawn = resourcesPerDay;

        // Используем фиксированные точки, если они есть и не включен рандом
        if (!useRandomSpawn && resourceSpawnPoints != null && resourceSpawnPoints.Length > 0) {
            resourcesToSpawn = Mathf.Min(resourcesToSpawn, resourceSpawnPoints.Length);
            
            for (int i = 0; i < resourcesToSpawn; i++) {
                SpawnResource(resourceSpawnPoints[i].position);
            }
        } else {
            // Спавним случайно
            for (int i = 0; i < resourcesToSpawn; i++) {
                Vector3 spawnPosition = GetRandomSpawnPosition();
                SpawnResource(spawnPosition);
            }
        }

        Debug.Log($"Spawned {resourcesToSpawn} resources for the day!");
    }

    Vector3 GetRandomSpawnPosition() {
        // Если заданы границы области
        if (spawnAreaMin != Vector2.zero || spawnAreaMax != Vector2.zero) {
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            return new Vector3(x, y, 0f);
        } else {
            // Иначе спавним вокруг этого объекта
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            return transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
        }
    }

    void SpawnResource(Vector3 position) {
        GameObject resource = Instantiate(resourcePrefab, position, Quaternion.identity);
        resource.tag = "Resource"; // Убедимся, что тег установлен
        resource.SetActive(true);
    }

    void OnEntityDeath(GameObject deadEntity) {
        // Дроп с убитых врагов
        if (deadEntity.CompareTag("Enemy") && resourceDropPrefab != null) {
            // Проверяем шанс дропа
            if (Random.Range(0, 100) < dropChancePercent) {
                Instantiate(resourceDropPrefab, deadEntity.transform.position, Quaternion.identity);
                Debug.Log($"Enemy dropped resource at {deadEntity.transform.position}");
            }
        }
    }

    // Визуализация области спавна
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        
        if (spawnAreaMin != Vector2.zero || spawnAreaMax != Vector2.zero) {
            // Рисуем прямоугольник области
            Vector3 center = new Vector3((spawnAreaMin.x + spawnAreaMax.x) / 2f, (spawnAreaMin.y + spawnAreaMax.y) / 2f, 0f);
            Vector3 size = new Vector3(spawnAreaMax.x - spawnAreaMin.x, spawnAreaMax.y - spawnAreaMin.y, 0f);
            Gizmos.DrawWireCube(center, size);
        } else {
            // Рисуем круг радиуса
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
