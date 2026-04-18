using System;
using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    public int maxMana = 50;
    public int currentMana = 50;

    public event Action<int, int> OnManaChanged;

    void Start()
    {
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        Notify();
    }

    void Notify()
    {
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public void SetMaxMana(int newMax, bool keepPercent = true)
    {
        newMax = Mathf.Max(1, newMax);

        newMax = Mathf.Max(1, newMax);

    if (keepPercent)
    {
        float pct = (maxMana > 0) ? (float)currentMana / maxMana : 1f;
        maxMana = newMax;
        currentMana = Mathf.Clamp(Mathf.RoundToInt(pct * maxMana), 0, maxMana);
    }
    else
    {
        maxMana = newMax;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
    }

    OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public bool Spend(int amount)
    {
        if (amount <= 0) return false;
        if (currentMana < amount) return false;

        currentMana -= amount;
        Notify();
        return true;
    }

    public void Regen(int amount)
    {
        if (amount <= 0) return;
        currentMana = Mathf.Min(maxMana, currentMana + amount);
        Notify();
    }

    public bool Restore(int amount)
    {
        if (amount <= 0) return false;
        if (currentMana >= maxMana) return false; // Mana lleno

        currentMana = Mathf.Min(maxMana, currentMana + amount);
        Notify();
        return true;
    }
}
