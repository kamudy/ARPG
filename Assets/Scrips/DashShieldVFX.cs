using UnityEngine;

/// <summary>
/// Efecto visual de escudo para el dash
/// Se instancia durante el dash y se destruye cuando termina
/// </summary>
public class DashShieldVFX : MonoBehaviour
{
    [Header("Visual")]
    public float rotationSpeed = 360f;   // Rotación del escudo (grados/segundo)
    public float scalePulse = 0.2f;      // Amplitud de pulsación (0 = sin pulsación)
    public float pulseSpeed = 8f;        // Velocidad de pulsación

    [Header("Material")]
    public Material shieldMaterial;      // Material del escudo (debe ser transparente)

    private Vector3 initialScale;
    private float lifeTime;
    private float startTime;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        initialScale = transform.localScale;
        startTime = Time.time;
        meshRenderer = GetComponent<MeshRenderer>();

        // El escudo es solo visual: no debe colisionar ni afectar cámara/player.
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Si no hay material asignado, intentar usar el del mesh renderer
        if (shieldMaterial == null && meshRenderer != null)
        {
            shieldMaterial = meshRenderer.material;
        }

        Debug.Log($"🛡️ DashShield creado en posición {transform.position}");
    }

    void Update()
    {
        // Rotar el escudo
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // Pulsación de escala (opcional)
        if (scalePulse > 0)
        {
            float pulse = Mathf.Sin((Time.time - startTime) * pulseSpeed) * scalePulse;
            transform.localScale = initialScale * (1f + pulse);
        }

        // Fade out opcional (descomenta si quieres que se desvanezca)
        // if (meshRenderer != null && shieldMaterial != null)
        // {
        //     float elapsedTime = Time.time - startTime;
        //     float alpha = Mathf.Clamp01(1f - (elapsedTime / lifeTime));
        //     shieldMaterial.color = new Color(shieldMaterial.color.r, shieldMaterial.color.g, shieldMaterial.color.b, alpha);
        // }
    }

    public void SetLifeTime(float duration)
    {
        lifeTime = duration;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
