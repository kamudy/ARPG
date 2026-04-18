using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Helper para resetear el personaje a nivel 1 con stats base y sin inventario.
/// Puedes usar esto desde el Inspector o llamarlo programáticamente.
/// </summary>
public class PlayerResetHelper : MonoBehaviour
{
    private Keyboard keyboard;

    void OnEnable()
    {
        keyboard = Keyboard.current;
    }

    /// <summary>
    /// Ejecuta el reseteo del personaje.
    /// </summary>
    public void ResetPlayer()
    {
        if (SaveManager.instance != null)
        {
            SaveManager.instance.ResetPlayerCharacter();
        }
        else
        {
            Debug.LogError("SaveManager no encontrado. Asegúrate de que esté en la escena.");
        }
    }

    // Atajo de teclado: Ctrl + R para resetear
    void Update()
    {
        if (keyboard != null && keyboard.ctrlKey.isPressed && keyboard.rKey.wasPressedThisFrame)
        {
            Debug.Log("🔄 Presionaste Ctrl + R - Resetando personaje...");
            ResetPlayer();
        }
    }
}
