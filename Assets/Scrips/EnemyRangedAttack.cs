using UnityEngine;

public class EnemyRangedAttack : MonoBehaviour
{
    [Header("Target")]
    public float detectionRange = 12f;
    public float chaseRange = 40f;
    public float attackRange = 9f;
    public float chaseResumeDistance = 10.5f;
    public float stopDistance = 6f;

    [Header("Attack")]
    public int damage = 20;
    public float attackCooldown = 2f;
    public float attackHoldDuration = 0.45f;
    public float postShotHoldDuration = 0.18f;
    public float postShotBlendPadding = 0.05f;
    public bool fireFromAnimationEvent = true;
    public float fireEventTimeout = 0.35f;
    public float aimHeightOffset = 1.1f;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint; // Punto donde salen los proyectiles

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseRunSpeed = 3.8f;

    [Header("Gravity")]
    public float gravedad = -9.81f;

    [Header("Animation")]
    public Animator animator;

    [Header("Debug")]
    public bool logDamage = false;

    private Transform player;
    private PlayerHealth playerHealth;
    private PlayerDerivedStats playerDerived;
    private Collider playerCollider;
    private EnemyPatrol patrol;
    private float nextAttackTime;
    private float attackHoldUntil;
    private float verticalVelocity = 0f;
    private Vector3 aggroOrigin;
    private bool hasAggro = false;
    private bool attackPositionLocked = false;
    private Vector3 attackLockedPosition;
    private bool pendingShot = false;
    private float pendingShotTimeoutAt = 0f;

    // Estados
    private enum EnemyState { Idle, Walking, Attacking, Dead }
    private EnemyState currentState = EnemyState.Idle;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;

        patrol = GetComponent<EnemyPatrol>();

