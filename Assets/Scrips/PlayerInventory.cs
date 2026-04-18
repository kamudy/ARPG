using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemStack
{
    public ItemData data;
    public int amount;

    public ItemStack(ItemData data, int amount)
    {
        this.data = data;
        this.amount = amount;
    }
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    public int maxSlots = 60;
    public int coins = 0;

    // Slots fijos (pueden ser null)
    [SerializeField] private List<ItemStack> items = new List<ItemStack>();
    public event Action OnInventoryChanged;

    // Equipment
    public ItemData head, chest, gloves, boots;
    public ItemData weaponLeft, weaponRight;

    void Awake()
    {
        EnsureSize();
    }

    void EnsureSize()
    {
        if (items == null) items = new List<ItemStack>();

        while (items.Count < maxSlots) items.Add(null);
        if (items.Count > maxSlots) items.RemoveRange(maxSlots, items.Count - maxSlots);
    }

    public int SlotCount => maxSlots;

    public ItemStack GetStackAt(int index)
    {
        EnsureSize();
        if (index < 0 || index >= items.Count) return null;
        return items[index];
    }

    public void SetStackAt(int index, ItemStack stack)
    {
        EnsureSize();
        if (index < 0 || index >= items.Count) return;
        items[index] = stack;
        OnInventoryChanged?.Invoke();
        
        // Guardar cuando cambia el inventario
        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();
    }

    public void SwapSlots(int a, int b)
    {
        EnsureSize();
        if (a == b) return;
        if (a < 0 || b < 0) return;
        if (a >= items.Count || b >= items.Count) return;

        (items[a], items[b]) = (items[b], items[a]);
        OnInventoryChanged?.Invoke();
        
        // Guardar cuando se intercambian items
        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        OnInventoryChanged?.Invoke();
        
        // Guardar cuando se añaden monedas
        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();
    }

    // Método público para notificar cambios (usado por SaveManager)
    public void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    // Método para obtener todos los items (para SaveManager)
    public List<ItemStack> GetAllItems()
    {
        EnsureSize();
        return items;
    }

    // =========================
    // REQUIREMENTS (centralizado)
    // =========================
    bool CanEquip(ItemData item, out string reason)
    {
        reason = "";

        if (item == null)
        {
            reason = "Item null";
            return false;
        }

        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats == null)
        {
            // Si no hay PlayerStats, no bloqueamos para no romperte el proyecto
            return true;
        }

        if (!stats.MeetsRequirements(item))
        {
            reason = stats.GetMissingRequirementsText(item);
            return false;
        }

        return true;
    }

    // =========================
    // ADD ITEM
    // =========================
    public bool AddItem(ItemData item)
    {
        EnsureSize();
        if (item == null)
        {
            Debug.LogError("❌ PlayerInventory.AddItem: item es NULL");
            return false;
        }

        Debug.Log($"🧪 AddItem() llamado: {item.name}");

        // stackable: buscar stack existente
        if (item.stackable)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].data == item)
                {
                    items[i].amount++;
                    Debug.Log($"  ✅ Item apilado en slot {i}. Cantidad: {items[i].amount}");
                    OnInventoryChanged?.Invoke();
                    
                    // Guardar cuando se añade item
                    if (SaveManager.instance != null)
                        SaveManager.instance.SaveGame();
                    
                    return true;  // ✅ Éxito
                }
            }
        }

        // buscar slot vacío (o con data corrompida)
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null || items[i].data == null)
            {
                items[i] = new ItemStack(item, 1);
                Debug.Log($"  ✅ Item añadido en slot {i}");
                OnInventoryChanged?.Invoke();
                
                // Guardar cuando se añade item
                if (SaveManager.instance != null)
                    SaveManager.instance.SaveGame();
                
                return true;  // ✅ Éxito
            }
        }

        Debug.LogWarning("❌ Inventario lleno. No se pudo añadir: " + item.name);
        return false;  // ❌ Inventario lleno
    }

    public bool RemoveOne(ItemData item)
    {
        EnsureSize();
        if (item == null) return false;

        if (item.stackable)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].data == item)
                {
                    items[i].amount--;
                    if (items[i].amount <= 0) items[i] = null;
                    OnInventoryChanged?.Invoke();
                    
                    // Guardar cuando se remueve item
                    if (SaveManager.instance != null)
                        SaveManager.instance.SaveGame();
                    
                    return true;
                }
            }
            return false;
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].data == item)
            {
                items[i] = null;
                OnInventoryChanged?.Invoke();
                
                // Guardar cuando se remueve item
                if (SaveManager.instance != null)
                    SaveManager.instance.SaveGame();
                
                return true;
            }
        }

        return false;
    }

    // =========================
    // USE POTION (desde slot específico)
    // =========================
    public bool UsePotionFromSlot(int slotIndex)
    {
        EnsureSize();
        if (slotIndex < 0 || slotIndex >= items.Count) return false;

        ItemStack stack = items[slotIndex];
        if (stack == null || stack.data == null) return false;

        ItemData item = stack.data;
        if (item.itemType != ItemType.Potion) return false;

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health == null)
        {
            Debug.LogError("El Player no tiene PlayerHealth.");
            return false;
        }

        bool healed = health.Heal(item.healAmount);
        if (!healed)
        {
            Debug.Log("Vida llena: no se consumió la poción.");
            return false;
        }

        // Remover 1 poción del slot
        stack.amount--;
        if (stack.amount <= 0) items[slotIndex] = null;
        else items[slotIndex] = stack;

        Debug.Log($"Usaste {item.itemName} (-1).");
        OnInventoryChanged?.Invoke();
        return true;
    }

    // USE POTION (alternativo por ItemData)
    public void UsePotion(ItemData item)
    {
        if (item == null) return;
        if (item.itemType != ItemType.Potion) return;

        bool used = false;

        // Intentar curar vida
        if (item.healAmount > 0)
        {
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health != null)
            {
                if (health.Heal(item.healAmount))
                {
                    used = true;
                    Debug.Log($"Usaste {item.itemName}: +{item.healAmount} vida");
                }
            }
        }

        // Intentar restaurar mana
        if (item.manaAmount > 0)
        {
            PlayerMana mana = GetComponent<PlayerMana>();
            if (mana != null)
            {
                if (mana.Restore(item.manaAmount))
                {
                    used = true;
                    Debug.Log($"Usaste {item.itemName}: +{item.manaAmount} mana");
                }
            }
        }

        // Si se usó la poción, removerla
        if (used)
        {
            if (RemoveOne(item))
            {
                Debug.Log($"(-1) {item.itemName}");
                OnInventoryChanged?.Invoke();
            }
        }
        else
        {
            Debug.Log($"No se pudo usar {item.itemName}");
        }
    }

    // =========================
    // EQUIP (por click derecho)
    // =========================
    public void Equip(ItemData item, bool toRightHand = true)
    {
        if (item == null) return;
        if (item.itemType == ItemType.Potion) return;

        if (!CanEquip(item, out string reason))
        {
            Debug.Log($"No podés equipar {item.itemName}. Falta: {reason}");
            return;
        }

        if (!RemoveOne(item))
        {
            Debug.LogWarning("No tenés ese item para equipar.");
            return;
        }

        EquipInternal(item, toRightHand);
        OnInventoryChanged?.Invoke();
    }

    // =========================
    // EQUIP (por drag exacto desde slot)
    // =========================
    public bool EquipFromSlot(int slotIndex, bool toRightHand = true)
    {
        EnsureSize();
        if (slotIndex < 0 || slotIndex >= items.Count) return false;

        ItemStack stack = items[slotIndex];
        if (stack == null || stack.data == null) return false;

        ItemData item = stack.data;
        if (item.itemType == ItemType.Potion) return false;

        if (!CanEquip(item, out string reason))
        {
            Debug.Log($"No podés equipar {item.itemName}. Falta: {reason}");
            return false;
        }

        // Sacar 1 EXACTAMENTE de este slot
        if (item.stackable)
        {
            stack.amount--;
            if (stack.amount <= 0) items[slotIndex] = null;
            else items[slotIndex] = stack;
        }
        else
        {
            items[slotIndex] = null;
        }

        EquipInternal(item, toRightHand);
        OnInventoryChanged?.Invoke();
        return true;
    }

    // =========================
    // Lógica real de equipamiento (reemplaza y devuelve el viejo)
    // =========================
    void EquipInternal(ItemData item, bool toRightHand)
    {
        if (item.itemType == ItemType.Armor)
        {
            ItemData old = null;

            switch (item.armorSlot)
            {
                case ArmorSlot.Head: old = head; head = item; break;
                case ArmorSlot.Chest: old = chest; chest = item; break;
                case ArmorSlot.Gloves: old = gloves; gloves = item; break;
                case ArmorSlot.Boots: old = boots; boots = item; break;
                default:
                    Debug.LogWarning("ArmorSlot inválido en " + item.itemName);
                    AddItem(item);
                    return;
            }

            if (old != null) AddItem(old);
            return;
        }

        if (item.itemType == ItemType.Weapon)
        {
            ItemData old = null;

            if (toRightHand)
            {
                old = weaponRight;
                weaponRight = item;
            }
            else
            {
                old = weaponLeft;
                weaponLeft = item;
            }

            if (old != null) AddItem(old);
            return;
        }

        // Si llega un tipo raro
        AddItem(item);
    }

    // =========================
    // UNEQUIP
    // =========================
    public void UnequipArmor(ArmorSlot slot)
    {
        ItemData equipped = null;

        switch (slot)
        {
            case ArmorSlot.Head: equipped = head; head = null; break;
            case ArmorSlot.Chest: equipped = chest; chest = null; break;
            case ArmorSlot.Gloves: equipped = gloves; gloves = null; break;
            case ArmorSlot.Boots: equipped = boots; boots = null; break;
        }

        if (equipped != null)
        {
            AddItem(equipped);
            OnInventoryChanged?.Invoke();
        }
    }

    public void UnequipWeapon(bool rightHand)
    {
        ItemData equipped = rightHand ? weaponRight : weaponLeft;

        if (rightHand) weaponRight = null;
        else weaponLeft = null;

        if (equipped != null)
        {
            AddItem(equipped);
            OnInventoryChanged?.Invoke();
        }
    }
}
