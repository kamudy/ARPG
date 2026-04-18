using UnityEngine;

public class OgreDemonEnemy : MonoBehaviour
{
    [Header("Ogre Demon Stats")]
    [SerializeField] private int ogreDemonMaxHealth = 150;
    [SerializeField] private int ogreDemonXPReward = 50;
    
    [Header("Combat")]
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private float attackCooldown = 1.5f;
    
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.8f;
    
    [Header("Loot")]
    [SerializeField] private ItemData ogreDemonHeadDrop;
    [SerializeField] private GameObject ogreDemonHeadPrefab;

    private Enemy enemyBase;

    void Start()
    {
        enemyBase = GetComponent<Enemy>();
        if (enemyBase != null)
        {
            enemyBase.maxHealth = ogreDemonMaxHealth;
            enemyBase.xpReward = ogreDemonXPReward;
            
            // Asignar la cabeza del Ogre Demon para drop
            enemyBase.SetHeadDropItem(ogreDemonHeadDrop);
            if (ogreDemonHeadPrefab != null)
                enemyBase.SetHeadDropPrefab(ogreDemonHeadPrefab);
        }
        
        Debug.Log($"👹 Ogre Demon spawn - HP: {ogreDemonMaxHealth}, DMG: {attackDamage}");
    }

    public int GetAttackDamage() => attackDamage;
    public float GetDetectionRange() => detectionRange;
    public float GetAttackRange() => attackRange;
    public float GetAttackCooldown() => attackCooldown;
    public float GetWalkSpeed() => walkSpeed;
    public float GetStopDistance() => stopDistance;
}
