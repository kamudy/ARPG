using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public int strength = 10;
    public int agility  = 10;
    public int vitality = 10;
    public int energy   = 10;

    [Header("Unspent Points")]
    public int points = 0;

    // ✅ Evento para UI
    public event Action OnStatsChanged;

    void Awake()
    {
        // Asegurar que al despertar, los stats notifiquen a la UI
        // Esto es especialmente importante en juegos nuevos
        Notify();
    }

    void Notify()
    {
        OnStatsChanged?.Invoke();
    }

    // ✅ Nivel REAL: lo toma del PlayerLevel
    public int Level
    {
        get
        {
            PlayerLevel lvl = GetComponent<PlayerLevel>();
            return (lvl != null) ? lvl.level : 1;
        }
    }

    // ✅ Alias por si tu UI todavía usa statPoints
    public int statPoints => points;

    // ✅ lo llama PlayerLevel cuando subís de nivel
    public void AddPoints(int amount)
    {
        if (amount <= 0) return;
        points += amount;
        Notify();
        Debug.Log($"Puntos +{amount}. Total puntos: {points}");
    }

    // Gastar puntos (lo usa tu panel)
    public bool SpendPointStrength()
    {
        if (points <= 0) return false;
        strength++;
        points--;
        Notify();
        
        // Guardar cambios de estadística
        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();
        
        return true;
    }

    public bool SpendPointAgility()
    {
        if (points <= 0) return false;
        agility++;
        points--;
        Notify();
        
        // Guardar cambios de estadística
        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();
        
        return true;
    }

    public bool SpendPointVitality()
    {
        if (points <= 0) return false;
        vitality++;
        points--;
        Notify();
        
        // Guardar cambios de estadística
        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();
        
        return true;
    }

    public bool SpendPointEnergy()
    {
        if (points <= 0) return false;
        energy++;
        points--;
        Notify();
        
        // Guardar cambios de estadística
        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();
        
        return true;
    }

    // =========================
    // REQUIREMENTS
    // =========================
    public bool MeetsRequirements(ItemData item)
    {
        if (item == null) return false;

        if (Level < item.reqLevel) return false;
        if (strength < item.reqStrength) return false;
        if (agility  < item.reqAgility)  return false;
        if (vitality < item.reqVitality) return false;
        if (energy   < item.reqEnergy)   return false;

        return true;
    }

    public string GetMissingRequirementsText(ItemData item)
    {
        if (item == null) return "Item inválido.";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (Level < item.reqLevel) sb.Append($"Nivel {item.reqLevel} ");
        if (strength < item.reqStrength) sb.Append($"STR {item.reqStrength} ");
        if (agility < item.reqAgility) sb.Append($"AGI {item.reqAgility} ");
        if (vitality < item.reqVitality) sb.Append($"VIT {item.reqVitality} ");
        if (energy < item.reqEnergy) sb.Append($"ENE {item.reqEnergy} ");

        return sb.ToString().Trim();
    }
}
