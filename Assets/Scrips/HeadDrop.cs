using UnityEngine;

public class HeadDrop : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;

    private Vector3 startPosition;
    private ItemPickup itemPickup;

    void Start()
    {
        startPosition = transform.position;
        itemPickup = GetComponent<ItemPickup>();
        
        if (itemPickup != null && itemPickup.item != null)
        {
            Debug.Log($"🎃 Cabeza drop: {itemPickup.item.itemName}");
        }
    }

    void Update()
    {
        // Rotación visual
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Movimiento de flotación
        Vector3 newPos = startPosition;
        newPos.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = newPos;
    }

    void OnTriggerEnter(Collider other)
    {
        // Puede ser usado para efectos visuales cuando se recoge
        if (other.CompareTag("Player"))
        {
            Debug.Log($"💀 Cabeza siendo recogida...");
        }
    }
}
