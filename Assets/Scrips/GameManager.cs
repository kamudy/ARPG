using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Respawn")]
    public float respawnDelay = 2f;

    private PlayerHealth playerHealth;
    private Vector3 spawnPosition;

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
        // Buscar el jugador en la escena
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth>();
            spawnPosition = playerObject.transform.position;

            // Suscribirse al evento de muerte
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDeath += OnPlayerDeath;
            }
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath -= OnPlayerDeath;
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("GameManager detectó la muerte del jugador. Reiniciando...");
        Invoke(nameof(RespawnPlayer), respawnDelay);
    }

    private void RespawnPlayer()
    {
        // Resetear la vida del jugador
        if (playerHealth != null)
        {
            playerHealth.currentHealth = playerHealth.maxHealth;
            playerHealth.Notify();

            // Mover el jugador a la posición de spawn
            CharacterController controller = playerHealth.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                playerHealth.transform.position = spawnPosition;
                controller.enabled = true;
            }
            else
            {
                playerHealth.transform.position = spawnPosition;
            }
        }

        // Reactivar el movimiento del jugador
        PlayerClickMovement movement = playerHealth?.GetComponent<PlayerClickMovement>();
        if (movement != null)
        {
            movement.ResetDeathState();
        }

        Debug.Log("Jugador respawneado");
    }
}
