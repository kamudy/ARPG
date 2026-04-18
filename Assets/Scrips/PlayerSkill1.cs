using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkill1 : MonoBehaviour
{
    [Header("Skill1 - Configuración")]
    public int manaCost = 50;
    public int damage = 100;
    public float cooldown = 1.5f;
    public float skillDelay = 0.2f;
    public float animationDuration = 0.7f;  // ✅ Duración de la animación (ajusta según tu clip)
    public float radius = 3f;

    [Header("Efectos")]
    public GameObject skillVFXPrefab;    // VFX del ataque (opcional, como slashVFX)
    public GameObject impactVFXPrefab;   // VFX de impacto por enemigo
    public Vector3 impactOffset = new Vector3(0f, 1.0f, 0f);
    public float skillVFXDistance = 2f;  // Distancia del VFX adelante del player

    [Header("Capas")]
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    [Header("Debug")]
    public bool logHits = true;

    private float nextUseTime;
    private PlayerMana mana;
    private PlayerAnimator playerAnimator;
    private PlayerClickMovement playerClickMovement;
    private Camera cam;

    void Awake()
    {
        mana = GetComponent<PlayerMana>();
        playerAnimator = GetComponent<PlayerAnimator>();
        playerClickMovement = GetComponent<PlayerClickMovement>();
        cam = Camera.main;
    }

    public void TryUseSkill1()
    {
        if (Time.time < nextUseTime) return;
        // ✅ CRITICO: Prevenir que se activate otro skill si ya hay uno activo
        if (playerClickMovement != null && playerClickMovement.isAttacking) return;

        // Gastar mana
        if (mana != null)
        {
            if (!mana.Spend(manaCost))
            {
                if (logHits) Debug.Log("❌ No hay mana para Skill1.");
                return;
            }
        }

        nextUseTime = Time.time + cooldown;

        // Rotación
        RotateToMouse();

        // Bloquear movimiento
        if (playerClickMovement != null)
            playerClickMovement.isAttacking = true;

        // Animar
        if (playerAnimator != null)
            playerAnimator.PlaySkill1();

        // Ejecutar con retraso
        StartCoroutine(DelayedSkillEffect());
    }

    System.Collections.IEnumerator DelayedSkillEffect()
    {
        yield return new WaitForSeconds(skillDelay);

        // VFX del ataque
        SpawnSkillVFX();
        
        // Daño e impact VFX
        DoDamageAndImpact();

        if (logHits) Debug.Log($"✨ Skill1 ejecutada | -{manaCost} mana | dmg={damage}");

        // ✅ Esperar a que la animación termine completamente (configurable en Inspector)
        yield return new WaitForSeconds(animationDuration - skillDelay);

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
                if (logHits) Debug.Log($"✓ Skill1: Rotado hacia mouse. Forward: {transform.forward}");
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
                    if (logHits) Debug.Log($"⚠ Skill1: Raycast falló. Usado fallback. Forward: {transform.forward}");
                }
            }
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        
        if (playerPlane.Raycast(ray, out float enter))
        {
            return ray.origin + ray.direction * enter;
        }
        
        return Vector3.zero;
    }

    void DoDamageAndImpact()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 2f, radius, enemyLayer);
        
        foreach (Collider collider in hits)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                // ✅ Usar posición del enemigo + offset, no ClosestPoint (evita que salga sobre el player)
                Vector3 impactPos = enemy.transform.position + impactOffset;
                SpawnImpact(impactPos);
                
                if (logHits) Debug.Log($"💥 Skill1 golpeó a {collider.name}: -{damage} HP");
            }
        }
    }

    void SpawnSkillVFX()
    {
        if (skillVFXPrefab == null) return;

        Vector3 spawnPos = transform.position + transform.forward * skillVFXDistance;
        Quaternion skillRot = Quaternion.LookRotation(transform.forward);
        GameObject vfx = Instantiate(skillVFXPrefab, spawnPos, skillRot);
        
        if (logHits) Debug.Log($"📍 Skill1 VFX spawneado en {spawnPos}");
    }

    void SpawnImpact(Vector3 hitPos)
    {
        if (impactVFXPrefab == null) return;
        Instantiate(impactVFXPrefab, hitPos, Quaternion.identity);
    }
}
