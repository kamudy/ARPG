using UnityEngine;


public enum ItemType
{
    Potion,
    Weapon,
    Armor,
    Head
}


public enum ArmorSlot
{
    Head,
    Chest,
    Gloves,
    Boots
}



[CreateAssetMenu(menuName = "RPG/Item")]
public class ItemData : ScriptableObject
{

    [Header("Armor Stats")]
    public int defense = 0;

    [Header("Potion")]
    public int healAmount = 25;
    public int manaAmount = 0; // Para pociones de mana
    public string itemName;
    public Sprite icon;
    public ItemType itemType;

    public bool stackable;
    public int maxStack = 99;

    public ArmorSlot armorSlot; // solo si es armadura

    [Header("Shop")]
    public int price = 100; // precio de venta

    [Header("Requirements")]
    public int reqLevel = 0;
    public int reqStrength = 0;
    public int reqAgility = 0;
    public int reqVitality = 0;
    public int reqEnergy = 0;

}
