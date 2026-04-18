using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerDerivedStats : MonoBehaviour
{
    [Header("Base Values")]
    public int baseMaxHP = 100;
    public int baseMaxMana = 50;
    public int baseMeleeDamage = 25;
    public float baseMoveSpeed = 6f;

    [Header("Scaling (por punto)")]
    public int hpPerVitality = 5;
    public int manaPerEnergy = 5;
    public int dmgPerStrength = 2;
    public float speedPerAgility = 0.03f;

    [Header("Defense")]
    public int baseDefense = 10;
    public float defensePerAgility = 0.5f; // cada punto de agi suma 0.5 de defensa

    [Header("Results (read-only)")]
    public int MaxHP { get; private set; }
    public int MaxMana { get; private set; }
    public int MeleeDamage { get; private set; }
    public float MoveSpeed { get; private set; }
    public int Defense { get; private set; }

    public event Action OnDerivedChanged;

    private PlayerStats stats;
    private PlayerInventory inv;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        inv = GetComponent<PlayerInventory>();

        // Asegurar que los valores base estén seteados correctamente
        if (baseDefense == 0)
        {
            baseDefense = 10;
            Debug.Log("⚠️ baseDefense estaba en 0, se corrigió a 10");
        }

        if (defensePerAgility == 0)
        {
            defensePerAgility = 0.5f;
            Debug.Log("⚠️ defensePerAgility estaba en 0, se corrigió a 0.5");
        }
    }

    void OnEnable()
    {
        if (stats != null) stats.OnStatsChanged += Recalculate;
        if (inv != null) inv.OnInventoryChanged += Recalculate; // equip/desequip
    }

    void OnDisable()
    {
        if (stats != null) stats.OnStatsChanged -= Recalculate;
        if (inv != null) inv.OnInventoryChanged -= Recalculate;
    }

    void Start()
    {
        Recalculate();
    }

    public void Recalculate()
    {
        if (stats == null) 
        {
            Debug.LogWarning("PlayerDerivedStats: stats es NULL");
            return;
        }

        MaxHP = baseMaxHP + stats.vitality * hpPerVitality;
        MaxMana = baseMaxMana + stats.energy * manaPerEnergy;
        MeleeDamage = baseMeleeDamage + stats.strength * dmgPerStrength;
        MoveSpeed = baseMoveSpeed + stats.agility * speedPerAgility;

        int def = baseDefense + (int)(stats.agility * defensePerAgility);

        if (inv != null)
        {
            def += GetArmorDefense(inv.head);
            def += GetArmorDefense(inv.chest);
            def += GetArmorDefense(inv.gloves);
            def += GetArmorDefense(inv.boots);
        }

        Defense = Mathf.Max(0, def);

        Debug.Log($"PlayerDerivedStats recalculado: Defensa Base={baseDefense}, Agi={stats.agility}, DefPerAgi={defensePerAgility}, Defensa Total={Defense}");

        OnDerivedChanged?.Invoke();
    }

    int GetArmorDefense(ItemData item)
    {
        if (item == null) return 0;
        if (item.itemType != ItemType.Armor) return 0;
        return Mathf.Max(0, item.defense);
    }
}
