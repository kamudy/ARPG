using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class UIItemSlot : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IDropHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image iconImage;
    public TMP_Text amountText;

    private InventoryUI ui;
    private int index;

    private ItemStack currentStack;

    // =========================
    // SETUP
    // =========================
    public void Setup(InventoryUI ui, int index)
    {
        this.ui = ui;
        this.index = index;
    }

    public void Set(ItemStack stack)
    {
        currentStack = stack;

        if (stack == null || stack.data == null)
        {
            Clear();
            return;
        }

        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = stack.data.icon;
            iconImage.color = Color.white;
        }

        if (amountText != null)
        {
            // mostrar cantidad solo si stackable y > 1
            bool show = stack.data.stackable && stack.amount > 1;
            amountText.gameObject.SetActive(show);
            if (show) amountText.text = stack.amount.ToString();
        }
    }

    public void Clear()
    {
        currentStack = null;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            iconImage.color = Color.clear;
        }

        if (amountText != null)
        {
            amountText.text = "";
            amountText.gameObject.SetActive(false);
        }
    }

    // Método para obtener el stack actual (usado por QuickSlotsUI)
    public ItemStack GetStack()
    {
        return currentStack;
    }

    // =========================
    // TOOLTIP
    // =========================
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ui == null) return;
        if (currentStack == null || currentStack.data == null) return;

        // ✅ Ahora ShowTooltip solo recibe el item
        ui.ShowTooltip(currentStack.data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ui == null) return;
        ui.HideTooltip();
    }

    // =========================
    // CLICK
    // =========================
    public void OnPointerClick(PointerEventData eventData)
    {
        if (ui == null) return;
        if (currentStack == null || currentStack.data == null) return;

        ui.HideTooltip();

        // Click derecho: usar/equipar
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            eventData.Use(); // Prevenir que otros scripts reciban este click
            ui.RightClick(index);
            return;
        }

        // Click izquierdo: prevenir que pase a otros sistemas (skills, etc)
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            eventData.Use();
        }
    }

    // =========================
    // DRAG & DROP
    // =========================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ui == null) return;

        // solo click izquierdo arrastra
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (currentStack == null || currentStack.data == null) return;

        ui.BeginDrag(index);
        ui.HideTooltip(); // evita que el tooltip quede flotando
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ui == null) return;
        if (!ui.IsDragging) return;

        ui.Drag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ui == null) return;

        // Si soltaste fuera de un slot/equipo, simplemente termina drag
        ui.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (ui == null) return;
        if (!ui.IsDragging) return;

        // Dropeaste sobre otro slot: swap
        ui.DropOn(index);
    }
}
