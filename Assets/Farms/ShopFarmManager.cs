using UnityEngine;

public class ShopFarmManager : MonoBehaviour
{
    public BuildingInventoryManagerFarm inventory; // Ссылка на инвентарь

    // Эту функцию вешаем на кнопку покупки Моркови (index 0)
    public void BuyCarrotSeed()
    {
        TryBuySeed(0);
    }

    // На кнопку Кукурузы (index 1)
    public void BuyCornSeed()
    {
        TryBuySeed(1);
    }

    // На кнопку Пшеницы (index 2)
    public void BuyWheatSeed()
    {
        TryBuySeed(2);
    }

    private void TryBuySeed(int index)
    {
        // Получаем данные о цене
        CropData crop = inventory.definedCrops[index];
        int cost = crop.seedCost;

        // Пробуем списать биомассу через твоего менеджера
        if (ResourceManager.Instance.SpendBiomass(cost))
        {
            // Если деньги списались -> добавляем 1 семечко
            inventory.AddSeed(index, 1);
        }
        else
        {
            Debug.Log("Не хватает биомассы!");
            // Тут можно добавить звук ошибки или мигание красным
        }
    }
}