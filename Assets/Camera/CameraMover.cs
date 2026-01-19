using UnityEngine;

public class CameraMover : MonoBehaviour
{
    // Скорость перемещения
    public float smoothSpeed = 5f;

    // Целевая позиция, куда камера должна приехать
    private Vector3 targetPosition;

    // Флаг, разрешающий движение
    private bool shouldMove = false;

    void Start()
    {
        // Изначально цель — это текущая позиция камеры
        targetPosition = transform.position;
    }

    void Update()
    {
        if (shouldMove)
        {
            // Плавно меняем позицию от текущей к целевой (Lerp)
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

            // Если камера почти приехала, останавливаем вычисления (оптимизация)
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                shouldMove = false;
            }
        }
    }

    // Эту функцию мы будем вызывать при нажатии кнопки
    // Мы передаем координаты X и Y, а Z оставляем -10 (стандарт для 2D)
    public void MoveToPosition(float x, float y)
    {
        targetPosition = new Vector3(x, y, -10f);
        shouldMove = true;
    }

    // Альтернатива: Если вы хотите переместиться к конкретному объекту
    public void MoveToObject(Transform obj)
    {
        targetPosition = new Vector3(obj.position.x, obj.position.y, -10f);
        shouldMove = true;
    }

    // Этот метод принимает 1 параметр (Transform), поэтому он появится в кнопке!
    public void MoveToTarget(Transform targetPoint)
    {
        // Берем координаты у объекта-цели
        targetPosition = new Vector3(targetPoint.position.x, targetPoint.position.y, -10f);
        shouldMove = true;
    }

}