using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Biomass Settings")]
    public int currentBiomass = 0;
    public int startingBiomass = 10; // Стартовые ресурсы

    // Событие для обновления UI
    public static event Action<int> OnBiomassChanged;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        currentBiomass = startingBiomass;
        OnBiomassChanged?.Invoke(currentBiomass);
    }

    public void AddBiomass(int amount) {
        currentBiomass += amount;
        OnBiomassChanged?.Invoke(currentBiomass);
        Debug.Log($"+{amount} Biomass! Total: {currentBiomass}");
    }

    public bool SpendBiomass(int amount) {
        if (currentBiomass >= amount) {
            currentBiomass -= amount;
            OnBiomassChanged?.Invoke(currentBiomass);
            Debug.Log($"-{amount} Biomass. Remaining: {currentBiomass}");
            return true;
        } else {
            Debug.LogWarning("Not enough Biomass!");
            return false;
        }
    }

    public int GetBiomass() {
        return currentBiomass;
    }
}
