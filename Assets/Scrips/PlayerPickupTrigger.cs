using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PlayerPickupTrigger : MonoBehaviour
{
    [Header("Pickup Range")]
    public float pickupRange = 2.5f;

    [Header("Fallback Scan")]
    public float scanInterval = 0.12f;
    public LayerMask pickupLayers = ~0;

    private PlayerInventory inventory;
    private SphereCollider triggerCollider;
    private float nextScanTime;

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        triggerCollider = GetComponent<SphereCollider>();

        if (triggerCollider == null)
        {
            Debug.LogError("❌ PlayerPickupTrigger: No se encontró SphereCollider");
            return;
        }

        // Configurar el trigger
        triggerCollider.radius = pickupRange;
        triggerCollider.center = Vector3.zero;
        triggerCollider.isTrigger = true;

        Debug.Log($"✅ PlayerPickupTrigger: Configurado con radio {pickupRange}m");
    }

    void Update()
    {
        if (inventory == null) return;
        if (Time.time < nextScanTime) return;

        nextScanTime = Time.time + scanInterval;

        // Fallback para configuraciones de física donde OnTrigger no entra de forma consistente.
        Collider[] nearby = Physics.OverlapSphere(transform.position, pickupRange, pickupLayers, QueryTriggerInteraction.Collide);
        for (int i = 0; i < nearby.Length; i++)
        {
            if (TryPickupFromCollider(nearby[i]))
                return;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        TryPickupFromCollider(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryPickupFromCollider(other);
    }

    private bool TryPickupFromCollider(Collider other)
    {
        if (other == null || inventory == null)
            return false;

        Coin coin = FindPickupComponent<Coin>(other);
        if (coin != null)
        {
            inventory.AddCoins(coin.value);
            Debug.Log($"💰 +{coin.value} monedas recogidas!");
            Destroy(coin.gameObject);
            return true;
        }

        ItemPickup itemPickup = FindPickupComponent<ItemPickup>(other);
        if (itemPickup != null)
        {
            if (itemPickup.item == null)
            {
                Debug.LogError($"❌ ItemPickup sin item asignado en {itemPickup.gameObject.name}");
                return true;
            }

            Debug.Log($"🧪 Recogiendo item: {itemPickup.item.name}");
            // ✅ Solo destruir si el item fue añadido exitosamente al inventario
            bool added = inventory.AddItem(itemPickup.item);
            if (added)
            {
                Destroy(itemPickup.gameObject);
                Debug.Log($"✅ Item {itemPickup.item.name} añadido al inventario");
            }
            else
            {
                Debug.LogWarning($"⚠️ Inventario lleno. {itemPickup.item.name} permanece en el suelo");
            }
            return added;  // ✅ Devolver si fue exitoso
        }

        return false;
    }

    private T FindPickupComponent<T>(Collider col) where T : Component
    {
        if (col == null) return null;

        T c = col.GetComponent<T>();
        if (c != null) return c;

        c = col.GetComponentInParent<T>();
        if (c != null) return c;

        c = col.GetComponentInChildren<T>(true);
        return c;
    }
}
