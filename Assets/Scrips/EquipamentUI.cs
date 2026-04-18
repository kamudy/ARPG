using UnityEngine;
using UnityEngine.UI;

public class EquipmentUI : MonoBehaviour
{
    public PlayerInventory inventory;

    [Header("Armor Slots")]
    public Image headSlot;
    public Image chestSlot;
    public Image glovesSlot;
    public Image bootsSlot;

    [Header("Weapon Slots")]
    public Image weaponLeftSlot;
    public Image weaponRightSlot;

    [Header("Buttons (same objects)")]
    public Button headButton;
    public Button chestButton;
    public Button glovesButton;
    public Button bootsButton;

    public Button weaponLeftButton;
    public Button weaponRightButton;

    void OnEnable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged += Refresh;

        // Conectar clicks (solo una vez por enable)
        if (headButton != null) headButton.onClick.AddListener(() => inventory.UnequipArmor(ArmorSlot.Head));
        if (chestButton != null) chestButton.onClick.AddListener(() => inventory.UnequipArmor(ArmorSlot.Chest));
        if (glovesButton != null) glovesButton.onClick.AddListener(() => inventory.UnequipArmor(ArmorSlot.Gloves));
        if (bootsButton != null) bootsButton.onClick.AddListener(() => inventory.UnequipArmor(ArmorSlot.Boots));

        if (weaponLeftButton != null) weaponLeftButton.onClick.AddListener(() => inventory.UnequipWeapon(false));
        if (weaponRightButton != null) weaponRightButton.onClick.AddListener(() => inventory.UnequipWeapon(true));

        Refresh();
    }

    void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= Refresh;

        // Limpiar listeners para evitar duplicados
        if (headButton != null) headButton.onClick.RemoveAllListeners();
        if (chestButton != null) chestButton.onClick.RemoveAllListeners();
        if (glovesButton != null) glovesButton.onClick.RemoveAllListeners();
        if (bootsButton != null) bootsButton.onClick.RemoveAllListeners();

        if (weaponLeftButton != null) weaponLeftButton.onClick.RemoveAllListeners();
        if (weaponRightButton != null) weaponRightButton.onClick.RemoveAllListeners();
    }

    void Refresh()
    {
        if (inventory == null) return;

        SetIcon(headSlot, inventory.head);
        SetIcon(chestSlot, inventory.chest);
        SetIcon(glovesSlot, inventory.gloves);
        SetIcon(bootsSlot, inventory.boots);

        SetIcon(weaponLeftSlot, inventory.weaponLeft);
        SetIcon(weaponRightSlot, inventory.weaponRight);
    }

    void SetIcon(Image img, ItemData item)
    {
        if (img == null) return;

        if (item == null || item.icon == null)
        {
            img.sprite = null;
            img.color = new Color(1, 1, 1, 0.15f); // “vacío”
        }
        else
        {
            img.sprite = item.icon;
            img.color = Color.white;
        }
    }
}
