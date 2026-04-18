using UnityEngine;

/// <summary>Marcador para una moneda en el suelo. La recogida la maneja PlayerPickupTrigger.</summary>
public class Coin : MonoBehaviour
{
    public int value = 1;
    [SerializeField] private float rotateSpeed = 120f;

    void Update()
    {
        // Animación de rotación
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
}
