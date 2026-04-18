using UnityEngine;
using TMPro;

public class DemonRitualUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject demonRitualPanel; // El panel visual (puede desactivarse)
    [SerializeField] private DemonRitualAltar altar;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI counterText;

    private Transform playerTransform;
    private bool panelActive = false;

    void Start()
    {
        // Buscar al jugador por tag
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerObject != null ? playerObject.transform : null;

        // Si panel no está asignado, usar este gameObject
        if (demonRitualPanel == null)
            demonRitualPanel = gameObject;

        Debug.Log($"🔍 DemonRitualUI - Player: {(playerTransform != null ? "✅" : "❌")}");
        Debug.Log($"🔍 DemonRitualUI - Altar: {(altar != null ? "✅" : "❌")}");
        Debug.Log($"🔍 DemonRitualUI - Panel: {(demonRitualPanel != null ? "✅" : "❌")}");

        if (altar != null)
        {
            altar.OnHeadsCollected += OnHeadsCollected;
            altar.OnDemonSpawned += OnDemonSpawned;
            altar.OnAltarReset += OnAltarReset;
        }

        // Inicialmente desactivado
        demonRitualPanel.SetActive(false);
        panelActive = false;
    }

    void Update()
    {
        if (altar == null || playerTransform == null)
            return;

        // Calcular distancia
        float distance = Vector3.Distance(playerTransform.position, altar.transform.position);
        bool isNear = distance <= altar.GetInteractionRange(); // Usar rango del altar
        bool shouldShow = isNear && !altar.IsRitualComplete();

        // Cambiar estado del panel
        if (shouldShow != panelActive)
        {
            demonRitualPanel.SetActive(shouldShow);
            panelActive = shouldShow;
            Debug.Log($"📍 Panel: {(shouldShow ? "✅ ACTIVADO" : "❌ DESACTIVADO")} - Distancia: {distance:F1}m, Ritual completado: {altar.IsRitualComplete()}");
        }

        // Actualizar UI
        if (shouldShow)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        int heads = altar.GetHeadsCollected();
        
        if (instructionText != null)
            instructionText.text = altar.IsRitualComplete() ? "F to invoke Demon" : "F to put head";
        
        if (counterText != null)
            counterText.text = $"{heads}/5";
    }

    private void OnHeadsCollected()
    {
        Debug.Log("🎉 Ritual completado!");
        if (instructionText != null)
            instructionText.text = "F to invoke Demon";
    }

    private void OnDemonSpawned()
    {
        Debug.Log("💀 Demon invocado!");
        demonRitualPanel.SetActive(false);
        panelActive = false;
    }

    private void OnAltarReset()
    {
        Debug.Log("🔄 Altar reseteado! UI reiniciado.");
        // El panel se reactivará automáticamente en Update() cuando el jugador esté cerca
        if (instructionText != null)
            instructionText.text = "F to put head";
        if (counterText != null)
            counterText.text = "0/5";
    }

    void OnDisable()
    {
        if (altar != null)
        {
            altar.OnHeadsCollected -= OnHeadsCollected;
            altar.OnDemonSpawned -= OnDemonSpawned;
            altar.OnAltarReset -= OnAltarReset;
        }
    }
}
