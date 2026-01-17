using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Vector2 moveInput;
    private Rigidbody2D rb;

    [Header("Interaction")]
    public float interactRange = 1.5f;
    public LayerMask interactableLayer;
    public GameObject turretPrefab; // Префаб турели для посадки
    public int turretCost = 5; // Стоимость в биомассе

    private GameObject nearestInteractable;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update() {
        HandleMovementInput();
        HandleInteractionInput();
        CheckForInteractables();
    }

    void HandleMovementInput() {
        // Получаем ввод (WASD или стрелки) через новый Input System
        moveInput = Vector2.zero;
        
        if (Keyboard.current != null) {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                moveInput.y -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                moveInput.x += 1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                moveInput.x -= 1;
        }
    }

    void HandleInteractionInput() {
        // Нажатие E для взаимодействия
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) {
            TryInteract();
        }
    }

    void CheckForInteractables() {
        // Ищем ближайший объект для взаимодействия по тегам
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);
        
        foreach (Collider2D hit in hits) {
            if (hit.CompareTag("Pot") || hit.CompareTag("Resource") || hit.CompareTag("ResourcePot")) {
                nearestInteractable = hit.gameObject;
                return;
            }
        }
        
        nearestInteractable = null;
    }

    void TryInteract() {
        if (nearestInteractable == null) return;

        // Взаимодействие с "Pot" (пустая ячейка для посадки турели)
        if (nearestInteractable.CompareTag("Pot")) {
            PlantTurret(nearestInteractable);
        }

        // Взаимодействие с ResourcePot (ферма ресурсов)
        if (nearestInteractable.CompareTag("ResourcePot")) {
            CollectFromResourcePot(nearestInteractable);
        }

        // Взаимодействие с обычными ресурсами
        if (nearestInteractable.CompareTag("Resource")) {
            CollectResource(nearestInteractable);
        }
    }

    void PlantTurret(GameObject pot) {
        // Проверяем, только в фазе подготовки
        if (GameManager.Instance.CurrentPhase != GameManager.GamePhase.PrepPhase) {
            Debug.Log("Can only plant during PrepPhase!");
            return;
        }

        // Проверяем ресурсы
        if (!ResourceManager.Instance.SpendBiomass(turretCost)) {
            Debug.Log("Not enough Biomass to plant turret!");
            return;
        }

        // Создаем турель на месте горшка
        if (turretPrefab != null) {
            Instantiate(turretPrefab, pot.transform.position, Quaternion.identity);
            Destroy(pot); // Удаляем горшок
            Debug.Log("Turret planted!");
        }
    }

    void CollectResource(GameObject resource) {
        // Собираем ресурс
        ResourceManager.Instance.AddBiomass(3);
        Destroy(resource);
        Debug.Log("Resource collected!");
    }

    void CollectFromResourcePot(GameObject pot) {
        ResourcePot resourcePot = pot.GetComponent<ResourcePot>();
        
        if (resourcePot != null && resourcePot.HasResources()) {
            int amount = resourcePot.CollectResources();
            ResourceManager.Instance.AddBiomass(amount);
            Debug.Log($"Collected {amount} biomass from farm pot!");
        } else {
            Debug.Log("No resources to collect from this pot.");
        }
    }

    void FixedUpdate() {
        // Двигаем кота без лишней физики (как в Brotato)
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    // Визуализация радиуса взаимодействия
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}