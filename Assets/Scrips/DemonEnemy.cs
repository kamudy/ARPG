using System;
using UnityEngine;

public class DemonEnemy : Enemy
{
    [Header("Demon Stats")]
    [SerializeField] private int demonMaxHealth = 250;
    [SerializeField] private int demonDamage = 35;
    [SerializeField] private float demonSpeed = 3.5f;
    [SerializeField] private float demonDetectionRange = 10f;
    [SerializeField] private int demonXPReward = 100;

    [Header("Demon Drops")]
    [SerializeField] private ItemData demonHeartDrop; // Drop especial legendario
    [SerializeField] private float demonHeartDropChance = 0.01f; // 1%

    public event Action OnDemonDeath;

    protected override void Start()
    {
        // Aplicar stats del Demon ANTES del Start de la clase base
        maxHealth = demonMaxHealth;
        xpReward = demonXPReward;
        
        // Llamar al Start de Enemy para inicializar correctamente
        base.Start();
        
        // Reinicializar currentHealth con la nueva maxHealth del Demon
        currentHealth = maxHealth;
        
        // Disparar evento inicial para que el UI se actualice
        InvokeHealthChanged(currentHealth, maxHealth);
        
        Debug.Log($"💀 Demon invocado - MaxHP: {maxHealth}, CurrentHP: {currentHealth}, DMG: {demonDamage}");
    }

    protected override void DieInternal()
    {
        isAlive = false;
        Debug.Log($"💀 Demon ha muerto - NO respawneará automáticamente (solo con ritual)");

        // Desactivar colisiones
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

        // Dar rewards al player
        PlayerLevel pl = FindFirstObjectByType<PlayerLevel>();
        if (pl != null)
            pl.AddXP(xpReward);
        
        PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
        if (ph != null) ph.Heal(3);

        PlayerMana pm = FindFirstObjectByType<PlayerMana>();
        if (pm != null) pm.Regen(4);

        // Drop loot especial del Demon
        DropLootInternal();

        // ✅ DIFERENCIA: Invocar evento de muerte para que el DemonRitualAltar reset el altar
        OnDemonDeath?.Invoke();
        
        Debug.Log("💀 El Demon ha sido derrotado! Aguardando nueva invocación del ritual...");
        
        // ⚠️ NO llamar a base.DieInternal() para evitar el Invoke(Respawn)
        // El respawn solo ocurrirá cuando se complete el ritual de nuevo
    }

    public int GetDemonDamage() => demonDamage;
    public float GetDetectionRange() => demonDetectionRange;
    public float GetSpeed() => demonSpeed;

    protected override void DropLootInternal()
    {
        base.DropLootInternal();

        // Drop especial: Demon Heart (1% chance)
        if (demonHeartDrop != null && UnityEngine.Random.value < demonHeartDropChance)
        {
            Vector3 heartPos = transform.position + Vector3.up * 0.5f;
            GameObject heartObject = new GameObject("DemonHeart_Drop");
            heartObject.transform.position = heartPos;
            
            ItemPickup pickup = heartObject.AddComponent<ItemPickup>();
            pickup.item = demonHeartDrop;
            
            Rigidbody rb = heartObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            
            SphereCollider collider = heartObject.AddComponent<SphereCollider>();
            collider.radius = 0.3f;
            collider.isTrigger = true;
            
            Debug.Log($"💎 ¡DEMON HEART OBTENIDO!");
        }
    }

    public void OnDeathAnimation()
    {
        // Hook para reproducir animación de muerte especial si existe
        // Puede ser llamado por el Animator
    }
}
