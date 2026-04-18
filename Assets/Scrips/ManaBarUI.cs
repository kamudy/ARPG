using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaBarUI : MonoBehaviour
{
    public PlayerMana playerMana;

    public Slider manaSlider;
    public TMP_Text manaText;

    void Start()
    {
        if (playerMana == null)
            playerMana = FindFirstObjectByType<PlayerMana>();

        if (playerMana != null)
            playerMana.OnManaChanged += OnManaChanged;

        Refresh();
    }

    void OnDestroy()
    {
        if (playerMana != null)
            playerMana.OnManaChanged -= OnManaChanged;
    }

    void OnManaChanged(int current, int max)
    {
        Refresh();
    }

    void Refresh()
    {
        if (playerMana == null) return;

        float pct = (playerMana.maxMana > 0) ? (float)playerMana.currentMana / playerMana.maxMana : 0f;

        if (manaSlider != null)
            manaSlider.value = Mathf.Clamp01(pct);

        if (manaText != null)
            manaText.text = $"Mana: {playerMana.currentMana}/{playerMana.maxMana}";
    }
}
