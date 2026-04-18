using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemSlot : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button buyButton;

    private ShopUI shopUI;
    private ItemData item;
    private bool isBuyMode = true;

    void Start()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(OnButtonClick);
    }

    public void Setup(ShopUI shopUI)
    {
        this.shopUI = shopUI;
    }

    public void SetItem(ItemData item, bool buyMode = true)
    {
        this.item = item;
        this.isBuyMode = buyMode;

        if (item == null)
        {
            Clear();
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = Color.white;
        }

        if (nameText != null)
            nameText.text = item.itemName;

        // Mostrar precio diferente según modo
        if (priceText != null)
        {
            if (isBuyMode)
            {
                priceText.text = $"Comprar: {item.price} monedas";
            }
            else
            {
                int sellPrice = Mathf.Max(1, Mathf.RoundToInt(item.price * 0.5f));
                priceText.text = $"Vender: {sellPrice} monedas";
            }
        }

        if (buyButton != null)
        {
            buyButton.interactable = true;
            // Cambiar texto del botón
            Text buttonText = buyButton.GetComponentInChildren<Text>();
            if (buttonText != null)
                buttonText.text = isBuyMode ? "Comprar" : "Vender";
        }
    }

    public void Clear()
    {
        item = null;

        if (iconImage != null)
            iconImage.sprite = null;

        if (nameText != null)
            nameText.text = "";

        if (priceText != null)
            priceText.text = "";

        if (buyButton != null)
            buyButton.interactable = false;
    }

    void OnButtonClick()
    {
        if (shopUI == null || item == null) return;

        if (isBuyMode)
        {
            shopUI.BuyItem(item);
        }
        else
        {
            shopUI.SellItem(item);
        }
    }
}
