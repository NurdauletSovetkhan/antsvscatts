using UnityEngine;

// Простой скрипт для спавна волн врагов
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs; // Массив разных типов врагов
    public Transform[] spawnPoints; // Массив точек спавна
    public int enemiesPerWave = 10;
    
    [Header("Spawn Timing")]
    public float minSpawnInterval = 0.5f; // Минимальный интервал
    public float maxSpawnInterval = 2f;   // Максимальный интервал
    public float spawnRadius = 10f; // Радиус спавна от этого объекта, если нет точек

    private int enemiesSpawned = 0;
    private float spawnTimer;
    private bool isSpawning = false;

    void OnEnable() {
        // Подписываемся на смену фаз
        GameManager.OnPhaseChanged += OnPhaseChanged;
    }

    void OnDisable() {
        GameManager.OnPhaseChanged -= OnPhaseChanged;
    }

    void OnPhaseChanged(GameManager.GamePhase newPhase) {
        if (newPhase == GameManager.GamePhase.ActionPhase) {
            StartWave();
        }
    }

    void StartWave() {
        isSpawning = true;
        enemiesSpawned = 0;
        spawnTimer = 0f;
        Debug.Log($"Starting wave with {enemiesPerWave} enemies!");
    }

    void Update() {
        if (!isSpawning) return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f && enemiesSpawned < enemiesPerWave) {
            SpawnEnemy();
            enemiesSpawned++;
            
            // Устанавливаем случайный интервал для следующего спавна
            spawnTimer = Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        // Проверяем, закончилась ли волна
        if (enemiesSpawned >= enemiesPerWave && GameObject.FindGameObjectsWithTag("Enemy").Length == 0) {
            EndWave();
        }
    }

    void SpawnEnemy() {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) {
            Debug.LogWarning("Enemy Prefabs array is empty!");
            return;
        }

        // Выбираем случайного врага из массива
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        Vector3 spawnPosition;

        // Если есть точки спавна - используем их
        if (spawnPoints != null && spawnPoints.Length > 0) {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            spawnPosition = spawnPoint.position;
        } else {
            // Иначе спавним вокруг этого объекта
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
        }

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.SetActive(true); // Убедимся, что враг активен
        Debug.Log($"Spawned enemy at {spawnPosition}");
    }

    void EndWave() {
        isSpawning = false;
        Debug.Log("Wave completed!");
        GameManager.Instance?.CompleteDay();
    }
}
