using UnityEngine;

public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("Target")]
    public float detectionRange = 9f;
    public float chaseRange = 30f;
    public float attackRange = 2.2f;
    public float chaseResumeDistance = 2.8f;

    [Header("Attack")]
    public int damage = 10;
    public float attackCooldown = 1.2f;
    public float attackHoldDuration = 0.45f;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float chaseRunSpeed = 4.5f;
    public float stopDistance = 1.8f;

    [Header("Gravity")]
    public float gravedad = -9.81f;

    [Header("Animation")]
    public Animator animator;

    [Header("Debug")]
    public bool logDamage = false;

    private Transform player;
    private PlayerHealth playerHealth;
    private PlayerDerivedStats playerDerived;
    private EnemyPatrol patrol;
    private float nextAttackTime;
    private float attackHoldUntil;
    private float verticalVelocity = 0f;
    private Vector3 aggroOrigin;
    private bool hasAggro = false;
    private bool attackPositionLocked = false;
    private Vector3 attackLockedPosition;

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
        }
    }

    void OnEnable()
    {
        hasAggro = false;
        currentState = EnemyState.Idle;
        attackPositionLocked = false;
        attackHoldUntil = 0f;
        nextAttackTime = 0f;
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

        bool shouldStandAndAttack = dist <= chaseResumeDistance || Time.time < nextAttackTime;

        if (!shouldStandAndAttack)
        {
            ReleaseAttackPositionLock();
            SetState(EnemyState.Walking);
            MoveTowardsPlayer();
            UpdateAnimation();
        }
        else
        {
            SetState(EnemyState.Attacking);
            BeginAttackPositionLock();
            ApplyAttackPositionLock();
            FacePlayer();
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

    void FacePlayer()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.deltaTime);
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

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;
        attackHoldUntil = Time.time + attackHoldDuration;

        int defense = (playerDerived != null) ? playerDerived.Defense : 0;
        int finalDamage = Mathf.Max(1, damage - defense);

        playerHealth.TakeDamage(finalDamage);

        if (logDamage)
            UnityEngine.Debug.Log($"Enemy hit -> {finalDamage} damage");
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
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
