using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth = 100;

    // Evento: (current, max)
    public event Action<int, int> OnHealthChanged;
    // Evento de muerte
    public event Action OnPlayerDeath;

    // Invulnerabilidad temporal (dash, abilities, etc)
    private bool isInvulnerable = false;

    private PlayerDerivedStats derivedStats;

    void Awake()
    {
        derivedStats = GetComponent<PlayerDerivedStats>();
    }

    void Start()
    {
        // Si hay PlayerDrivedStats, usar su MaxHP
        if (derivedStats != null)
        {
            maxHealth = derivedStats.MaxHP;
        }

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Notify();
    }

    void OnEnable()
    {
        if (derivedStats != null)
            derivedStats.OnDerivedChanged += SyncMaxHealth;
    }

    void OnDisable()
    {
        if (derivedStats != null)
            derivedStats.OnDerivedChanged -= SyncMaxHealth;
    }

    private void SyncMaxHealth()
    {
        if (derivedStats == null) return;
        maxHealth = derivedStats.MaxHP;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Notify();
    }

    public void Notify()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public bool Heal(int amount)
    {
        if (amount <= 0) return false;
        if (currentHealth >= maxHealth) return false;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"[PlayerHealth] Healed: +{amount}, Total: {currentHealth}/{maxHealth}");

        Notify();
        return true;
    }

    public bool TakeDamage(int amount)
    {
        // Si está invulnerable, no toma daño
        if (isInvulnerable)
        {
            Debug.Log($"🛡️ Daño bloqueado por invulnerabilidad: {amount}");
            return false;
        }

        if (amount <= 0) return false;
        if (currentHealth <= 0) return false;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        Notify();

        // Verificar si el jugador murió
        if (currentHealth <= 0)
        {
            Die();
        }

        return true;
    }

    /// <summary>Activar invulnerabilidad temporal (para dash, abilities, etc)</summary>
    public void SetInvulnerable(bool state)
    {
        isInvulnerable = state;
        if (state)
            Debug.Log("🛡️ Invulnerabilidad ACTIVADA");
        else
            Debug.Log("🛡️ Invulnerabilidad DESACTIVADA");
    }

    public bool IsInvulnerable() => isInvulnerable;

    private void Die()
    {
        Debug.Log("¡El jugador ha muerto!");
        OnPlayerDeath?.Invoke();
    }

    // ✅ Cambiar maxHealth en runtime manteniendo el % de vida (recomendado)
    public void SetMaxHealth(int newMax, bool keepPercent = true)
    {
        newMax = Mathf.Max(1, newMax);

        if (keepPercent)
        {
            float pct = (maxHealth > 0) ? (float)currentHealth / maxHealth : 1f;
            maxHealth = newMax;
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(pct * maxHealth), 0, maxHealth);
        }
        else
        {
            maxHealth = newMax;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        Notify();
    }
}
