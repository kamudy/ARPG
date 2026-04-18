using UnityEngine;

/// <summary>
/// Sistema de patrulla para enemigos
/// Se integra con EnemyMeleeAttack/EnemyRangedAttack
/// </summary>
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] patrolPoints;           // Puntos de patrulla
    public float patrolSpeed = 1.5f;           // Velocidad de patrulla (menor que combate)
    public float returnRunSpeed = 4.5f;        // Velocidad al volver al punto de patrulla tras combate
    public float stoppingDistance = 0.3f;      // Distancia para considerar que llegó al punto
    public float waitTimeAtPoint = 1f;         // Segundos en esperar en cada punto
    public bool createDebugSpheres = true;     // Visualizar puntos en scene

    [Header("State")]
    public bool isPatrolling = true;

    [Header("Gravity")]
    public float gravedad = -9.81f;

    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private CharacterController controller;
    private Vector3 initialPosition;
    private Animator animator;
    private float verticalVelocity = 0f;
    private bool isInCombatMode = false;
    private bool isReturningToPatrolPoint = false;

    void Start()
    {
        // Obtener CharacterController si existe
        controller = GetComponent<CharacterController>();

        // Obtener Animator para animaciones
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning("⚠️ EnemyPatrol: No se encontró Animator en el enemigo");

        // Si no hay puntos de patrulla, usar la posición inicial como solo punto
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            initialPosition = transform.position;
            patrolPoints = new Transform[1];
            // Crear un punto virtual en la posición inicial
            GameObject pointGO = new GameObject("PatrolPoint_0");
            pointGO.transform.position = initialPosition;
            pointGO.transform.SetParent(transform.parent);
            patrolPoints[0] = pointGO.transform;
            Debug.LogWarning("⚠️ EnemyPatrol: No hay puntos de patrulla asignados. Usando posición inicial.");
        }

        // Debug: crear esferas para visualizar puntos
        if (createDebugSpheres)
        {
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                {
                    Debug.Log($"📍 Patrol point en: {point.position}");
                }
            }
        }
    }

    void Update()
    {
        // No patrullar si está inactivo
        if (!gameObject.activeInHierarchy) return;

        // Aplicar gravedad
        verticalVelocity += gravedad * Time.deltaTime;
        
        // Limitar velocidad terminal (velocidad máxima hacia abajo)
        if (verticalVelocity < -5f)
            verticalVelocity = -5f;
        
        if (controller != null && controller.enabled && controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = 0f;
        }

        // SEGUIR DESNIVELES: Raycast hacia abajo para pegar al terreno
        Ray rayDown = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(rayDown, out RaycastHit hit, 10f))
        {
            float terrainHeight = hit.point.y;
            float desiredHeight = terrainHeight + 0.1f;  // Mínimo offset encima del terreno
            
            if (controller != null && controller.enabled)
            {
                controller.enabled = false;
                transform.position = new Vector3(transform.position.x, desiredHeight, transform.position.z);
                controller.enabled = true;
            }
            else
                transform.position = new Vector3(transform.position.x, desiredHeight, transform.position.z);
                
            verticalVelocity = 0f;
        }

        if (isInCombatMode)
        {
            isPatrolling = false;
            // El script de combate controla movimiento/animacion
            
            // Aplicar gravedad incluso en combate
            if (controller != null && controller.enabled)
                controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
            else
                transform.position += Vector3.up * verticalVelocity * Time.deltaTime;

            return;
        }

        isPatrolling = true;

        // Si no está en combate, patrullar
        if (isPatrolling && patrolPoints != null && patrolPoints.Length > 0)
        {
            if (isReturningToPatrolPoint)
                ReturnToPatrolPoint();
            else
                Patrol();
        }

        // Aplicar gravedad al final
        if (controller != null && controller.enabled)
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        else
            transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
    }

    void Patrol()
    {
        if (patrolPoints[currentPatrolIndex] == null)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector3 direction = targetPoint.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;

        // Esperar en el punto
        if (isWaiting)
        {
            // Parar animación cuando espera
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
            }
            
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
            return;
        }

        // Llego al punto
        if (distance < stoppingDistance)
        {
            isWaiting = true;
            waitTimer = waitTimeAtPoint;
            // Parar animación cuando llega
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
            }
            return;
        }

        // Mover hacia el punto - Con animación
        Vector3 moveDirection = direction.normalized;
        
        // Activar animación de caminar 🚶
        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
        }
        
        if (controller != null && controller.enabled)
        {
            controller.Move(moveDirection * patrolSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += moveDirection * patrolSpeed * Time.deltaTime;
        }

        // Rotar hacia el punto
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
    }

    void ReturnToPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            isReturningToPatrolPoint = false;
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        if (targetPoint == null)
        {
            isReturningToPatrolPoint = false;
            return;
        }

        Vector3 direction = targetPoint.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance <= stoppingDistance)
        {
            isReturningToPatrolPoint = false;
            isWaiting = true;
            waitTimer = waitTimeAtPoint;
            if (animator != null)
            {
                animator.SetBool("isRunning", false);
                animator.SetBool("isWalking", false);
            }
            return;
        }

        Vector3 moveDirection = direction.normalized;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
        }

        if (controller != null && controller.enabled)
            controller.Move(moveDirection * returnRunSpeed * Time.deltaTime);
        else
            transform.position += moveDirection * returnRunSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.deltaTime);
    }

    public void ResumePatrol()
    {
        isPatrolling = true;
        isWaiting = false;
        waitTimer = 0;
        isInCombatMode = false;
        isReturningToPatrolPoint = true;
    }

    public void StopPatrol()
    {
        isPatrolling = false;
        isReturningToPatrolPoint = false;
        // Detener animación cuando se detiene la patrulla
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }
    }

    public void SetCombatMode(bool combatMode)
    {
        isInCombatMode = combatMode;

        if (combatMode)
        {
            StopPatrol();
            return;
        }

        ResumePatrol();
    }

    // Debug Gizmos
    void OnDrawGizmosSelected()
    {
        if (patrolPoints == null) return;

        // Dibujar puntos
        Gizmos.color = Color.green;
        foreach (Transform point in patrolPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.3f);
            }
        }

        // Dibujar línea entre puntos
        Gizmos.color = Color.cyan;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] != null && patrolPoints[(i + 1) % patrolPoints.Length] != null)
            {
                Gizmos.DrawLine(
                    patrolPoints[i].position,
                    patrolPoints[(i + 1) % patrolPoints.Length].position
                );
            }
        }

    }
}
