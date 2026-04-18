using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ShopUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject shopPanel;
    public TMP_Text shopTitleText;
    public TMP_Text coinsText;

    [Header("Tabs")]
    public Button buyTab;
    public Button sellTab;

    [Header("Items")]
    public Transform itemGrid;
    public ShopItemSlot slotPrefab;
    public int maxShopSlots = 24;

    [Header("Refs")]
    public PlayerInventory playerInventory;

    private ShopNPC currentShop;
    private List<ShopItemSlot> slots = new List<ShopItemSlot>();
    private PlayerInputActions inputActions;
    private bool isBuyMode = true;

    void Start()
    {
        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<PlayerInventory>();

        if (shopPanel != null)
            shopPanel.SetActive(false);

        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();

        if (buyTab != null)
            buyTab.onClick.AddListener(() => SetBuyMode(true));
        if (sellTab != null)
            sellTab.onClick.AddListener(() => SetBuyMode(false));

        CreateSlots();
    }

    void CreateSlots()
    {
        if (slotPrefab == null) return;

        for (int i = 0; i < maxShopSlots; i++)
        {
            ShopItemSlot slot = Instantiate(slotPrefab, itemGrid);
            slot.Setup(this);
            slots.Add(slot);
        }
    }

    public void OpenShop(ShopNPC shop)
    {
        currentShop = shop;

        if (shopPanel != null)
            shopPanel.SetActive(true);

        if (shopTitleText != null)
            shopTitleText.text = shop.npcName;

        isBuyMode = true;
        UpdateTabButtons();
        RefreshShop();
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        currentShop = null;
    }

    void SetBuyMode(bool buyMode)
    {
        isBuyMode = buyMode;
        UpdateTabButtons();
        RefreshShop();
    }

    void UpdateTabButtons()
    {
        if (buyTab != null)
            buyTab.interactable = !isBuyMode;
        if (sellTab != null)
            sellTab.interactable = isBuyMode;
    }

    void RefreshShop()
    {
        if (currentShop == null) return;

        if (coinsText != null && playerInventory != null)
            coinsText.text = $"Monedas: {playerInventory.coins}";

        if (isBuyMode)
        {
            RefreshBuyMode();
        }
        else
        {
            RefreshSellMode();
        }
    }

    void RefreshBuyMode()
    {
        // Mostrar items de la tienda
        for (int i = 0; i < slots.Count; i++)
        {
            if (i < currentShop.shopItems.Count)
            {
                ItemData item = currentShop.shopItems[i];
                slots[i].SetItem(item, true); // true = modo compra
                slots[i].gameObject.SetActive(true);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    void RefreshSellMode()
    {
        // Mostrar items del inventario del jugador
        int slotIndex = 0;
        for (int i = 0; i < playerInventory.SlotCount && slotIndex < slots.Count; i++)
        {
            ItemStack stack = playerInventory.GetStackAt(i);
            if (stack != null && stack.data != null)
            {
                slots[slotIndex].SetItem(stack.data, false); // false = modo venta
                slots[slotIndex].gameObject.SetActive(true);
                slotIndex++;
            }
        }

        // Desactivar slots restantes
        for (int i = slotIndex; i < slots.Count; i++)
        {
            slots[i].gameObject.SetActive(false);
        }
    }

    public void BuyItem(ItemData item)
    {
        if (currentShop == null || playerInventory == null) return;

        if (currentShop.TryBuyItem(item, playerInventory))
        {
            RefreshShop();
        }
    }

    public void SellItem(ItemData item)
    {
        if (currentShop == null || playerInventory == null) return;

        if (currentShop.TrySellItem(item, playerInventory))
        {
            RefreshShop();
        }
    }

    void Update()
    {
        if (shopPanel != null && shopPanel.activeSelf)
        {
            if (inputActions.Player.ToggleInventory.triggered || 
                (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
            {
                CloseShop();
            }
        }
    }

    void OnDestroy()
    {
        if (inputActions != null)
            inputActions.Player.Disable();
    }
}