        // Si no hay punto de spawn asignado, usar la posición del enemigo
        if (projectileSpawnPoint == null)
            projectileSpawnPoint = transform;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null)
        {
            PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
            if (ph != null) p = ph.gameObject;
        }

        if (p != null)
        {
            player = p.transform;
            playerHealth = p.GetComponent<PlayerHealth>();
            playerDerived = p.GetComponent<PlayerDerivedStats>();
            playerCollider = p.GetComponent<Collider>();
            if (playerCollider == null)
                playerCollider = p.GetComponentInChildren<Collider>(true);
        }
    }

    void OnEnable()
    {
        hasAggro = false;
        currentState = EnemyState.Idle;
        attackPositionLocked = false;
        attackHoldUntil = 0f;
        nextAttackTime = 0f;
        pendingShot = false;
        pendingShotTimeoutAt = 0f;
        if (animator != null)
            animator.applyRootMotion = false;
        if (patrol == null)
            patrol = GetComponent<EnemyPatrol>();
        if (patrol != null)
            patrol.SetCombatMode(false);
    }

    void Update()
    {
        // No hacer nada si el enemigo está muerto
        if (currentState == EnemyState.Dead)
            return;

        // SEGUIR DESNIVELES: Raycast hacia abajo para pegar al terreno
        Ray rayDown = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(rayDown, out RaycastHit hit, 10f))
        {
            float terrainHeight = hit.point.y;
            float desiredHeight = terrainHeight + 0.1f;  // Mínimo offset encima del terreno
            transform.position = new Vector3(transform.position.x, desiredHeight, transform.position.z);
            verticalVelocity = 0f;
        }
        
        // Limitar velocidad terminal (velocidad máxima hacia abajo)
        if (verticalVelocity < -5f)
            verticalVelocity = -5f;

        // No hacer nada si el player está muerto o no existe
        if (player == null || playerHealth == null || playerHealth.currentHealth <= 0)
        {
            LoseAggro();
            return;
        }

        float dist = GetPlanarDistanceToPlayer();
        float distFromAggroOrigin = GetPlanarDistance(aggroOrigin, player.position);

        if (!hasAggro && dist <= detectionRange)
            GainAggro();

        if (hasAggro && distFromAggroOrigin > chaseRange)
        {
            LoseAggro();
            return;
        }

        if (!hasAggro)
        {
            SetState(EnemyState.Idle);
            // Fuera de combate, EnemyPatrol controla la animación de caminar.
            return;
        }

        if (Time.time < attackHoldUntil)
        {
            SetState(EnemyState.Attacking);
            BeginAttackPositionLock();
            ApplyAttackPositionLock();
            FacePlayer();
            UpdateAnimation();
            return;
        }

        bool shouldStandAndAttack = dist <= chaseResumeDistance || pendingShot;

        if (pendingShot && fireFromAnimationEvent && Time.time >= pendingShotTimeoutAt)
        {
            // Fallback para no perder disparos si el Animation Event no está configurado.
            SpawnProjectile();
        }

        if (!shouldStandAndAttack)
        {
            ReleaseAttackPositionLock();
            SetState(EnemyState.Walking);
            MoveTowardsPlayer();
            UpdateAnimation();
        }
        else
        {
            BeginAttackPositionLock();
            ApplyAttackPositionLock();
            FacePlayer();

            bool canAttackNow = pendingShot || Time.time >= nextAttackTime;
            SetState(canAttackNow ? EnemyState.Attacking : EnemyState.Idle);

            if (canAttackNow)
                TryAttack();

            UpdateAnimation();
        }
    }

    void GainAggro()
    {
        hasAggro = true;
        aggroOrigin = transform.position;
        if (patrol != null)
            patrol.SetCombatMode(true);
    }

    void LoseAggro()
    {
        hasAggro = false;
        ReleaseAttackPositionLock();
        SetState(EnemyState.Idle);
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", false);
        }
        if (patrol != null)
            patrol.SetCombatMode(false);
    }

    void SetState(EnemyState newState)
    {
        currentState = newState;
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = currentState == EnemyState.Walking;
        bool isAttacking = currentState == EnemyState.Attacking;

        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", isMoving);
        animator.SetBool("isAttacking", isAttacking);
    }

    void MoveTowardsPlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        if (dir.magnitude <= stopDistance)
            return;

        Vector3 step = dir.normalized * chaseRunSpeed * Time.deltaTime;
        transform.position += step;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.deltaTime);
    }

    float GetPlanarDistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return GetPlanarDistance(transform.position, player.position);
    }

    float GetPlanarDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, 8f * Time.deltaTime);
    }

    void BeginAttackPositionLock()
    {
        if (attackPositionLocked) return;
        attackPositionLocked = true;
        attackLockedPosition = transform.position;
    }

    void ApplyAttackPositionLock()
    {
        if (!attackPositionLocked) return;
        transform.position = new Vector3(attackLockedPosition.x, transform.position.y, attackLockedPosition.z);
    }

    void ReleaseAttackPositionLock()
    {
        attackPositionLocked = false;
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        if (pendingShot) return;

        if (projectilePrefab == null)
        {
            Debug.LogError($"[{gameObject.name}] NO HAY PROJECTILE PREFAB ASIGNADO!");
            return;
        }

        attackHoldUntil = Time.time + attackHoldDuration;

        if (fireFromAnimationEvent)
        {
            pendingShot = true;
            pendingShotTimeoutAt = Time.time + fireEventTimeout;
            return;
        }

        SpawnProjectile();
    }

    /// <summary>
    /// Llamar desde Animation Event en el clip de ataque para sincronizar el spawn del proyectil.
    /// </summary>
    public void AnimationEvent_FireProjectile()
    {
        if (!pendingShot && fireFromAnimationEvent)
            return;

        SpawnProjectile();
    }

    // Alias para Animation Events configurados con nombres cortos.
    public void FireProjectile()
    {
        AnimationEvent_FireProjectile();
    }

    public void AE_FireProjectile()
    {
        AnimationEvent_FireProjectile();
    }

    void SpawnProjectile()
    {
        nextAttackTime = Time.time + attackCooldown;
        float holdDuration = postShotHoldDuration;

        if (animator != null)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.length > 0.0001f)
            {
                float currentCycleProgress = state.normalizedTime % 1f;
                float remainingNormalized = Mathf.Clamp01(1f - currentCycleProgress);
                float remainingTime = remainingNormalized * state.length;
                holdDuration = Mathf.Max(holdDuration, remainingTime + postShotBlendPadding);
            }
        }

        attackHoldUntil = Mathf.Max(attackHoldUntil, Time.time + holdDuration);
        pendingShot = false;
        pendingShotTimeoutAt = 0f;

        // Instanciar el proyectil
        Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        
        Debug.Log($"[{gameObject.name}] ¡DISPARANDO! Posición: {spawnPos}, Daño: {damage}");
        
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        
        if (projectile == null)
        {
            Debug.LogError($"[{gameObject.name}] Fallo al instanciar projectile!");
            return;
        }

        Debug.Log($"[{gameObject.name}] Projectile instanciado: {projectile.name}");

        // Obtener el componente Arrow y configurarlo
        Arrow arrow = projectile.GetComponent<Arrow>();
        if (arrow != null)
        {
            Vector3 aimPoint = GetPlayerAimPoint();
            Vector3 direction = (aimPoint - spawnPos).normalized;
            arrow.Launch(direction, damage, playerDerived, transform);
            Debug.Log($"[{gameObject.name}] Arrow.Launch() llamado");
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] ¡Projectile NO TIENE componente Arrow!");
            Destroy(projectile);
        }
    }

    Vector3 GetPlayerAimPoint()
    {
        if (player == null)
            return transform.position + transform.forward;

        if (playerCollider == null)
        {
            playerCollider = player.GetComponent<Collider>();
            if (playerCollider == null)
                playerCollider = player.GetComponentInChildren<Collider>(true);
        }

        if (playerCollider != null)
            return playerCollider.bounds.center;

        return player.position + Vector3.up * aimHeightOffset;
    }

    public void PlayDeathAnimation()
    {
        hasAggro = false;
        ReleaseAttackPositionLock();
        currentState = EnemyState.Dead;
        if (patrol != null)
            patrol.SetCombatMode(true);

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", false);
            animator.SetTrigger("Death");
            
            // Espera a que termine la animación de muerte (4.8s + buffer)
            StartCoroutine(DisableAfterAnimation(5.2f));
        }
        else
        {
            Invoke(nameof(DisableGameObject), 5.2f);
        }
    }

    private AnimationClip GetDeathAnimationClip()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return null;

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.ToLower().Contains("death") || clip.name.ToLower().Contains("dead"))
                return clip;
        }
        return null;
    }

    private System.Collections.IEnumerator DisableAfterAnimation(float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        DisableGameObject();
    }

    private void DisableGameObject()
    {
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
