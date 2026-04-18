using System;
using UnityEngine;

public class PlayerLevel : MonoBehaviour
{

    [Header("Rewards")]
    public int pointsPerLevel = 5;

    [Header("Level")]
    public int level = 1;

    [Header("XP")]
    public int currentXP = 0;

    [Tooltip("XP base para subir de nivel (nivel 1 -> 2)")]
    public int baseXPToNext = 100;

    [Tooltip("Crecimiento por nivel (ej: 1.25 = 25% más por nivel)")]
    public float growth = 1.25f;

    public event Action OnXPChanged;
    public event Action OnLevelUp;

    public int XPToNext => Mathf.RoundToInt(baseXPToNext * Mathf.Pow(growth, level - 1));

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        // Subidas múltiples si ganás mucha xp
        while (currentXP >= XPToNext)
        {
            currentXP -= XPToNext;
            level++;
            PlayerStats stats = GetComponent<PlayerStats>();
            if (stats != null)
            stats.AddPoints(pointsPerLevel);    
            
            OnLevelUp?.Invoke();
            
            // Guardar cuando subes de nivel
            if (SaveManager.instance != null)
                SaveManager.instance.SaveGame();
        }

        OnXPChanged?.Invoke();
        
        // Guardar cuando ganas experiencia
        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();
    }

    public float XPPercent()
    {
        int need = XPToNext;
        if (need <= 0) return 0f;
        return Mathf.Clamp01((float)currentXP / need);
    }
}
