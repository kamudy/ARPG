using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerSkill3 : MonoBehaviour
{
    [Header("Skill3 - Configuración")]
    public int manaCost = 50;
    public int damage = 100;
    public float cooldown = 1.5f;
    public float skillDelay = 0.2f;        // Timing del primer golpe/daño
    public float impactDelay = 0.25f;      // ✅ Timing del primer impact VFX
    public float skillDelay2 = 0.5f;       // Timing del segundo golpe/daño
    public float impactDelay2 = 0.55f;     // ✅ Timing del segundo impact VFX
    public float animationDuration = 0.8f; // ✅ Duración de la animación (ambos golpes)
    public float radius = 3f;

    [Header("Efectos")]
    public GameObject skillVFXPrefab;    // VFX del primer golpe
    public GameObject impactVFXPrefab;   // VFX de impacto del primer golpe
    public GameObject skillVFXPrefab2;   // ✅ VFX del segundo golpe
    public GameObject impactVFXPrefab2;  // ✅ VFX de impacto del segundo golpe
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

    public void TryUseSkill3()
    {
        if (Time.time < nextUseTime) return;
        // ✅ CRITICO: Prevenir que se activate otro skill si ya hay uno activo
        if (playerClickMovement != null && playerClickMovement.isAttacking) return;

        // Gastar mana
        if (mana != null)
        {
            if (!mana.Spend(manaCost))
            {
                if (logHits) Debug.Log("❌ No hay mana para Skill3.");
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
            playerAnimator.PlaySkill3();

        // Ejecutar con retraso
        StartCoroutine(DelayedSkillEffect());
    }

    System.Collections.IEnumerator DelayedSkillEffect()
    {
        // Rastrear enemigos golpeados para evitar hits duplicados
        HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

        // ===== PRIMER GOLPE =====
        yield return new WaitForSeconds(skillDelay);
        SpawnSkillVFX();
        DoDamage(hitEnemies);  // Solo hace daño
        if (logHits) Debug.Log($"⚔️  Skill3 - Hit 1/2 (Daño)");

        // ===== PRIMER IMPACT VFX (timing controlado) =====
        yield return new WaitForSeconds(impactDelay - skillDelay);
        SpawnImpactVFX(hitEnemies);  // Impact VFX del primer hit
        if (logHits) Debug.Log($"💥 Skill3 - Impact 1/2");

        // ===== SEGUNDO GOLPE =====
        yield return new WaitForSeconds(skillDelay2 - impactDelay);
        SpawnSkillVFX2();  // VFX del segundo golpe
        DoDamage2(hitEnemies);  // Solo hace daño
        if (logHits) Debug.Log($"⚔️  Skill3 - Hit 2/2 (Daño)");

        // ===== SEGUNDO IMPACT VFX (timing controlado) =====
        yield return new WaitForSeconds(impactDelay2 - skillDelay2);
        SpawnImpactVFX2(hitEnemies);  // Impact VFX del segundo hit
        if (logHits) Debug.Log($"💥 Skill3 - Impact 2/2");

        if (logHits) Debug.Log($"✨ Skill3 completada | -{manaCost} mana | dmg={damage} x2 hits");

        // ✅ Esperar a que la animación termine completamente (configurable en Inspector)
        yield return new WaitForSeconds(animationDuration - impactDelay2);

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
                if (logHits) Debug.Log($"✓ Skill3: Rotado hacia mouse. Forward: {transform.forward}");
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
                    if (logHits) Debug.Log($"⚠ Skill3: Raycast falló. Usado fallback. Forward: {transform.forward}");
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

    void DoDamageAndImpact(HashSet<Enemy> alreadyHit)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 2f, radius, enemyLayer);
        
        foreach (Collider collider in hits)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && !alreadyHit.Contains(enemy))
            {
                enemy.TakeDamage(damage);
                alreadyHit.Add(enemy);  // ✅ Marcar como golpeado
                
                if (logHits) Debug.Log($"💥 Skill3 golpeó a {collider.name}: -{damage} HP");
            }
        }
    }

    // ✅ NUEVO: Solo hace daño (sin VFX)
    void DoDamage(HashSet<Enemy> alreadyHit)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 2f, radius, enemyLayer);
        
        foreach (Collider collider in hits)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && !alreadyHit.Contains(enemy))
            {
                enemy.TakeDamage(damage);
                alreadyHit.Add(enemy);
                
                if (logHits) Debug.Log($"⚔️ Skill3 golpeó a {collider.name}: -{damage} HP");
            }
        }
    }

    // ✅ NUEVO: Solo spawna los impact VFX (sin daño)
    void SpawnImpactVFX(HashSet<Enemy> hitEnemies)
    {
        foreach (Enemy enemy in hitEnemies)
        {
            if (enemy != null)
            {
                Vector3 hitPos = enemy.transform.position + impactOffset;
                Instantiate(impactVFXPrefab, hitPos, Quaternion.identity);
            }
        }
    }

    void SpawnSkillVFX()
    {
        if (skillVFXPrefab == null) return;

        Vector3 spawnPos = transform.position + transform.forward * skillVFXDistance;
        Quaternion skillRot = Quaternion.LookRotation(transform.forward);
        GameObject vfx = Instantiate(skillVFXPrefab, spawnPos, skillRot);
        
        if (logHits) Debug.Log($"📍 Skill3 VFX spawneado en {spawnPos}");
    }

    void SpawnImpact(Vector3 hitPos)
    {
        if (impactVFXPrefab == null) return;
        Instantiate(impactVFXPrefab, hitPos, Quaternion.identity);
    }

    // ============ SEGUNDO GOLPE - VFX Y DAÑO ============
    
    void DoDamageAndImpact2(HashSet<Enemy> alreadyHit)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 2f, radius, enemyLayer);
        
        foreach (Collider collider in hits)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && !alreadyHit.Contains(enemy))
            {
                enemy.TakeDamage(damage);
                alreadyHit.Add(enemy);
                
                if (logHits) Debug.Log($"⚔️ Skill3 Hit2 golpeó a {collider.name}: -{damage} HP");
            }
        }
    }

    // ✅ NUEVO: Segundo hit - Solo hace daño (sin VFX)
    void DoDamage2(HashSet<Enemy> alreadyHit)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 2f, radius, enemyLayer);
        
        foreach (Collider collider in hits)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && !alreadyHit.Contains(enemy))
            {
                enemy.TakeDamage(damage);
                alreadyHit.Add(enemy);
                
                if (logHits) Debug.Log($"⚔️ Skill3 Hit2 golpeó a {collider.name}: -{damage} HP");
            }
        }
    }

    // ✅ NUEVO: Segundo hit - Solo spawna los impact VFX (sin daño)
    void SpawnImpactVFX2(HashSet<Enemy> hitEnemies)
    {
        foreach (Enemy enemy in hitEnemies)
        {
            if (enemy != null)
            {
                Vector3 hitPos = enemy.transform.position + impactOffset;
                Instantiate(impactVFXPrefab2, hitPos, Quaternion.identity);
            }
        }
    }

    void SpawnSkillVFX2()
    {
        if (skillVFXPrefab2 == null) return;

        Vector3 spawnPos = transform.position + transform.forward * skillVFXDistance;
        Quaternion skillRot = Quaternion.LookRotation(transform.forward);
        GameObject vfx = Instantiate(skillVFXPrefab2, spawnPos, skillRot);
        
        if (logHits) Debug.Log($"📍 Skill3 VFX2 spawneado en {spawnPos}");
    }

    void SpawnImpact2(Vector3 hitPos)
    {
        if (impactVFXPrefab2 == null) return;
        Instantiate(impactVFXPrefab2, hitPos, Quaternion.identity);
    }
}
