using UnityEngine;
using TMPro;

// Простой UI менеджер для отображения информации
public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI biomassText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI dayText;

    void OnEnable() {
        // Подписываемся на события
        ResourceManager.OnBiomassChanged += UpdateBiomassUI;
        GameManager.OnPhaseChanged += UpdatePhaseUI;
    }

    void OnDisable() {
        ResourceManager.OnBiomassChanged -= UpdateBiomassUI;
        GameManager.OnPhaseChanged -= UpdatePhaseUI;
    }

    void Update() {
        UpdateTimerUI();
        UpdateDayUI();
    }

    void UpdateBiomassUI(int amount) {
        if (biomassText != null) {
            biomassText.text = $"Biomass: {amount}";
        }
    }

    void UpdateTimerUI() {
        if (timerText != null && GameManager.Instance != null) {
            float timeRemaining = GameManager.Instance.GetPhaseTimeRemaining();
            timerText.text = $"Time: {Mathf.CeilToInt(timeRemaining)}s";
        }
    }

    void UpdatePhaseUI(GameManager.GamePhase phase) {
        if (phaseText != null) {
            switch (phase) {
                case GameManager.GamePhase.PrepPhase:
                    phaseText.text = "DAY - PREPARATION PHASE";
                    phaseText.color = new Color(1f, 0.8f, 0f); // Золотой
                    break;
                case GameManager.GamePhase.ActionPhase:
                    phaseText.text = "NIGHT - INVASION!!";
                    phaseText.color = new Color(1f, 0f, 0f); // Красный
                    break;
                case GameManager.GamePhase.GameOver:
                    phaseText.text = "☠ GAME OVER ☠";
                    phaseText.color = Color.black;
                    break;
            }
        }
    }

    void UpdateDayUI() {
        if (dayText != null && GameManager.Instance != null) {
            dayText.text = $"Day {GameManager.Instance.currentDay}";
        }
    }
}
