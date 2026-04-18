using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth health;
    public Slider healthSlider;
    public TMP_Text healthText;

    void OnEnable()
    {
        if (health != null)
            health.OnHealthChanged += UpdateUI;
    }

    void OnDisable()
    {
        if (health != null)
            health.OnHealthChanged -= UpdateUI;
    }

    void Start()
    {
        // ✅ Forzar una actualización inicial por si el evento ya pasó en Start del Player
        if (health != null)
            UpdateUI(health.currentHealth, health.maxHealth);
    }

    void UpdateUI(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
        {
            healthText.text = $"HP {current}/{max}";
        }
    }
}
