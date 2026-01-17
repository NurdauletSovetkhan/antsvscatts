using UnityEngine;
using UnityEngine.Rendering;

public class HorrorManager : MonoBehaviour
{
    public static HorrorManager Instance { get; private set; }

    [Header("Horror State")]
    public bool isHorrorMode = false;

    [Header("Visual Effects")]
    public Volume postProcessVolume; // URP Post-Processing Volume
    public Color horrorColor = new Color(0.29f, 0f, 0f); // #4A0000

    [Header("Gameplay Modifiers")]
    public float enemySpeedMultiplier = 1.5f;
    public float turretDamageMultiplier = 1.3f;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void TriggerHorrorMode() {
        if (isHorrorMode) return; // Уже активен

        isHorrorMode = true;
        Debug.Log("HORROR MODE ACTIVATED!");

        // Визуальные изменения
        ApplyHorrorVisuals();

        // Усиливаем врагов
        BuffEnemies();

        // Усиливаем турели
        BuffTurrets();
    }

    void ApplyHorrorVisuals() {
        // Включаем пост-обработку (Vignette, Chromatic Aberration)
        if (postProcessVolume != null) {
            postProcessVolume.weight = 1f; // Максимальная интенсивность
        }

        // Меняем цвет окружения (опционально - можно через Light2D)
        // RenderSettings.ambientLight = horrorColor;
    }

    void BuffEnemies() {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies) {
            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai != null) {
                ai.moveSpeed *= enemySpeedMultiplier;
            }

            // Меняем цвет спрайта на кроваво-красный
            SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
            if (sr != null) {
                sr.color = horrorColor;
            }
        }
    }

    void BuffTurrets() {
        GameObject[] turrets = GameObject.FindGameObjectsWithTag("Turret");

        foreach (GameObject turret in turrets) {
            Turret t = turret.GetComponent<Turret>();
            if (t != null) {
                t.damage *= turretDamageMultiplier;
            }

            // Меняем цвет спрайта
            SpriteRenderer sr = turret.GetComponent<SpriteRenderer>();
            if (sr != null) {
                sr.color = horrorColor;
            }
        }
    }

    // Вызывается при подборе проклятого предмета
    public void OnCursedItemPickup() {
        TriggerHorrorMode();
    }
}
