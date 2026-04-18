using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private ItemData headDropItem;
    [SerializeField] private GameObject headDropPrefab; // Prefab visua de la cabeza
    [SerializeField] private float headDropChance = 0.10f; // 10% chance para cabeza
    
    public int maxHealth = 100;

    public int xpReward = 20;
    public int currentHealth; // Público para que el UI pueda acceder
    
    // Evento para notificar cambios de salud - protected para que se pueda invocar desde subclases
    public event Action<int, int> OnHealthChanged;

    public EnemyHealthBar healthBar;

    private Vector3 initialPosition;
    [SerializeField] private float respawnDelay = 10f;
    protected bool isAlive = true;

    protected virtual void Start()
    {
        initialPosition = transform.position;
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }
    }

    void OnDisable()
    {
        // No cancelar Respawn aquí; el enemigo se desactiva al morir y debe poder reaparecer.
    }

    public void TakeDamage(int damage)
    {
        // No tomar daño si el enemigo no está vivo o está desactivado
        if (!isAlive || !gameObject.activeSelf)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        // Disparar evento de cambio de salud
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"💥 {gameObject.name}: Daño recibido {damage}. Salud: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void SetHeadDropItem(ItemData head)
    {
        headDropItem = head;
    }

    public void SetHeadDropPrefab(GameObject prefab)
    {
        headDropPrefab = prefab;
    }

    /// <summary>Método protegido para que subclases puedan invocar el evento</summary>
    protected void InvokeHealthChanged(int current, int max)
    {
        OnHealthChanged?.Invoke(current, max);
    }

    void Die()
    {
        DieInternal();
    }

    protected virtual void DieInternal()
    {
        isAlive = false;
        Debug.Log($"{gameObject.name} ha muerto. Respawneará en {respawnDelay}s");

        // Permite atravesar el cadáver inmediatamente.
        SetCollisionEnabled(false);

        // Reproducir animación de muerte
        EnemyMeleeAttack meleeAttack = GetComponent<EnemyMeleeAttack>();
        if (meleeAttack != null)
        {
            meleeAttack.PlayDeathAnimation();
        }

        EnemyRangedAttack rangedAttack = GetComponent<EnemyRangedAttack>();
        if (rangedAttack != null)
        {
            rangedAttack.PlayDeathAnimation();
        }

        // Dar XP al player
        PlayerLevel pl = FindFirstObjectByType<PlayerLevel>();
        if (pl != null)
            pl.AddXP(xpReward);
        // Dar regen al player    
        PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
        if (ph != null) ph.Heal(3);

        PlayerMana pm = FindFirstObjectByType<PlayerMana>();
        if (pm != null) pm.Regen(4);    
        
        DropLootInternal();

        // Programar respawn usando Invoke (funciona incluso cuando está inactivo)
        Invoke(nameof(Respawn), respawnDelay);
    }

    private void Respawn()
    {
        Debug.Log($"{gameObject.name}: Respawneando en {initialPosition}");
        
        // Resetear posición y salud
        transform.position = initialPosition;
        currentHealth = maxHealth;
        isAlive = true;

        // Actualizar la barra de salud
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        // Restaurar colisiones para el enemigo vivo.
        SetCollisionEnabled(true);

        // Reactivar el enemigo
        gameObject.SetActive(true);
    }

    protected void SetCollisionEnabled(bool enabled)
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = enabled;

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            col.enabled = enabled;
        }
    }

    void DropLoot()
    {
        DropLootInternal();
    }

    protected virtual void DropLootInternal()
    {
        // Monedas
        if (coinPrefab != null)
        {
            int coinsToDrop = UnityEngine.Random.Range(1, 4);

            for (int i = 0; i < coinsToDrop; i++)
            {
                Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * 0.5f;
                pos.y = transform.position.y;

                Instantiate(coinPrefab, pos, Quaternion.identity);
            }
        }

        // Cabeza ritual (bajo chance)
        if (headDropItem != null && headDropPrefab != null && UnityEngine.Random.value < headDropChance)
        {
            Vector3 headPos = transform.position + Vector3.up * 0.5f;
            GameObject headObject = Instantiate(headDropPrefab, headPos, Quaternion.identity);
            headObject.name = "HeadDrop_" + headDropItem.itemName;
            
            ItemPickup pickup = headObject.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.item = headDropItem;
            }
            
            Debug.Log($"🎃 ¡{headDropItem.itemName} obtenida de {gameObject.name}!");
        }

        // Item (30% chance)
        if (itemPrefabs != null && itemPrefabs.Length > 0 && UnityEngine.Random.value < 0.3f)
        {
            Vector3 pos = transform.position;
            Instantiate(itemPrefabs[UnityEngine.Random.Range(0, itemPrefabs.Length)], pos, Quaternion.identity);
        }
    }   
}
