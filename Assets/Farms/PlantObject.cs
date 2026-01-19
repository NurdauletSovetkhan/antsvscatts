using UnityEngine;

public class PlantObject : MonoBehaviour
{
    private CropData data; // Данные о том, кто мы (Морковь или Кукуруза)
    private int currentStageIndex = 0;
    private GameObject currentVisual; // Текущий видимый префаб (например, Prefab_Carrot_02)
    private bool isMature = false;
    public CropData GetData() { return data; }

    // Эту функцию вызовет Менеджер при посадке
    public void Initialize(CropData cropData)
    {
        data = cropData;
        currentStageIndex = 0;
        UpdateVisual();
    }

    public void WaterPlant()
    {
        if (!isMature)
        {
            Invoke("Grow", data.timeBetweenStages);
        }
    }

    void Grow()
    {
        currentStageIndex++;

        // Проверяем, есть ли следующая стадия
        if (currentStageIndex < data.growthStages.Length)
        {
            UpdateVisual();
        }
        else
        {
            isMature = true; // Выросли
        }
    }

    // Главная магия: Удаляем старый префаб, создаем новый
    void UpdateVisual()
    {
        if (currentVisual != null) Destroy(currentVisual);

        // Создаем префаб текущей стадии как дочерний объект (transform)
        currentVisual = Instantiate(data.growthStages[currentStageIndex], transform);
    }

    public void Harvest()
    {
        // 1. Создаем эффект сбора (VFX)
        if (data.harvestVFX != null)
            Instantiate(data.harvestVFX, transform.position, Quaternion.identity);

        // 2. Выкидываем продукт (Морковку)
        if (data.produce != null)
            Instantiate(data.produce, transform.position, Quaternion.identity);

        // 3. Уничтожаем сам куст
        Destroy(gameObject);
    }

    public bool IsReady()
    {
        // Считаем, что готово, если это последняя стадия
        return currentStageIndex >= data.growthStages.Length - 1;
    }
}