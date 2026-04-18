using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 6, -6);
    public Vector3 lookTargetOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Orbit")]
    public float orbitSensitivity = 0.10f;
    public float minPitch = -75f;
    public float maxPitch = 85f;
    public float defaultPitch = 35f;
    public bool alignBehindTargetOnStart = true;

    [Header("Follow Smoothing")]
    public float verticalFollowSpeed = 2f;
    public float startupSnapDuration = 0.25f;

    [Header("Zoom")]
    public float zoomSpeed = 2f;
    public float minFOV = 25f;
    public float maxFOV = 70f;
    public float defaultFOV = 55f;

    [Header("Distance")]
    public float defaultDistance = 8f;
    public bool forceDefaultDistanceOnStart = true;

    [Header("Fixed Camera")]
    public bool fixedCameraMode = true;
    public bool lockFocusY = true;
    public bool applyCollisionInFixedMode = false;
    public float fixedMinPitch = -40f;
    public float fixedMaxPitch = 65f;
    public float extraGroundClearanceInFixedMode = 0.35f;

    [Header("Collision")]
    public LayerMask cameraCollisionMask = ~0;
    public float collisionRadius = 0.25f;
    public float collisionBuffer = 0.15f;
    public float minCollisionDistance = 1.5f;
    public float groundClearance = 0.25f;
    private Camera mainCamera;
    private float targetFOV;

    private InventoryUI inventoryUI;
    private float orbitYaw;
    private float orbitPitch;
    private float orbitDistance;
    private Vector2 lastMousePosition;
    private Vector3 smoothedFocusPoint;
    private float fixedFocusY;
    private bool hasSmoothedFocus;
    private float startupSnapEndTime;

    void Start()
    {
        mainCamera = Camera.main;
        inventoryUI = FindFirstObjectByType<InventoryUI>();
        EnsureTarget();

        Vector3 initialDir;
        if (target != null)
        {
            initialDir = transform.position - target.position;
        }
        else
        {
            initialDir = offset;
        }

        if (initialDir.sqrMagnitude < 0.0001f)
        {
            initialDir = new Vector3(0f, 10f, -10f);
        }

        orbitDistance = initialDir.magnitude;
        if (forceDefaultDistanceOnStart)
        {
            orbitDistance = defaultDistance;
        }

        if (target != null)
        {
            // Siempre iniciar detrás del player para evitar arranque cenital.
            orbitYaw = target.eulerAngles.y;
            orbitPitch = Mathf.Clamp(defaultPitch, GetCurrentMinPitch(), GetCurrentMaxPitch());
            fixedFocusY = target.position.y + lookTargetOffset.y;
        }
        else
        {
            orbitYaw = Mathf.Atan2(initialDir.x, initialDir.z) * Mathf.Rad2Deg;
            orbitPitch = Mathf.Asin(Mathf.Clamp(initialDir.y / Mathf.Max(orbitDistance, 0.0001f), -1f, 1f)) * Mathf.Rad2Deg;
            orbitPitch = Mathf.Clamp(orbitPitch, GetCurrentMinPitch(), GetCurrentMaxPitch());
        }

        if (Mouse.current != null)
        {
            lastMousePosition = Mouse.current.position.ReadValue();
        }
        
        if (mainCamera != null)
        {
            targetFOV = Mathf.Clamp(defaultFOV, minFOV, maxFOV);
            mainCamera.fieldOfView = targetFOV;
            Debug.Log($"Cámara encontrada. FOV inicial: {mainCamera.fieldOfView}");
        }
        else
        {
            Debug.LogError("No se encontró Camera.main en la escena");
        }

        // Inicializar posición/rotación de forma determinista para evitar rebote al iniciar Play.
        if (target != null)
        {
            Quaternion orbitRotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
            Vector3 orbitOffset = orbitRotation * new Vector3(0f, 0f, -orbitDistance);
            Vector3 focusPoint = GetFocusPoint();

            smoothedFocusPoint = focusPoint;
            hasSmoothedFocus = true;
            Vector3 desiredPos = focusPoint + orbitOffset;
            desiredPos = ApplyCollisionIfNeeded(focusPoint, desiredPos);
            transform.position = desiredPos;
            transform.LookAt(focusPoint);
            startupSnapEndTime = Time.time + startupSnapDuration;
        }
    }

    void LateUpdate()
    {
        EnsureTarget();
        if (!target) return;

        HandleOrbitInput();

        Quaternion orbitRotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
        Vector3 orbitOffset = orbitRotation * new Vector3(0f, 0f, -orbitDistance);

        Vector3 focusPoint = GetFocusPoint();

        if (fixedCameraMode)
        {
            smoothedFocusPoint = focusPoint;
            hasSmoothedFocus = true;

            Vector3 fixedDesiredPos = smoothedFocusPoint + orbitOffset;
            fixedDesiredPos = ApplyCollisionIfNeeded(smoothedFocusPoint, fixedDesiredPos);
            fixedDesiredPos = ClampAboveGround(fixedDesiredPos);
            transform.position = fixedDesiredPos;
            transform.LookAt(smoothedFocusPoint);
            HandleZoom();
            return;
        }

        if (!hasSmoothedFocus)
        {
            smoothedFocusPoint = focusPoint;
            hasSmoothedFocus = true;

            // Primer frame: snap directo para no interpolar desde una posición vieja/chueca.
            transform.position = smoothedFocusPoint + orbitOffset;
            transform.LookAt(smoothedFocusPoint);
            HandleZoom();
            return;
        }

        // En los primeros instantes, hacer snap para evitar micro-rebotes por correcciones iniciales del player.
        if (Time.time < startupSnapEndTime)
        {
            smoothedFocusPoint = focusPoint;
            Vector3 earlyDesiredPos = ApplyCollisionIfNeeded(smoothedFocusPoint, smoothedFocusPoint + orbitOffset);
            earlyDesiredPos = ClampAboveGround(earlyDesiredPos);
            transform.position = earlyDesiredPos;
            transform.LookAt(smoothedFocusPoint);
            HandleZoom();
            return;
        }

        // Suavizado independiente: Y más lenta para evitar rebote en desniveles.
        float horizontalLerp = smoothSpeed * Time.deltaTime;
        float verticalLerp = verticalFollowSpeed * Time.deltaTime;
        smoothedFocusPoint.x = Mathf.Lerp(smoothedFocusPoint.x, focusPoint.x, horizontalLerp);
        smoothedFocusPoint.z = Mathf.Lerp(smoothedFocusPoint.z, focusPoint.z, horizontalLerp);
        smoothedFocusPoint.y = Mathf.Lerp(smoothedFocusPoint.y, focusPoint.y, verticalLerp);

        Vector3 desiredPos = smoothedFocusPoint + orbitOffset;
        desiredPos = ApplyCollisionIfNeeded(smoothedFocusPoint, desiredPos);
        desiredPos = ClampAboveGround(desiredPos);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(smoothedFocusPoint);

        // Aplicar zoom
        HandleZoom();
    }

    Vector3 GetFocusPoint()
    {
        Vector3 focusPoint = target.position + lookTargetOffset;

        if (fixedCameraMode && lockFocusY)
        {
            focusPoint.y = fixedFocusY;
        }

        return focusPoint;
    }

    Vector3 ApplyCollisionIfNeeded(Vector3 focusPoint, Vector3 desiredPos)
    {
        if (!fixedCameraMode || applyCollisionInFixedMode)
        {
            return ResolveCameraCollision(focusPoint, desiredPos);
        }

        return desiredPos;
    }

    Vector3 ClampAboveGround(Vector3 position)
    {
        if (Physics.Raycast(
                position + Vector3.up * 2f,
                Vector3.down,
                out RaycastHit groundHit,
                50f,
                cameraCollisionMask,
                QueryTriggerInteraction.Ignore))
        {
            float minY = groundHit.point.y + groundClearance;
            if (fixedCameraMode)
            {
                minY += extraGroundClearanceInFixedMode;
            }

            if (position.y < minY)
            {
                position.y = minY;
            }
        }

        return position;
    }

    Vector3 ResolveCameraCollision(Vector3 focusPoint, Vector3 desiredPos)
    {
        Vector3 direction = desiredPos - focusPoint;
        float desiredDistance = direction.magnitude;

        if (desiredDistance <= 0.001f)
            return desiredPos;

        Vector3 dirNormalized = direction / desiredDistance;
        float correctedDistance = desiredDistance;

        RaycastHit[] hits = Physics.SphereCastAll(
            focusPoint,
            collisionRadius,
            dirNormalized,
            desiredDistance,
            cameraCollisionMask,
            QueryTriggerInteraction.Ignore);

        if (hits != null && hits.Length > 0)
        {
            float nearestValidDistance = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (ShouldIgnoreCollisionHit(hit))
                    continue;

                if (hit.distance < nearestValidDistance)
                    nearestValidDistance = hit.distance;
            }

            if (nearestValidDistance < float.MaxValue)
                correctedDistance = Mathf.Max(minCollisionDistance, nearestValidDistance - collisionBuffer);
        }

        Vector3 correctedPos = focusPoint + dirNormalized * correctedDistance;

        // Seguridad extra para evitar que la cámara quede bajo el terreno.
        if (Physics.Raycast(
                correctedPos + Vector3.up * 2f,
                Vector3.down,
                out RaycastHit groundHit,
                20f,
                cameraCollisionMask,
                QueryTriggerInteraction.Ignore))
        {
            float minY = groundHit.point.y + groundClearance;
            if (correctedPos.y < minY)
            {
                correctedPos.y = minY;
            }
        }

        return correctedPos;
    }

    bool ShouldIgnoreCollisionHit(RaycastHit hit)
    {
        if (hit.collider == null)
            return true;

        Transform hitTransform = hit.collider.transform;

        // Ignorar cualquier collider del target o de sus hijos.
        if (target != null && (hitTransform == target || hitTransform.IsChildOf(target) || target.IsChildOf(hitTransform)))
            return true;

        // Ignorar el escudo de dash para que no acerque la cámara.
        if (hitTransform.GetComponentInParent<DashShieldVFX>() != null)
            return true;

        return false;
    }

    void EnsureTarget()
    {
        if (target != null) return;

        PlayerClickMovement playerMovement = FindFirstObjectByType<PlayerClickMovement>();
        if (playerMovement != null)
        {
            target = playerMovement.transform;
            fixedFocusY = target.position.y + lookTargetOffset.y;
            return;
        }

        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            target = playerHealth.transform;
            fixedFocusY = target.position.y + lookTargetOffset.y;
        }
    }

    void HandleOrbitInput()
    {
        if (Mouse.current == null) return;

        if (inventoryUI != null && inventoryUI.IsOpen)
        {
            return;
        }

        bool rightPressed = Mouse.current.rightButton.isPressed;
        Vector2 currentMousePosition = Mouse.current.position.ReadValue();

        if (!rightPressed)
        {
            lastMousePosition = currentMousePosition;
            return;
        }

        Vector2 delta = currentMousePosition - lastMousePosition;
        lastMousePosition = currentMousePosition;

        orbitYaw += delta.x * orbitSensitivity;
        orbitPitch -= delta.y * orbitSensitivity;
        orbitPitch = Mathf.Clamp(orbitPitch, GetCurrentMinPitch(), GetCurrentMaxPitch());
    }

    float GetCurrentMinPitch()
    {
        if (fixedCameraMode)
        {
            return Mathf.Max(minPitch, fixedMinPitch);
        }

        return minPitch;
    }

    float GetCurrentMaxPitch()
    {
        if (fixedCameraMode)
        {
            return Mathf.Min(maxPitch, fixedMaxPitch);
        }

        return maxPitch;
    }

    void HandleZoom()
    {
        if (mainCamera == null) return;

        // No hacer zoom si el inventario está abierto
        if (inventoryUI != null && inventoryUI.IsOpen)
        {
            return;
        }

        try
        {
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (scroll != 0)
                {
                    // Scroll arriba = valor positivo = acercar (FOV menor)
                    targetFOV -= scroll * zoomSpeed;
                    targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
                }
            }
        }
        catch
        {
            // Si hay error con Mouse.current, ignorar
        }

        // Aplicar el FOV suavemente
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * 3f);
    }
}