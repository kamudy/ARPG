using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // Player Stats
    public int playerLevel;
    public int playerXP;
    public int playerHealth;
    public int playerMaxHealth;
    public int playerMana;
    public int playerMaxMana;

    // Player Character Stats
    public int strength;
    public int agility;
    public int vitality;
    public int energy;
    public int statPoints;

    // Player Position
    public Vector3SerializableData playerPosition;

    // Inventory
    public int coins;
    public List<ItemStackData> inventoryItems = new List<ItemStackData>();

    // Equipped Items
    public string equippedHead = "";
    public string equippedChest = "";
    public string equippedGloves = "";
    public string equippedBoots = "";
    public string equippedWeaponLeft = "";
    public string equippedWeaponRight = "";
}

[System.Serializable]
public class ItemStackData
{
    public string itemName;
    public int amount;
}

[System.Serializable]
public class Vector3SerializableData
{
    public float x;
    public float y;
    public float z;

    public Vector3SerializableData(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}
