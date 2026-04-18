using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class DeathMessageUI : MonoBehaviour
{
    public static DeathMessageUI Instance { get; private set; }

    public TMP_Text deathMessageText;
    public float displayDuration = 3f;
    public float fadeDuration = 1f;

    private CanvasGroup canvasGroup;
    private Graphic graphic;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Desabilitar raycast del texto para que no bloquee clicks
        if (deathMessageText != null)
        {
            graphic = deathMessageText.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.raycastTarget = false;
            }
        }

        // Empezar invisible
        canvasGroup.alpha = 0f;

        // Buscar el jugador y suscribirse al evento de muerte
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDeath += ShowDeathMessage;
            }
        }
    }

    private void ShowDeathMessage()
    {
        StartCoroutine(AnimateDeathMessage());
    }

    private IEnumerator AnimateDeathMessage()
    {
        if (deathMessageText != null)
        {
            deathMessageText.text = "¡HAS MUERTO!";
        }

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Esperar a que se muestre
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}
