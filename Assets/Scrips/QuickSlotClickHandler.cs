using UnityEngine;
using UnityEngine.EventSystems;

public class QuickSlotClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private QuickSlotsUI quickSlotsUI;
    private int slotIndex = -1;

    public void SetupSlot(QuickSlotsUI ui, int index)
    {
        quickSlotsUI = ui;
        slotIndex = index;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (quickSlotsUI == null) return;
        quickSlotsUI.OnSlotPointerClick(slotIndex, eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (quickSlotsUI == null) return;
        quickSlotsUI.OnSlotPointerEnter(slotIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (quickSlotsUI == null) return;
        quickSlotsUI.OnSlotPointerExit(slotIndex);
    }
}
