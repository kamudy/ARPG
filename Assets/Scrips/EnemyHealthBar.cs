using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    private Slider slider;

    void Awake()
    {
        slider = GetComponentInChildren<Slider>();

        if (slider == null)
        {
            Debug.LogError("EnemyHealthBar: NO se encontró un Slider hijo");
        }
    }

    public void SetHealth(int current, int max)
    {
        if (slider == null) return;

        slider.maxValue = max;
        slider.value = current;
    }
}
