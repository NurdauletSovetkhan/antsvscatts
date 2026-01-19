using UnityEngine;

public class BuildingInventoryManagerFarm : MonoBehaviour
{
    [Header("Данные")]
    // Какие виды растений у нас вообще есть (Морковь, Кукуруза, Пшеница)
    public CropData[] definedCrops;

    [Header("UI Ссылки")]
    public InventorySlot[] seedSlotsUI;    // 3 слота под семена
    public InventorySlot[] produceSlotsUI; // 3 слота под плоды

    [Header("Состояние Инвентаря")]
    // Количество семян (индекс 0 = семена definedCrops[0])
    public int[] seedCounts;
    // Количество плодов (индекс 0 = плоды definedCrops[0])
    public int[] produceCounts;

    private int selectedSeedIndex = 0;

    void Start()
    {
        // Инициализируем массивы, если они пустые
        if (seedCounts.Length != definedCrops.Length) seedCounts = new int[definedCrops.Length];
        if (produceCounts.Length != definedCrops.Length) produceCounts = new int[definedCrops.Length];

        UpdateUI();
        SelectSeedSlot(0);
    }

    void Update()
    {
        // Выбор слота семян
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSeedSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSeedSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSeedSlot(2);
    }

    // --- ЛОГИКА UI ---

    void SelectSeedSlot(int index)
    {
        if (index < 0 || index >= seedSlotsUI.Length) return;

        selectedSeedIndex = index;

        // Визуальное выделение
        foreach (var slot in seedSlotsUI) slot.Deselect();
        seedSlotsUI[index].Select();
    }

    void UpdateUI()
    {
        // Обновляем слоты семян
        for (int i = 0; i < seedSlotsUI.Length; i++)
        {
            if (i < definedCrops.Length)
                seedSlotsUI[i].Setup(definedCrops[i], seedCounts[i]);
            else
                seedSlotsUI[i].Setup(null, 0);
        }

        // Обновляем слоты плодов (продукции)
        for (int i = 0; i < produceSlotsUI.Length; i++)
        {
            if (i < definedCrops.Length)
                produceSlotsUI[i].Setup(definedCrops[i], produceCounts[i]); // Используем ту же иконку, но это Плод
            else
                produceSlotsUI[i].Setup(null, 0);
        }
    }

    // --- ЛОГИКА МАГАЗИНА И ФЕРМЫ ---

    // 1. Покупка семян (Вызывается из Магазина)
    public void AddSeed(int index, int amount)
    {
        seedCounts[index] += amount;
        UpdateUI();
    }

    // 2. Попытка взять семя для посадки (Вызывается FarmManager)
    public CropData TryGetSeedToPlant()
    {
        // Если семян этого типа > 0
        if (seedCounts[selectedSeedIndex] > 0)
        {
            return definedCrops[selectedSeedIndex];
        }
        return null; // Нет семян
    }

    // 3. Трата семени (Вызывается FarmManager ПОСЛЕ успешной посадки)
    public void ConsumeSeed()
    {
        seedCounts[selectedSeedIndex]--;
        UpdateUI();
    }

    // 4. Сбор урожая (Вызывается FarmManager)
    // Мы передаем CropData, чтобы понять, какой именно плод добавить
    public void AddProduce(CropData crop)
    {
        // Ищем индекс этого растения в нашем списке
        for (int i = 0; i < definedCrops.Length; i++)
        {
            if (definedCrops[i] == crop)
            {
                produceCounts[i]++;
                UpdateUI();
                return;
            }
        }
    }
}