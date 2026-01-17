using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GamePhase { PrepPhase, ActionPhase, GameOver }
    public GamePhase CurrentPhase { get; private set; }

    [Header("Phase Settings")]
    public float prepPhaseDuration = 60f; // Фаза подготовки 60 секунд
    private float phaseTimer;

    [Header("Day Counter")]
    public int currentDay = 1;

    [Header("Day/Night Visuals")]
    public UnityEngine.Rendering.Universal.Light2D globalLight; // Global Light 2D
    public Color dayColor = new Color(1f, 1f, 1f, 1f); // Белый свет
    public Color nightColor = new Color(0.3f, 0.3f, 0.5f, 1f); // Темно-синий

    // События для других скриптов
    public static event Action<GamePhase> OnPhaseChanged;
    public static event Action OnGameOver;

    void Awake() {
        // Singleton pattern
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        StartPrepPhase();
    }

    void Update() {
        switch (CurrentPhase) {
            case GamePhase.PrepPhase:
                UpdatePrepPhase();
                break;
            case GamePhase.ActionPhase:
                // Логика фазы атаки (пока пустая)
                break;
            case GamePhase.GameOver:
                // Игра окончена
                break;
        }
    }

    void StartPrepPhase() {
        CurrentPhase = GamePhase.PrepPhase;
        phaseTimer = prepPhaseDuration;
        
        // Визуал: День
        SetDayVisuals(true);
        
        // Скрываем всех врагов
        HideAllEnemies();
        
        OnPhaseChanged?.Invoke(CurrentPhase);
        Debug.Log($"[Day {currentDay}] PrepPhase started! ({prepPhaseDuration} sec)");
    }

    void UpdatePrepPhase() {
        phaseTimer -= Time.deltaTime;

        if (phaseTimer <= 0f) {
            StartActionPhase();
        }
    }

    void StartActionPhase() {
        CurrentPhase = GamePhase.ActionPhase;
        
        // Визуал: Ночь
        SetDayVisuals(false);
        
        // Показываем врагов (их заспавнит EnemySpawner)
        
        OnPhaseChanged?.Invoke(CurrentPhase);
        Debug.Log($"[Day {currentDay}] ActionPhase started! Enemies incoming!");
    }

    // Вызывается, когда база уничтожена
    public void TriggerGameOver() {
        CurrentPhase = GamePhase.GameOver;
        OnGameOver?.Invoke();
        Debug.Log("GAME OVER!");
    }

    // Вызывается, когда волна врагов побеждена
    public void CompleteDay() {
        currentDay++;
        StartPrepPhase(); // Начинаем новый день
    }

    // Публичный метод для UI (получение оставшегося времени)
    public float GetPhaseTimeRemaining() {
        return Mathf.Max(0f, phaseTimer);
    }

    void SetDayVisuals(bool isDay) {
        if (globalLight != null) {
            globalLight.color = isDay ? dayColor : nightColor;
            globalLight.intensity = isDay ? 1f : 0.6f;
        }
    }

    void HideAllEnemies() {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies) {
            enemy.SetActive(false); // Скрываем врага
        }
    }

    public bool IsDay() {
        return CurrentPhase == GamePhase.PrepPhase;
    }
}
