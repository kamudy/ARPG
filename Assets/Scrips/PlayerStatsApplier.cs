using UnityEngine;

[RequireComponent(typeof(PlayerDerivedStats))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerStatsApplier : MonoBehaviour
{
    private PlayerDerivedStats derived;
    private PlayerHealth health;
    private PlayerMana mana;

    void Awake()
    {
        derived = GetComponent<PlayerDerivedStats>();
        health = GetComponent<PlayerHealth>();
        mana = GetComponent<PlayerMana>(); // puede ser null si no está agregado
    }

    void OnEnable()
    {
        if (derived != null)
            derived.OnDerivedChanged += Apply;
    }

    void OnDisable()
    {
        if (derived != null)
            derived.OnDerivedChanged -= Apply;
    }

    void Start()
    {
        Apply();
    }

    void Apply()
    {
        if (derived == null) return;

        // HP
        if (health != null)
            health.SetMaxHealth(derived.MaxHP, keepPercent: true);

        // Mana (si agregaste el componente después, lo vuelve a buscar)
        if (mana == null)
            mana = GetComponent<PlayerMana>();

        if (mana != null)
            mana.SetMaxMana(derived.MaxMana, keepPercent: true);

        // Debug (podés apagar después)
        // Debug.Log($"Apply Stats -> MaxHP:{derived.MaxHP} MaxMana:{derived.MaxMana}");
    }
}
