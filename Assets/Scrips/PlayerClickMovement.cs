using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerClickMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;         // base (si no hay derived)
    public float runSpeed = 6f;          // velocidad al correr (Shift) - debe ser ~1.5x moveSpeed
    public float rotationSpeed = 12f;
    public float stopDistance = 1.6f;

    [Header("Attack")]
    public int attackDamage = 25;        // base (si no hay derived)
    public float attackCooldown = 1.2f;  // Cooldown entre ataques
    public float attackDelay = 0.15f;    // Retraso para sincronizar daño con animación
    public GameObject impactVFXPrefab;   // Efecto de impacto al golpear
    public Vector3 impactOffset = new Vector3(0f, 1.0f, 0f);
    public bool isAttacking;

    [Header("Dash")]
    public float dashSpeed = 22f;        // Aumentado de 14f para mayor distancia
    public float dashDuration = 0.25f;   // Aumentado de 0.15f para mayor recorrido
    public float dashCooldown = 0.9f;
    public float minDashMouseDistance = 0.6f;
    public GameObject dashShieldPrefab;  // Prefab del escudo visual

    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    [Header("Gravity")]
    public float gravedad = -9.81f;

    private PlayerDerivedStats derived;

    private CharacterController controller;
    private Vector3 targetPosition;
    private bool hasTarget;

    private Enemy currentEnemy;
    private PlayerInputActions inputActions;
    private PlayerHealth playerHealth;
    private PlayerAnimator playerAnimator;
    private PlayerSlashSkill playerSlashSkill;
    private PlayerSkill1 playerSkill1;
    private PlayerSkill2 playerSkill2;
    private PlayerSkill3 playerSkill3;

    // Running
    private bool isRunning = false;

    // Click queue
    private bool clickQueued;
    private Vector2 queuedMousePos;

    // Dash
    private bool isDashing;
    private float dashEndTime;
    private float nextDashTime;
    private Vector3 dashDirection;
    private GameObject dashShieldInstance;  // Instancia del escudo durante el dash

    // Attack cooldown
    private float nextAttackTime;

    // Mouse world pos
    private Vector3 lastMouseWorldPos;
    private Vector3 lastMoveDirection = Vector3.forward;

    // Estado
    private bool isAlive = true;
    private float verticalVelocity = 0f;  // Para gravedad
    private float currentMovementSpeed = 0f; // Para animaciones (velocidad actual del personaje)

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        targetPosition = transform.position;

        inputActions = new PlayerInputActions();
        derived = GetComponent<PlayerDerivedStats>();
        playerHealth = GetComponent<PlayerHealth>();
        playerAnimator = GetComponent<PlayerAnimator>();
        playerSlashSkill = GetComponent<PlayerSlashSkill>();
        playerSkill1 = GetComponent<PlayerSkill1>();
        playerSkill2 = GetComponent<PlayerSkill2>();
        playerSkill3 = GetComponent<PlayerSkill3>();

        // Debug
        Debug.Log($"✅ PlayerClickMovement Awake: Skill1={playerSkill1 != null}, Skill2={playerSkill2 != null}, Skill3={playerSkill3 != null}");
        if (playerAnimator == null)
            Debug.LogError("❌ PlayerClickMovement: PlayerAnimator NO ENCONTRADO");
        else
            Debug.Log("✅ PlayerClickMovement: PlayerAnimator ENCONTRADO correctamente");
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Click.performed += OnClick;
        inputActions.Player.Dash.performed += OnDash;

        // Suscribirse al evento de muerte
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath += OnPlayerDeath;
        }
    }

    void OnDisable()
    {
        inputActions.Player.Click.performed -= OnClick;
        inputActions.Player.Dash.performed -= OnDash;
        inputActions.Player.Disable();

        // Desuscribirse del evento de muerte
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath -= OnPlayerDeath;
        }
    }

    void Update()
    {
        // No hacer nada si el jugador está muerto
        if (!isAlive)
            return;

        // Detectar Running con Shift
        isRunning = Keyboard.current.shiftKey.isPressed;
        float currentSpeed = isRunning ? runSpeed : moveSpeed;

        // PRIMERO: Raycast AGRESIVO para corregir antes que nada
        Ray rayDown = new Ray(transform.position + Vector3.up * 1f, Vector3.down);
        if (Physics.Raycast(rayDown, out RaycastHit hit, 3f, groundLayer, QueryTriggerInteraction.Ignore))
        {
            float terrainHeight = hit.point.y;
            float playerBottom = controller.bounds.min.y;
            
            // Si el player ATRAVIESA el terreno, levantarlo MUCHO
            if (playerBottom < terrainHeight)
            {
                float correction = (terrainHeight - playerBottom) + controller.skinWidth;
                controller.Move(Vector3.up * correction);
                verticalVelocity = 0f;
            }
        }

        // Aplicar gravedad 
        verticalVelocity += gravedad * Time.deltaTime;
        
        // Si está en el suelo, resetear velocidad vertical
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = 0f;  // Cero cuando está en el suelo
        }

        UpdateMouseWorldPosition();

        // Fallback: asegurar click izquierdo aunque el callback del Input Action no dispare.
        if (!clickQueued && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            clickQueued = true;
            queuedMousePos = Mouse.current.position.ReadValue();
        }

        // Dash tiene prioridad
        if (isDashing)
        {
            UpdateDash();
            return;
        }

        // Procesar click en Update
        if (clickQueued)
        {
            clickQueued = false;
            HandleClick(queuedMousePos);
        }

        HandleMovement(currentSpeed);
        
        // Input: Tecla 1 para Skill1
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Debug.Log("🔑 Key 1 presionada");
            if (playerSkill1 != null)
                playerSkill1.TryUseSkill1();
            else
                Debug.LogWarning("❌ playerSkill1 es NULL!");
        }
        
        // Input: Tecla 2 para Skill2
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Debug.Log("🔑 Key 2 presionada");
            if (playerSkill2 != null)
                playerSkill2.TryUseSkill2();
            else
                Debug.LogWarning("❌ playerSkill2 es NULL!");
        }
        
        // Input: Tecla 3 para Skill3
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            Debug.Log("🔑 Key 3 presionada");
            if (playerSkill3 != null)
                playerSkill3.TryUseSkill3();
            else
                Debug.LogWarning("❌ playerSkill3 es NULL!");
        }
        
        // Aplicar gravedad SIEMPRE (separado del movimiento XZ)
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    // =========================
    void OnClick(InputAction.CallbackContext context)
    {
        clickQueued = true;
        queuedMousePos = Mouse.current.position.ReadValue();
    }

    void OnDash(InputAction.CallbackContext context)
    {
        TryStartDash();
    }

    // =========================
    // MOUSE WORLD POS
    // =========================
    void UpdateMouseWorldPosition()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer))
        {
            lastMouseWorldPos = hit.point;
        }
    }

    // =========================
    // CLICK PROCESS
    // =========================
    void HandleClick(Vector2 mousePos)
    {
        // Ignorar clicks sobre UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, ~0, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
            return;

        float nearestEnemyDistance = float.MaxValue;
        Enemy nearestEnemy = null;
        float nearestGroundDistance = float.MaxValue;
        Vector3 nearestGroundPoint = Vector3.zero;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider == null)
                continue;

            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                if (hit.distance < nearestEnemyDistance)
                {
                    nearestEnemyDistance = hit.distance;
                    nearestEnemy = enemy;
                }

                continue;
            }

            if (IsInLayerMask(hit.collider.gameObject.layer, groundLayer) && hit.distance < nearestGroundDistance)
            {
                nearestGroundDistance = hit.distance;
                nearestGroundPoint = hit.point;
            }
        }

        if (nearestEnemy != null)
        {
            currentEnemy = nearestEnemy;
            targetPosition = currentEnemy.transform.position;
            hasTarget = true;
            return;
        }

        if (nearestGroundDistance < float.MaxValue)
        {
            currentEnemy = null;
            targetPosition = nearestGroundPoint;
            hasTarget = true;
        }
    }

    bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    // =========================
    // MOVEMENT + ATTACK
    // =========================
    void HandleMovement(float currentSpeed)
    {
        CorrectHeightOverEnemies();

        // Bloquear movimiento mientras se ataca
        if (isAttacking)
        {
            return;
        }

        if (!hasTarget)
        {
            // Animación controlada automáticamente por parámetro 'speed' (= 0)
            return;
        }

        Vector3 destination = (currentEnemy != null) ? currentEnemy.transform.position : targetPosition;

        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        // ATACAR
        if (currentEnemy != null && distance <= stopDistance)
        {
            FaceEnemyInstant(currentEnemy.transform.position);
            hasTarget = false;
            currentMovementSpeed = 0f; // Reset velocidad para Idle automático
            Attack();
            return;
        }

        // MOVER
        if (distance > stopDistance)
        {
            Vector3 dirNorm = direction.normalized;

            // ✅ Usar currentSpeed (afectado por running) en lugar de moveSpeed base
            float speed = currentSpeed;

            // Verificar si hay un enemigo bloqueando el camino
            if (!IsEnemyBlocking(dirNorm))
            {
                // Solo MOVIMIENTO XZ sin gravedad (gravedad aplicada en Update())
                Vector3 moveDir = new Vector3(dirNorm.x * speed, 0, dirNorm.z * speed);
                controller.Move(moveDir * Time.deltaTime);
                
                // Actualizar velocidad actual para animaciones
                currentMovementSpeed = speed;
            }
            else
            {
                // Enemigo bloqueando - reducir velocidad
                currentMovementSpeed = speed * 0.3f;
            }
            
            RotateTowards(direction);

            // Animación controlada por parámetro 'speed' de forma automática

            lastMoveDirection = dirNorm;
        }
        else
        {
            // Llegó al destino: parar y quedarse quieto
            currentMovementSpeed = 0f;
            hasTarget = false;
        }
    }

    bool IsEnemyBlocking(Vector3 direction)
    {
        // Raycast preventivo para detectar si hay un enemigo bloqueando
        Ray ray = new Ray(transform.position, direction);
        
        if (Physics.Raycast(ray, out RaycastHit hit, stopDistance + 0.5f, enemyLayer))
        {
            // Si detecta un enemigo muy cerca, bloquear movimiento
            if (hit.distance < 0.8f)
            {
                return true;
            }
        }
        
        return false;
    }

    void CorrectHeightOverEnemies()
    {
        // DESACTIVADO: Causaba desplazamientos horizontales no deseados
        // Si necesitas evitar que el player atraviese enemigos, usar Physics.IgnoreCollision en su lugar
        /*
        Ray ray = new Ray(transform.position, Vector3.down);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 3f, enemyLayer))
        {
            float distanceToEnemy = Vector3.Distance(transform.position, hit.point);
            
            if (distanceToEnemy < 0.5f)
            {
                Vector3 awayFromEnemy = (transform.position - hit.point).normalized;
                awayFromEnemy.y = 0;
                controller.Move(awayFromEnemy * 0.3f);
            }
        }
        */
    }

    void Attack()
    {
        if (currentEnemy == null)
        {
            Debug.Log("❌ Attack: No hay enemigo objetivo");
            return;
        }

        FaceEnemyInstant(currentEnemy.transform.position);

        // Verificar cooldown
        if (Time.time < nextAttackTime)
        {
            Debug.Log("⏱️ Ataque en cooldown...");
            return;
        }

        Debug.Log($"⚔️ ATAQUE BÁSICO - Distancia: {Vector3.Distance(transform.position, currentEnemy.transform.position)}, Animator: {playerAnimator}");

        nextAttackTime = Time.time + attackCooldown;
        isAttacking = true;

        // Animar ataque alternando entre basicAttack1 y basicAttack2
        if (playerAnimator != null)
        {
            playerAnimator.PlayBasicAttack();
        }
        else
            Debug.LogError("❌ Attack: PlayerAnimator es NULL");

        // Retrasar el daño e impacto para sincronizar con la animación
        Enemy enemyToHit = currentEnemy;
        currentEnemy = null;
        StartCoroutine(DelayedAttackDamage(enemyToHit));
    }

    void FaceEnemyInstant(Vector3 enemyPosition)
    {
        Vector3 lookDir = enemyPosition - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(lookDir.normalized);
    }

    System.Collections.IEnumerator DelayedAttackDamage(Enemy enemy)
    {
        // Esperar hasta que se aplique el daño (en medio de la animación)
        yield return new WaitForSeconds(attackDelay);

        if (enemy != null)
        {
            // ✅ Strength afecta daño
            int dmg = (derived != null) ? derived.MeleeDamage : attackDamage;

            // Aplicar daño
            enemy.TakeDamage(dmg);

            // Spawnear efecto de impacto
            SpawnImpact(enemy.transform.position);

            Debug.Log($"⚔️ Ataque básico! Daño: {dmg}");
        }

        // Apagar el flag de ataque inmediatamente después del daño
        isAttacking = false;
    }

    void SpawnImpact(Vector3 enemyPos)
    {
        if (impactVFXPrefab == null) return;

        Vector3 pos = enemyPos + impactOffset;
        Instantiate(impactVFXPrefab, pos, Quaternion.identity);
    }

    void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // =========================
    // DASH (AL MOUSE)
    // =========================
    void TryStartDash()
    {
        if (Time.time < nextDashTime) return;
        if (isDashing) return;
        // ✅ CRITICO: Prevenir dash si hay un skill/ataque activo
        if (isAttacking) return;

        hasTarget = false;
        currentEnemy = null;

        Vector3 dir = lastMouseWorldPos - transform.position;
        dir.y = 0f;

        // Si mouse cerca, usar última dirección útil
        if (dir.magnitude < minDashMouseDistance)
            dir = lastMoveDirection;

        if (dir.sqrMagnitude < 0.01f)
            dir = transform.forward;

        dashDirection = dir.normalized;

        transform.rotation = Quaternion.LookRotation(dashDirection);

        // ✅ ANIMAR DASH
        if (playerAnimator != null)
            playerAnimator.PlayDash();

        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;

        // Activar invulnerabilidad durante el dash
        if (playerHealth != null)
        {
            playerHealth.SetInvulnerable(true);
            Debug.Log("🏃 Dash iniciado - INVULNERABLE");
        }

        // Crear el escudo visual
        if (dashShieldPrefab != null)
        {
            dashShieldInstance = Instantiate(dashShieldPrefab, transform.position, Quaternion.identity);
            dashShieldInstance.transform.SetParent(transform); // Que siga al player
            Debug.Log("🛡️ Escudo de dash creado");
        }
        else
        {
            Debug.LogWarning("⚠️ dashShieldPrefab no asignado en PlayerClickMovement");
        }
    }

    void UpdateDash()
    {
        // Mantener el escudo sincronizado con el player
        if (dashShieldInstance != null)
        {
            dashShieldInstance.transform.position = transform.position;
        }

        // Solo MOVIMIENTO XZ del dash (gravedad aplicada en Update())
        Vector3 dashMove = new Vector3(dashDirection.x * dashSpeed, 0, dashDirection.z * dashSpeed);
        CollisionFlags flags = controller.Move(dashMove * Time.deltaTime);

        // cortar si chocamos adelante/lado
        if ((flags & CollisionFlags.Sides) != 0 || (flags & CollisionFlags.CollidedAbove) != 0)
        {
            isDashing = false;
            currentMovementSpeed = 0f;
            
            // Desactivar invulnerabilidad cuando se corta el dash
            if (playerHealth != null)
                playerHealth.SetInvulnerable(false);
            
            // Destruir escudo
            if (dashShieldInstance != null)
            {
                Destroy(dashShieldInstance);
                dashShieldInstance = null;
            }
            return;
        }

        if (Time.time >= dashEndTime)
        {
            isDashing = false;
            currentMovementSpeed = 0f;
            
            // Desactivar invulnerabilidad cuando termina el dash
            if (playerHealth != null)
                playerHealth.SetInvulnerable(false);
            
            // Destruir escudo
            if (dashShieldInstance != null)
            {
                Destroy(dashShieldInstance);
                dashShieldInstance = null;
                Debug.Log("🏃 Dash terminado - invulnerabilidad desactivada");
            }
        }
    }

    private void OnPlayerDeath()
    {
        isAlive = false;
        
        // Desactivar invulnerabilidad si está activa
        if (playerHealth != null)
            playerHealth.SetInvulnerable(false);
        
        if (playerAnimator != null)
            playerAnimator.PlayDeath();
        Debug.Log("PlayerClickMovement: Jugador muerto, movimiento deshabilitado");
    }

    public void ResetDeathState()
    {
        isAlive = true;
        currentMovementSpeed = 0f;
        if (playerAnimator != null)
        {
            playerAnimator.ResetAnimator();
        }
        Debug.Log("PlayerClickMovement: Movimiento rehabilitado, animación reseteada");
    }

    /// <summary>Obtiene la velocidad actual del movimiento para usar en animaciones (Blend Tree)</summary>
    public float GetCurrentVelocity()
    {
        return currentMovementSpeed;
    }
}
