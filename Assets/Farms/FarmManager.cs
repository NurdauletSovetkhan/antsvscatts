using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class FarmManager : MonoBehaviour
{
    [Header("Земля")]
    public Tilemap soilTilemap;
    public TileBase wetRuleTile; // СЮДА КИДАЕМ ВАШ RuleTile (Watered)
    public TileBase dryRuleTile; // Сюда RuleTile (Dry)

    [Header("Что сажаем?")]
    public BuildingInventoryManagerFarm inventory;
    public GameObject basePlantContainer; // Пустой префаб с новым скриптом PlantObject
    public CropData selectedSeed;         // Какое растение выбрано сейчас? (перетащите сюда Data_Carrot по умолчанию)

    // База данных посадок
    private Dictionary<Vector3Int, PlantObject> activePlants = new Dictionary<Vector3Int, PlantObject>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) HandleInput();

        // Временный тест: нажимаем 1, 2, 3 для смены семян
        // Позже тут будет UI кнопок
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeSeed("Carrot");
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeSeed("Corn");
    }

    // Вспомогательный метод для теста (удалите, когда сделаете UI)
    public CropData carrotData; // Привяжите в инспекторе Data_Carrot
    public CropData cornData;   // Привяжите в инспекторе Data_Corn

    void ChangeSeed(string name)
    {
        if (name == "Carrot") selectedSeed = carrotData;
        if (name == "Corn") selectedSeed = cornData;
        Debug.Log("Выбрано: " + name);
    }

    void HandleInput()
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPos = soilTilemap.WorldToCell(worldPoint);

        if (!soilTilemap.HasTile(gridPos)) return;

        // Если тут уже растет что-то
        if (activePlants.ContainsKey(gridPos))
        {
            PlantObject plant = activePlants[gridPos];

            if (plant.IsReady())
            {
                // Получаем данные растения перед уничтожением
                CropData harvestedData = plant.GetData(); // Надо добавить метод GetData() в PlantObject!

                plant.Harvest();
                activePlants.Remove(gridPos);
                soilTilemap.SetTile(gridPos, dryRuleTile);

                // 4. Добавляем плод в инвентарь
                inventory.AddProduce(harvestedData);
            }
            else
            {
                WaterTile(gridPos, plant);
            }
        }
        else // Если пусто -> Сажаем то, что выбрано в selectedSeed
        {
            PlantNewCrop(gridPos);
        }
    }

    void PlantNewCrop(Vector3Int pos)
    {
        // БЫЛО (Вызывает ошибку):
        // CropData seedToPlant = inventory.GetSelectedSeed();

        // СТАЛО (Правильно):
        // 1. Проверяем, есть ли семена в инвентаре
        CropData seedToPlant = inventory.TryGetSeedToPlant();

        // Если семян нет (метод вернул null) — выходим, ничего не сажаем
        if (seedToPlant == null)
        {
            Debug.Log("Нет семян для посадки!");
            return;
        }

        Vector3 spawnPos = soilTilemap.GetCellCenterWorld(pos);

        // Создаем растение
        GameObject newObj = Instantiate(basePlantContainer, spawnPos, Quaternion.identity);
        PlantObject script = newObj.GetComponent<PlantObject>();

        script.Initialize(seedToPlant);

        activePlants.Add(pos, script);

        // 2. ВАЖНО: Списываем 1 семечко из инвентаря
        inventory.ConsumeSeed();
    }

    void WaterTile(Vector3Int pos, PlantObject plant)
    {
        soilTilemap.SetTile(pos, wetRuleTile); // Ставим мокрый RuleTile
        plant.WaterPlant();
    }
}