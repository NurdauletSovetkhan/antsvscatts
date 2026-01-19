using UnityEngine;
using TMPro;     // Раскомментируйте, если используете TextMeshPro

public class LocationSwitcher : MonoBehaviour
{
    [Header("Настройки")]
    public CameraMover cameraMover; // Ссылка на ваш скрипт камеры

    [Header("Точки назначения")]
    public Transform housePoint;    // Точка "Дом" (начальная)
    public Transform farmPoint;     // Точка "Ферма"

    [Header("Кнопка")]
    public TMP_Text buttonLabel; // Используйте это для TextMeshPro

    private bool isAtHouse = true;  // Логическая переменная: "Мы сейчас дома?"

    // Эту функцию вешаем на кнопку
    public void ToggleLocation()
    {
        if (isAtHouse)
        {
            // Если мы дома -> Едем на Ферму
            cameraMover.MoveToTarget(farmPoint);

            // Меняем надпись на кнопке, чтобы игрок знал, что она теперь ведет "Домой"
            if (buttonLabel != null) buttonLabel.text = "House";

            isAtHouse = false; // Запоминаем, что мы уехали из дома
        }
        else
        {
            // Если мы на ферме -> Едем Домой
            cameraMover.MoveToTarget(housePoint);

            // Меняем надпись обратно
            if (buttonLabel != null) buttonLabel.text = "Farm";

            isAtHouse = true; // Запоминаем, что мы вернулись
        }
    }
}