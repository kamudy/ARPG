using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 12f;
    public float lifetime = 8f;

    [Header("Hit Detection")]
    public float hitRadius = 0.2f;

    [Header("Impact")]
    public GameObject impactVFXPrefab;

    private Vector3 direction = Vector3.zero;
    private int damage = 0;
    private PlayerDerivedStats playerDerivedStats;
    private Transform shooterRoot;
    private bool hasHit = false;
    private bool isMoving = false;

    void Start()
    {
        if (GetComponentInChildren<Renderer>(true) == null)
            Debug.LogWarning("[Arrow] El prefab no tiene Renderer visible. Se movera y pegara, pero no se vera en escena.");

        // Auto-destruirse despu�s del lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Movimiento simple cada frame
        if (isMoving && !hasHit && direction != Vector3.zero)
        {
            Vector3 previousPosition = transform.position;

            // Mover hacia adelante
            transform.position += direction * speed * Time.deltaTime;

            // Rotar para que apunte en la direcci�n del movimiento
            transform.rotation = Quaternion.LookRotation(direction);

            // Fallback robusto: detecta impactos aunque OnTrigger no dispare por settings de f�sica.
            SweepForHits(previousPosition, transform.position);
        }
    }

    void SweepForHits(Vector3 from, Vector3 to)
    {
        if (hasHit) return;

        Vector3 delta = to - from;
        float distance = delta.magnitude;
        if (distance <= 0.0001f) return;

        Ray ray = new Ray(from, delta.normalized);
        RaycastHit[] hits = Physics.SphereCastAll(ray, hitRadius, distance, ~0, QueryTriggerInteraction.Collide);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i].collider;
            if (col == null) continue;

            // Ignorar colisiones con la propia flecha y con el tirador.
            if (col.GetComponentInParent<Arrow>() == this) continue;
            if (shooterRoot != null && col.transform.IsChildOf(shooterRoot)) continue;

            PlayerHealth playerHealth = col.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                DamagePlayer(playerHealth, hits[i].point);
                return;
            }

            Enemy enemyHit = col.GetComponentInParent<Enemy>();
            if (enemyHit != null)
                continue;

            HitSomething(hits[i].point);
            return;
        }
    }

    public void Launch(Vector3 dir, int dmg, PlayerDerivedStats playerDerived, Transform shooter)
    {
        direction = dir.normalized;
        damage = dmg;
        playerDerivedStats = playerDerived;
        shooterRoot = shooter;
        isMoving = true;

        // Rotar la flecha desde el inicio
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Forzar modo trigger y sin f�sica para evitar empujar al player.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider[] arrowColliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < arrowColliders.Length; i++)
        {
            arrowColliders[i].isTrigger = true;
        }

        // Ignorar colisiones con el enemigo que dispara para evitar autodestrucción al nacer.
        if (shooterRoot != null)
        {
            Collider[] shooterColliders = shooterRoot.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < arrowColliders.Length; i++)
            {
                for (int j = 0; j < shooterColliders.Length; j++)
                {
                    Physics.IgnoreCollision(arrowColliders[i], shooterColliders[j], true);
                }
            }
        }

        Debug.Log($"[Arrow] �LANZADA! Direcci�n: {direction}, Da�o: {damage}, Velocidad: {speed}");
    }

    void OnTriggerEnter(Collider col)
    {
        if (hasHit) return;

        Enemy enemyHit = col.GetComponentInParent<Enemy>();
        if (enemyHit != null)
            return;

        PlayerHealth playerHealth = col.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null && isMoving)
        {
            DamagePlayer(playerHealth, col.ClosestPoint(transform.position));
            return;
        }

        // Si golpea algo que no es enemigo, destruirse.
        if (!col.CompareTag("Enemy"))
        {
            HitSomething(col.ClosestPoint(transform.position));
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        Enemy enemyHit = collision.collider.GetComponentInParent<Enemy>();
        if (enemyHit != null)
            return;

        // Fallback: si por configuraci�n del prefab entra por colisi�n, buscar PlayerHealth en la jerarqu�a.
        PlayerHealth playerHealth = collision.collider.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null && isMoving)
        {
            Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
            DamagePlayer(playerHealth, hitPoint);
            return;
        }

        // Si golpea algo que no es el jugador, destruirse.
        if (!collision.gameObject.CompareTag("Enemy"))
        {
            Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
            HitSomething(hitPoint);
        }
    }

    void DamagePlayer(PlayerHealth playerHealth, Vector3 hitPoint)
    {
        if (hasHit) return;

        hasHit = true;
        isMoving = false;

        int defense = (playerDerivedStats != null) ? playerDerivedStats.Defense : 0;
        int finalDamage = Mathf.Max(1, damage - defense);

        playerHealth.TakeDamage(finalDamage);

        Debug.Log($"[Arrow] �IMPACTO JUGADOR! Da�o: {finalDamage} en posici�n {hitPoint}");

        CreateImpact(hitPoint);
        Destroy(gameObject, 0.02f);
    }

    void HitSomething(Vector3 hitPoint)
    {
        hasHit = true;
        isMoving = false;

        Debug.Log($"[Arrow] �IMPACTO AMBIENTE! Posici�n: {hitPoint}");

        CreateImpact(hitPoint);
        Destroy(gameObject, 0.02f);
    }

    void CreateImpact(Vector3 hitPoint)
    {
        if (impactVFXPrefab != null)
        {
            Instantiate(impactVFXPrefab, hitPoint, Quaternion.identity);
        }
    }

    // Debug: mostrar la direcci�n de movimiento en el editor
    void OnDrawGizmosSelected()
    {
        if (isMoving && direction != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + direction * 2f);
        }
    }
}
