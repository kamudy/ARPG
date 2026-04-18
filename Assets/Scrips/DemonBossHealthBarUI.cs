using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DemonBossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject demonBossHealthPanel; // Panel padre
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private DemonRitualAltar altar;
    
    private DemonEnemy currentDemon;

    void Start()
    {
        Debug.Log("🟢 DemonBossHealthBarUI inicializado");
        
        // Si no está asignado el panel, usar este gameobject
        if (demonBossHealthPanel == null)
            demonBossHealthPanel = gameObject;
        
        if (altar != null)
        {
            altar.OnDemonSpawned += OnDemonSpawned;
            altar.OnAltarReset += OnAltarReset;
            Debug.Log("✅ Suscrito a eventos del altar");
        }
        else
        {
            Debug.LogError("❌ Altar NO asignado en DemonBossHealthBarUI");
        }

        // Inicialmente desactivado
        demonBossHealthPanel.SetActive(false);
    }

    private void OnDemonSpawned()
    {
        Debug.Log("🔴 OnDemonSpawned: Buscando Demon...");
        
        currentDemon = FindFirstObjectByType<DemonEnemy>();
        
        if (currentDemon != null)
        {
            // Suscribirse al evento de cambio de salud
            currentDemon.OnHealthChanged += UpdateUI;
            
            // Actualización inicial
            UpdateUI(currentDemon.currentHealth, currentDemon.maxHealth);
            
            demonBossHealthPanel.SetActive(true);
            Debug.Log($"✅ Barra conectada al Demon: HP {currentDemon.currentHealth}/{currentDemon.maxHealth}");
        }
        else
        {
            Debug.LogError("❌ No se encontró DemonEnemy en la escena");
        }
    }

    private void OnAltarReset()
    {
        Debug.Log("🔴 OnAltarReset: Desactivando barra");
        
        // Desuscribirse del evento
        if (currentDemon != null)
        {
            currentDemon.OnHealthChanged -= UpdateUI;
        }
        
        demonBossHealthPanel.SetActive(false);
        currentDemon = null;
    }

    private void UpdateUI(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
        {
            healthText.text = $"ER DIABLO HP: {current}/{max}";
        }
    }

    void OnDisable()
    {
        if (altar != null)
        {
            altar.OnDemonSpawned -= OnDemonSpawned;
            altar.OnAltarReset -= OnAltarReset;
        }
        
        if (currentDemon != null)
        {
            currentDemon.OnHealthChanged -= UpdateUI;
        }
    }
}
