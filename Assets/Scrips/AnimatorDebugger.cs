using UnityEngine;

public class AnimatorDebugger : MonoBehaviour
{
    private Animator animator;
    private EnemyMeleeAttack enemyMelee;

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyMelee = GetComponent<EnemyMeleeAttack>();

        if (animator == null)
        {
            Debug.LogError($"❌ {gameObject.name}: NO TIENE ANIMATOR");
        }
        else
        {
            Debug.Log($"✅ {gameObject.name}: Animator encontrado");
            Debug.Log($"   Controller asignado: {animator.runtimeAnimatorController.name}");
            
            // Listar parámetros
            Debug.Log($"   Parámetros disponibles:");
            foreach (var param in animator.parameters)
            {
                Debug.Log($"     - {param.name} ({param.type})");
            }
        }

        if (enemyMelee == null)
        {
            Debug.LogError($"❌ {gameObject.name}: NO TIENE EnemyMeleeAttack");
        }
        else if (enemyMelee.animator == null)
        {
            Debug.LogError($"⚠️ {gameObject.name}: EnemyMeleeAttack.animator está NULL");
        }
        else
        {
            Debug.Log($"✅ {gameObject.name}: EnemyMeleeAttack.animator está asignado");
        }
    }

    void Update()
    {
        if (animator != null && Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log($"\n=== ESTADO ACTUAL DE {gameObject.name} ===");
            Debug.Log($"Estado actual: {animator.GetCurrentAnimatorStateInfo(0).fullPathHash}");
            
            foreach (var param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Bool)
                {
                    Debug.Log($"  {param.name} = {animator.GetBool(param.name)}");
                }
                else if (param.type == AnimatorControllerParameterType.Float)
                {
                    Debug.Log($"  {param.name} = {animator.GetFloat(param.name)}");
                }
                else if (param.type == AnimatorControllerParameterType.Int)
                {
                    Debug.Log($"  {param.name} = {animator.GetInteger(param.name)}");
                }
            }
        }
    }
}
