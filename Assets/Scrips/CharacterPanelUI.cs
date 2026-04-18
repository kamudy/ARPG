using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CharacterPanelUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("Refs")]
    public PlayerLevel playerLevel;
    public PlayerStats stats;
    public PlayerDerivedStats derived;
    public PlayerHealth health;
    public PlayerMana mana;

    [Header("Texts (base)")]
    public TMP_Text levelText;
    public TMP_Text xpText;
    public TMP_Text pointsText;

    public TMP_Text strengthText;
    public TMP_Text agilityText;
    public TMP_Text vitalityText;
    public TMP_Text energyText;

    [Header("Texts (derived)")]
    public TMP_Text hpText;
    public TMP_Text manaText;
    public TMP_Text damageText;
    public TMP_Text defenseText;
    public TMP_Text speedText;

    [Header("Plus Buttons")]
    public Button strengthPlus;
    public Button agilityPlus;
    public Button vitalityPlus;
    public Button energyPlus;

    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.OpenCharacter.performed += OnToggle;

        if (playerLevel == null) playerLevel = FindFirstObjectByType<PlayerLevel>();
        if (stats == null) stats = FindFirstObjectByType<PlayerStats>();
        if (derived == null) derived = FindFirstObjectByType<PlayerDerivedStats>();
        if (health == null) health = FindFirstObjectByType<PlayerHealth>();
        if (mana == null) mana = FindFirstObjectByType<PlayerMana>();

        if (playerLevel != null)
        {
            playerLevel.OnXPChanged += Refresh;
            playerLevel.OnLevelUp += Refresh;
        }

        if (stats != null)
            stats.OnStatsChanged += Refresh;

        if (derived != null)
        {
            derived.OnDerivedChanged += Refresh;
        }
        else
        {
            Debug.LogWarning("CharacterPanelUI: PlayerDerivedStats no encontrado");
        }

        if (health != null)
            health.OnHealthChanged += OnHealthUI;

        if (mana != null)
            mana.OnManaChanged += OnManaUI;

        // Botones +
        if (strengthPlus != null) strengthPlus.onClick.AddListener(() => { if (stats != null) stats.SpendPointStrength(); });
        if (agilityPlus != null) agilityPlus.onClick.AddListener(() => { if (stats != null) stats.SpendPointAgility(); });
        if (vitalityPlus != null) vitalityPlus.onClick.AddListener(() => { if (stats != null) stats.SpendPointVitality(); });
        if (energyPlus != null) energyPlus.onClick.AddListener(() => { if (stats != null) stats.SpendPointEnergy(); });

        Refresh();
    }

    void OnDisable()
    {
        inputActions.Player.OpenCharacter.performed -= OnToggle;
        inputActions.Player.Disable();

        if (playerLevel != null)
        {
            playerLevel.OnXPChanged -= Refresh;
            playerLevel.OnLevelUp -= Refresh;
        }

        if (stats != null) stats.OnStatsChanged -= Refresh;
        if (derived != null) derived.OnDerivedChanged -= Refresh;

        if (health != null) health.OnHealthChanged -= OnHealthUI;
        if (mana != null) mana.OnManaChanged -= OnManaUI;

        if (strengthPlus != null) strengthPlus.onClick.RemoveAllListeners();
        if (agilityPlus != null) agilityPlus.onClick.RemoveAllListeners();
        if (vitalityPlus != null) vitalityPlus.onClick.RemoveAllListeners();
        if (energyPlus != null) energyPlus.onClick.RemoveAllListeners();
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        Refresh();
    }

    void OnToggle(InputAction.CallbackContext ctx) => Toggle();

    public void Toggle()
    {
        if (panel == null) return;
        panel.SetActive(!panel.activeSelf);
        if (panel.activeSelf) Refresh();
    }

    // Llega por evento de health
    void OnHealthUI(int cur, int max)
    {
        if (hpText != null) hpText.text = $"HP: {cur}/{max}";
    }

    // Llega por evento de mana
    void OnManaUI(int cur, int max)
    {
        if (manaText != null) manaText.text = $"Mana: {cur}/{max}";
    }

    public void Refresh()
    {
        if (playerLevel != null)
        {
            if (levelText != null) levelText.text = $"Level: {playerLevel.level}";
            if (xpText != null) xpText.text = $"XP: {playerLevel.currentXP}/{playerLevel.XPToNext}";
        }

        if (stats != null)
        {
            if (pointsText != null) pointsText.text = $"Points: {stats.points}";
            if (strengthText != null) strengthText.text = $"Strength: {stats.strength}";
            if (agilityText != null) agilityText.text = $"Agility: {stats.agility}";
            if (vitalityText != null) vitalityText.text = $"Vitality: {stats.vitality}";
            if (energyText != null) energyText.text = $"Energy: {stats.energy}";

            bool canSpend = stats.points > 0;
            if (strengthPlus != null) strengthPlus.interactable = canSpend;
            if (agilityPlus != null) agilityPlus.interactable = canSpend;
            if (vitalityPlus != null) vitalityPlus.interactable = canSpend;
            if (energyPlus != null) energyPlus.interactable = canSpend;
        }

        if (derived != null)
        {
            if (damageText != null) damageText.text = $"Damage: {derived.MeleeDamage}";
            if (defenseText != null) defenseText.text = $"Defense: {derived.Defense}";
            if (speedText != null) speedText.text = $"Speed: {derived.MoveSpeed:0.00}";

            // Si todavía no llegaron los eventos (Start order), mostramos algo igual:
            if (health != null && hpText != null) hpText.text = $"HP: {health.currentHealth}/{health.maxHealth}";
            if (mana != null && manaText != null) manaText.text = $"Mana: {mana.currentMana}/{mana.maxMana}";
        }
    }
}
