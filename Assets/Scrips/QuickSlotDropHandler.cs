using UnityEngine;
using UnityEngine.EventSystems;

public class QuickSlotDropHandler : MonoBehaviour, IDropHandler
{
    private QuickSlotsUI quickSlotsUI;
    private int slotIndex = -1;

    public void SetupSlot(QuickSlotsUI ui, int index)
    {
        quickSlotsUI = ui;
        slotIndex = index;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (quickSlotsUI == null) return;
        quickSlotsUI.OnSlotDrop(slotIndex, eventData);
    }
}
