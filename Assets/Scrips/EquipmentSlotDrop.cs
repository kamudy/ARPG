using UnityEngine;
using UnityEngine.EventSystems;

public enum EquipmentSlotType
{
    Head,
    Chest,
    Gloves,
    Boots,
    WeaponLeft,
    WeaponRight
}

public class EquipmentSlotDrop : MonoBehaviour, IDropHandler
{
    public InventoryUI inventoryUI;
    public EquipmentSlotType slotType;

    public void OnDrop(PointerEventData eventData)
    {
        if (inventoryUI == null) return;
        if (!inventoryUI.IsDragging) return;

        inventoryUI.DropOnEquipment(slotType);
    }
}
