using UnityEngine;

/// <summary>
/// Controlador para el VFX del slash que respeta la rotación del player
/// Compatible con cualquier tipo de VFX (Particles, meshes, líneas, etc.)
/// </summary>
public class SlashVFXController : MonoBehaviour
{
    [Header("Life")]
    public float lifeTime = 0.5f;

    private float dieTime;

    void Awake()
    {
        dieTime = Time.time + lifeTime;
        Debug.Log($"✓ SlashVFX instanciado - Pos: {transform.position}, Forward: {transform.forward}");
    }

    void Update()
    {
        // Destruir cuando expire el tiempo de vida
        if (Time.time >= dieTime)
        {
            Destroy(gameObject);
        }
    }
}

