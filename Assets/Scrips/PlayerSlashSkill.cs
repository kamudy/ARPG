using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerSlashSkill : MonoBehaviour
{
    [Header("Input")]
    public PlayerInputActions inputActions;

    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    [Header("Slash")]
    public float radius = 2.0f;
    public int damage = 40;
    public float cooldown = 0.6f;
    public int manaCost = 10;
    public float slashDelay = 0.1f;         // ✅ Cuándo se realiza el daño
    public float impactVFXDelay = 0.15f;    // ✅ Cuándo aparece el impact VFX (puede ser diferente)
    public float animationDuration = 0.6f;  // Duración de la animación (ajusta según tu clip)
    public float slashDistance = 1.5f;      // Distancia hacia adelante del player para el VFX
    public float slashVFXScale = 1.0f;      // Escala del VFX (aumenta para hacerlo más grande)

    [Header("VFX")]
    public GameObject slashVFXPrefab;     // (opcional) tu SlashArcVFX LineRenderer
    public GameObject impactVFXPrefab;    // (obligatorio para impacto)
    public Vector3 impactOffset = new Vector3(0f, 1.0f, 0f);

    [Header("UI")]
    public InventoryUI inventoryUI;

    [Header("Debug")]
    public bool logHits = true;

    private float nextUseTime;
    private List<Vector3> lastHitPositions = new List<Vector3>();  // ✅ Guardar posiciones de enemigos golpeados

    // Referencias
    private PlayerMana mana; // tu script de mana (debe tener Spend(int))
    private PlayerAnimator playerAnimator;
    private PlayerClickMovement playerClickMovement;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        mana = GetComponent<PlayerMana>();
        playerAnimator = GetComponent<PlayerAnimator>();
        playerClickMovement = GetComponent<PlayerClickMovement>();

        if (inputActions == null)
            inputActions = new PlayerInputActions();

        // Auto-encontrar InventoryUI si no está asignado
        if (inventoryUI == null)
            inventoryUI = FindFirstObjectByType<InventoryUI>();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    public void TryUseSlash()
    {
        if (Time.time < nextUseTime) return;
        // ✅ CRITICO: Prevenir que se active otro skill si ya hay uno activo
        if (playerClickMovement != null && playerClickMovement.isAttacking) return;

        // gastar mana
        if (mana != null)
        {
            if (!mana.Spend(manaCost))
            {
                if (logHits) Debug.Log("No hay mana para Slash.");
                return;
            }
        }

        nextUseTime = Time.time + cooldown;

        // 0) Rotar player hacia el mouse (inmediatamente)
        RotateToMouse();

        // 1) Bloquear movimiento
        if (playerClickMovement != null)
            playerClickMovement.isAttacking = true;

        // 2) Animar ataque slash (inmediatamente)
        if (playerAnimator != null)
        {
            playerAnimator.PlaySlash();
        }

        // 3) Retrasar VFX y daño para sincronizar con la animación
        StartCoroutine(DelayedSlashEffect());
    }

    System.Collections.IEnumerator DelayedSlashEffect()
    {
        // Limpiar lista de hits previos
        lastHitPositions.Clear();

        // ===== PRIMER EVENTO: IMPACTO + DAÑO =====
        yield return new WaitForSeconds(slashDelay);

        // VFX de Slash (el arco visual)
        SpawnSlashVFX();

        // Realizar daño en enemigos cercanos (guarda posiciones en lastHitPositions)
        int hits = DoDamageAndImpact();

        if (logHits) Debug.Log($"⚔️  Slash - Hit realizado: {hits} enemigos");

        // ===== SEGUNDO EVENTO: IMPACT VFX =====
        yield return new WaitForSeconds(impactVFXDelay - slashDelay);

        // ✅ Spawnear impact VFX en las posiciones guardadas
        SpawnStoredImpacts();

        if (logHits) Debug.Log($"💥 Slash - Impact VFX mostrado");

        // ✅ Esperar a que la animación termine completamente antes de permitir otro ataque
        yield return new WaitForSeconds(animationDuration - impactVFXDelay);

        // Apagar el flag de ataque cuando termina la animación
        if (playerClickMovement != null)
            playerClickMovement.isAttacking = false;
    }

    void RotateToMouse()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer))
        {
            Vector3 dir = hit.point - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
                if (logHits) Debug.Log($"✓ Player rotado hacia mouse. Forward: {transform.forward}");
            }
        }
        else
        {
            // Fallback: rotar hacia la posición del mouse en pantalla si raycast falla
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            if (mouseWorldPos != Vector3.zero)
            {
                Vector3 dir = mouseWorldPos - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(dir);
                    if (logHits) Debug.Log($"⚠ Raycast falló. Usado fallback. Forward: {transform.forward}");
                }
            }
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        // Obtener posición del mouse a altura del player respecto a la cámara
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        // Plano a la altura del player
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        
        if (playerPlane.Raycast(ray, out float enter))
        {
            return ray.origin + ray.direction * enter;
        }
        
        return Vector3.zero;
    }

    void SpawnSlashVFX()
    {
        if (slashVFXPrefab == null) return;

        // Spawn adelante del player, rotado hacia la dirección del player
        Vector3 spawnPos = transform.position + transform.forward * slashDistance;
        Quaternion slashRot = Quaternion.LookRotation(transform.forward);
        GameObject vfx = Instantiate(slashVFXPrefab, spawnPos, slashRot);
        vfx.transform.localScale = Vector3.one * slashVFXScale;
        
        // Log de diagnóstico
        if (logHits) Debug.Log($"📍 Slash VFX spawneado en {spawnPos} con rotación {slashRot.eulerAngles}");
    }

    int DoDamageAndImpact()
    {
        // Centro del hit delante del player (MU-like)
        Vector3 center = transform.position + transform.forward * 1.0f + Vector3.up * 0.9f;

        Collider[] cols = Physics.OverlapSphere(center, radius, enemyLayer);

        int hitCount = 0;

        for (int i = 0; i < cols.Length; i++)
        {
            Enemy enemy = cols[i].GetComponentInParent<Enemy>();
            if (enemy == null) continue;

            enemy.TakeDamage(damage);
            hitCount++;

            // ✅ Solo guardar posición, NO spawnear impact aquí
            lastHitPositions.Add(enemy.transform.position);
        }

        return hitCount;
    }

    // ✅ NUEVO: Spawnear impacts en las posiciones guardadas
    void SpawnStoredImpacts()
    {
        foreach (Vector3 pos in lastHitPositions)
        {
            SpawnImpact(pos);
        }
    }

    void SpawnImpact(Vector3 enemyPos)
    {
        if (impactVFXPrefab == null) return;

        Vector3 pos = enemyPos + impactOffset;
        Instantiate(impactVFXPrefab, pos, Quaternion.identity);
    }

    // Gizmo para ver el área del slash
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + transform.forward * 1.0f + Vector3.up * 0.9f;
        Gizmos.DrawWireSphere(center, radius);
    }
}
