using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPBarUI : MonoBehaviour
{
    public PlayerLevel playerLevel;

    public Slider xpSlider;
    public TMP_Text xpText;
    public TMP_Text levelText;

    void Start()
    {
        if (playerLevel == null)
            playerLevel = FindFirstObjectByType<PlayerLevel>();

        if (playerLevel != null)
        {
            playerLevel.OnXPChanged += Refresh;
            playerLevel.OnLevelUp += Refresh;
        }

        Refresh();
    }

    void OnDestroy()
    {
        if (playerLevel != null)
        {
            playerLevel.OnXPChanged -= Refresh;
            playerLevel.OnLevelUp -= Refresh;
        }
    }

    void Refresh()
    {
        if (playerLevel == null) return;

        float pct = playerLevel.XPPercent();

        if (xpSlider != null)
            xpSlider.value = pct;

        if (xpText != null)
            xpText.text = $"XP: {playerLevel.currentXP}/{playerLevel.XPToNext}";

        if (levelText != null)
            levelText.text = $"Lvl {playerLevel.level}";
    }
}
