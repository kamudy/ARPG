using UnityEngine;

/// <summary>Sistema de animación basado en Blend Tree 1D con parámetro "speed"</summary>
public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private PlayerClickMovement playerMovement;
    private Rigidbody rb;
    private CharacterController charController;

    [Header("Speed System (Blend Tree 1D)")]
    private int speedHash = Animator.StringToHash("speed");
    [SerializeField] private float maxSpeed = 6.5f;
    [SerializeField] private float speedDamping = 0.12f;
    private float currentSpeedVelocity = 0f;
    
    [Header("Attack Animation Hashes")]
    private int attackHash = Animator.StringToHash("attack");
    private int slashHash = Animator.StringToHash("ataka2");
    private int deathHash = Animator.StringToHash("death");
    private int basicAttack1Hash = Animator.StringToHash("basicAttack1");
    private int basicAttack2Hash = Animator.StringToHash("basicAttack2");
    private int skill1Hash = Animator.StringToHash("skill1");
    private int skill2Hash = Animator.StringToHash("skill2");
    private int skill3Hash = Animator.StringToHash("skill3");
    private int dashHash = Animator.StringToHash("dash");

    private int lastBasicAttackUsed = 0;

    void Awake()
    {
        FindAnimator();
    }

    void Start()
    {
        playerMovement = GetComponent<PlayerClickMovement>();
        rb = GetComponent<Rigidbody>();
        charController = GetComponent<CharacterController>();

        if (animator == null)
        {
            Debug.LogError("❌ PlayerAnimator: No se encontró Animator");
            enabled = false;
            return;
        }

        animator.applyRootMotion = false;
        Debug.Log("✅ PlayerAnimator inicializado - Speed Blend Tree 1D");
        PrintAnimatorParameters();
    }

    private void FindAnimator()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Transform horseman = transform.Find("horseman");
            if (horseman != null)
                animator = horseman.GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (animator == null)
            return;
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        float currentVelocity = GetCharacterVelocity();
        float targetSpeed = Mathf.Clamp01(currentVelocity / maxSpeed);
        
        float smoothedSpeed = Mathf.SmoothDamp(
            animator.GetFloat(speedHash),
            targetSpeed,
            ref currentSpeedVelocity,
            speedDamping
        );
        
        animator.SetFloat(speedHash, smoothedSpeed);
    }

    private float GetCharacterVelocity()
    {
        if (charController != null && charController.enabled)
        {
            if (playerMovement != null)
                return playerMovement.GetCurrentVelocity();
            return 0f;
        }

        if (rb != null)
            return rb.linearVelocity.magnitude;

        return 0f;
    }

    #region ===== ATTACK ANIMATIONS =====

    public void PlaySlash()
    {
        if (animator != null)
            animator.SetTrigger(slashHash);
    }

    public void PlayBasicAttack()
    {
        if (animator != null)
        {
            if (lastBasicAttackUsed == 0)
            {
                animator.SetTrigger(basicAttack1Hash);
                lastBasicAttackUsed = 1;
            }
            else
            {
                animator.SetTrigger(basicAttack2Hash);
                lastBasicAttackUsed = 0;
            }
        }
    }

    public void PlaySkill1() { if (animator != null) animator.SetTrigger(skill1Hash); }
    public void PlaySkill2() { if (animator != null) animator.SetTrigger(skill2Hash); }
    public void PlaySkill3() { if (animator != null) animator.SetTrigger(skill3Hash); }
    public void PlayDash() { if (animator != null) animator.SetTrigger(dashHash); }
    public void PlayDeath() { if (animator != null) animator.SetTrigger(deathHash); }

    #endregion

    #region ===== UTILITY =====

    public void ResetAnimator()
    {
        if (animator == null) return;
        animator.SetFloat(speedHash, 0f);
        animator.Rebind();
        animator.Update(0f);
    }

    public float GetCurrentAnimationDuration()
    {
        if (animator == null) return 0.6f;
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }

    private void PrintAnimatorParameters()
    {
        if (animator == null) return;
        AnimatorControllerParameter[] parameters = animator.parameters;
        Debug.Log($"📊 Animator parámetros ({parameters.Length}):");
        foreach (var param in parameters)
            Debug.Log($"  - {param.name} ({param.type})");
    }

    #endregion
}
