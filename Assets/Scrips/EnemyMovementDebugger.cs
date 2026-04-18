using UnityEngine;

/// <summary>
/// Script de diagnóstico para detectar problemas de sincronización entre
/// movimiento y animación en enemigos
/// </summary>
public class EnemyMovementDebugger : MonoBehaviour
{
    private Animator animator;
    private Vector3 lastPosition;
    private string activeScripts = "";
    private float distanceMovedLastFrame = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        lastPosition = transform.position;
        
        // Detectar qué scripts de movimiento están activos
        EnemyMeleeAttack melee = GetComponent<EnemyMeleeAttack>();
        EnemyRangedAttack ranged = GetComponent<EnemyRangedAttack>();
        EnemyPatrol patrol = GetComponent<EnemyPatrol>();

        activeScripts = "";
        if (melee != null && melee.enabled) activeScripts += "EnemyMeleeAttack ";
        if (ranged != null && ranged.enabled) activeScripts += "EnemyRangedAttack ";
        if (patrol != null && patrol.enabled) activeScripts += "EnemyPatrol ";

        Debug.Log($"✅ {gameObject.name} - Debugger iniciado");
        Debug.Log($"   Scripts activos: {(string.IsNullOrEmpty(activeScripts) ? "NINGUNO" : activeScripts)}");
        
        if (animator == null)
            Debug.LogError($"❌ {gameObject.name}: No tiene Animator asignado!");
        else
            Debug.Log($"   Animator encontrado: {animator.runtimeAnimatorController.name}");
    }

    void Update()
    {
        if (animator == null) return;

        // Medir distancia movida
        distanceMovedLastFrame = Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        // Obtener estado del animator
        bool isWalking = animator.GetBool("isWalking");
        bool isAttacking = animator.GetBool("isAttacking");
        
        // Detectar problemas
        if (isWalking && distanceMovedLastFrame < 0.001f)
        {
            Debug.LogWarning($"⚠️ {gameObject.name}: Animación WALK pero SIN MOVIMIENTO (camina en el lugar)");
        }
        
        if (distanceMovedLastFrame > 0.01f && !isWalking && !isAttacking)
        {
            Debug.LogWarning($"⚠️ {gameObject.name}: MOVIMIENTO SIN ANIMACIÓN (isWalking={isWalking}, isAttacking={isAttacking})");
        }

        // Debug cada X frames
        if (Time.frameCount % 60 == 0)  // Cada 60 frames (aproximadamente cada segundo a 60 FPS)
        {
            Debug.Log($"📊 {gameObject.name} Estado:");
            Debug.Log($"   isWalking: {isWalking}, isAttacking: {isAttacking}");
            Debug.Log($"   Posición: {transform.position}");
            Debug.Log($"   Distancia movida último frame: {distanceMovedLastFrame:F4}");
        }
    }

    // Presionar D en el juego para un diagnóstico completo
    public void PrintDiagnostics()
    {
        Debug.Log($"\n=== DIAGNÓSTICO COMPLETO: {gameObject.name} ===");
        Debug.Log($"Scripts activos: {(string.IsNullOrEmpty(activeScripts) ? "NINGUNO (❌ PROBLEMA!)" : activeScripts)}");
        
        if (animator != null)
        {
            Debug.Log($"Animator: {animator.runtimeAnimatorController.name}");
            Debug.Log($"  - isWalking: {animator.GetBool("isWalking")}");
            Debug.Log($"  - isAttacking: {animator.GetBool("isAttacking")}");
            Debug.Log($"  - Hash estado actual: {animator.GetCurrentAnimatorStateInfo(0).fullPathHash}");
        }
        else
            Debug.LogError($"❌ No hay Animator!");

        Debug.Log($"Posición: {transform.position}");
        Debug.Log($"Distancia movida: {distanceMovedLastFrame:F4}");
        Debug.Log($"=== FIN DIAGNÓSTICO ===\n");
    }
}
