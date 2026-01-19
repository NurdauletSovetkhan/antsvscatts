using UnityEngine;
using TMPro;

public class DamageIndicator : MonoBehaviour
{
    [Header("Settings")]
    public float lifetime = 1f;
    public float floatSpeed = 2f;
    public float fadeSpeed = 1f;
    
    private TextMeshPro textMesh;
    private Color startColor;
    private float timer;

    void Awake() {
        // Ищем TextMeshPro и на этом объекте, и среди детей
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null) {
            textMesh = GetComponentInChildren<TextMeshPro>();
        }

        if (textMesh != null) {
            startColor = textMesh.color;
        }
    }

    void Update() {
        timer += Time.deltaTime;

        // Летим вверх
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Плавное исчезновение
        if (textMesh != null) {
            float alpha = Mathf.Lerp(startColor.a, 0f, timer / lifetime);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }

        // Удаляем когда время вышло
        if (timer >= lifetime) {
            Destroy(gameObject);
        }
    }

    public void SetDamage(float damage) {
        if (textMesh != null) {
            textMesh.text = Mathf.RoundToInt(damage).ToString();
        }
    }

    public void SetColor(Color color) {
        if (textMesh != null) {
            startColor = color;
            textMesh.color = color;
        }
    }
}
