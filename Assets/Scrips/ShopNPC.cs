using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShopNPC : MonoBehaviour
{
    [Header("Shop")]
    public string npcName = "Vendedor";
    public List<ItemData> shopItems = new List<ItemData>();

    [Header("Sell Settings")]
    public float sellPriceMultiplier = 0.5f; // Vende al 50% del precio original

    [Header("UI")]
    public ShopUI shopUI;

    [Header("Interaction")]
    public float interactionDistance = 3f;

    private Collider npcCollider;
    private PlayerInputActions inputActions;
    private Transform playerTransform;

    void Start()
    {
        if (shopUI == null)
            shopUI = FindFirstObjectByType<ShopUI>();

        npcCollider = GetComponent<Collider>();
        if (npcCollider == null)
        {
            Debug.LogError("El NPC necesita un Collider para funcionar.");
        }

        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        inputActions.Player.Click.performed += OnClickPerformed;

        playerTransform = FindFirstObjectByType<PlayerInventory>()?.transform;
    }

    void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Player.Click.performed -= OnClickPerformed;
            inputActions.Player.Disable();
        }
    }

    void OnClickPerformed(InputAction.CallbackContext ctx)
    {
        // Raycast desde la cámara
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                OpenShop();
            }
        }
    }

    public void OpenShop()
    {
        if (shopUI == null)
        {
            Debug.LogError("ShopUI no encontrada.");
            return;
        }

        shopUI.OpenShop(this);
    }

    public bool TryBuyItem(ItemData item, PlayerInventory inventory)
    {
        if (item == null) return false;
        if (inventory == null) return false;

        if (inventory.coins < item.price)
        {
            Debug.Log($"No tienes suficientes monedas. Necesitas {item.price}, tienes {inventory.coins}.");
            return false;
        }

        // ✅ Verificar si el item se puede añadir antes de gastar monedas
        bool added = inventory.AddItem(item);
        if (!added)
        {
            Debug.LogWarning($"⚠️ Inventario lleno. No se pudo comprar {item.itemName}");
            return false;
        }

        inventory.AddCoins(-item.price);

        Debug.Log($"Compraste {item.itemName} por {item.price} monedas.");
        return true;
    }

    public bool TrySellItem(ItemData item, PlayerInventory inventory)
    {
        if (item == null) return false;
        if (inventory == null) return false;

        // Calcular precio de venta
        int sellPrice = Mathf.Max(1, Mathf.RoundToInt(item.price * sellPriceMultiplier));

        // Remover item del inventario
        if (!inventory.RemoveOne(item))
        {
            Debug.Log("No tienes ese item para vender.");
            return false;
        }

        // Agregar monedas
        inventory.AddCoins(sellPrice);

        Debug.Log($"Vendiste {item.itemName} por {sellPrice} monedas.");
        return true;
    }
}
