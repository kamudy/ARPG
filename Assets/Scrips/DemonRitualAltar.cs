using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DemonRitualAltar : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private ItemData[] requiredHeads = new ItemData[5]; // 5 cabezas requeridas
    [SerializeField] private GameObject demonPrefab;
    [SerializeField] private Vector3 spawnOffset = Vector3.up * 2f;
    [SerializeField] private float interactionRange = 3f;

    [Header("Visual")]
    [SerializeField] private Transform[] headStakes = new Transform[5]; // 5 posiciones visuales para las cabezas
    [SerializeField] private GameObject[] headDisplayPrefabs = new GameObject[5]; // Prefabs diferentes para cada cabeza (o reutilizar)
    [SerializeField] private ParticleSystem ritualVFX;

    // Estado
    private ItemData[] collectedHeads = new ItemData[5];
    private GameObject[] headVisuals = new GameObject[5];
    private GameObject activeDemon;
    private bool playerNearby = false;
    private Transform playerTransform;
    private PlayerInventory playerInventory;
    private bool isCompleted = false;

    public event Action OnHeadsCollected;
    public event Action OnDemonSpawned;
    public event Action OnAltarReset; // Nuevo evento para cuando se resetea

    void Start()
    {
        // Buscar al jugador por tag (para distancia)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerObject != null ? playerObject.transform : null;
        
        // Obtener inventario del jugador
        playerInventory = FindFirstObjectByType<PlayerInventory>();
        
        if (playerTransform == null)
            Debug.LogError("❌ DemonRitualAltar: No se encontró al jugador con tag 'Player'");
        
        if (playerInventory == null)
            Debug.LogError("❌ DemonRitualAltar: No se encontró PlayerInventory");
        
        if (demonPrefab == null)
            Debug.LogError("❌ DemonRitualAltar: demonPrefab sin asignar");
            
        Debug.Log("✅ DemonRitualAltar: Inicializado. Requiere 5 cabezas para invocar Demon");
    }

    void Update()
    {
        // Detectar si el jugador está cerca
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            playerNearby = distance <= interactionRange;
        }

        // Verificar si el Demon fue destruido (limpieza automática) - PRIMERA PRIORIDAD
        if (activeDemon != null)
        {
            try
            {
                if (!activeDemon.activeSelf)
                {
                    Debug.LogWarning("🔴 ALERT: Demon detectado como inactivo - Ejecutando ResetAltar");
                    activeDemon = null;
                    ResetAltar();
                    return; // Salir early
                }
            }
            catch
            {
                // El objeto fue completamente destruido
                Debug.LogWarning("🔴 ALERT: Demon completamente destruido - Ejecutando ResetAltar");
                activeDemon = null;
                ResetAltar();
                return;
            }
        }

        // Input: Presionar F para colocar cabeza o invocar Demon
        if (playerNearby && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (!isCompleted)
            {
                TryPlaceHead();
            }
            else if (activeDemon == null)
            {
                InvokeDemon();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualizar rango de interacción
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    /// <summary>Intenta colocar una cabeza desde el inventario del jugador</summary>
    private void TryPlaceHead()
    {
        if (playerInventory == null)
            return;

        // Buscar en el inventario una cabeza que no esté colocada
        for (int i = 0; i < playerInventory.SlotCount; i++)
        {
            ItemStack stack = playerInventory.GetStackAt(i);
            if (stack == null || stack.data == null) continue;

            if (stack.data.itemType == ItemType.Head)
            {
                // Buscar slot vacío en el altar
                for (int j = 0; j < 5; j++)
                {
                    if (collectedHeads[j] == null)
                    {
                        PlaceHead(j, stack.data);
                        
                        // Remover del inventario
                        stack.amount--;
                        if (stack.amount <= 0)
                        {
                            playerInventory.SetStackAt(i, null);
                        }
                        else
                        {
                            playerInventory.SetStackAt(i, stack);
                        }

                        return;
                    }
                }

                Debug.LogWarning("⚠️ El altar está lleno de cabezas.");
                return;
            }
        }

        Debug.LogWarning("⚠️ No tienes cabezas en el inventario para colocar.");
    }

    /// <summary>Coloca una cabeza en una posición del altar</summary>
    private void PlaceHead(int slotIndex, ItemData head)
    {
        collectedHeads[slotIndex] = head;
        
        // Obtener el prefab correspondiente para esta cabeza
        GameObject prefabToUse = GetHeadDisplayPrefab(slotIndex, head);
        
        // Crear visual de la cabeza si existe el prefab
        if (prefabToUse != null && headStakes[slotIndex] != null)
        {
            if (headVisuals[slotIndex] != null)
                Destroy(headVisuals[slotIndex]);
                
            headVisuals[slotIndex] = Instantiate(prefabToUse, headStakes[slotIndex].position, Quaternion.identity);
            headVisuals[slotIndex].transform.SetParent(headStakes[slotIndex]);
            Debug.Log($"🧙 {head.itemName} colocada en el slot {slotIndex + 1} [Prefab: {prefabToUse.name}]");
        }
        else if (headStakes[slotIndex] != null)
        {
            Debug.LogWarning($"⚠️ No hay prefab asignado para la cabeza en slot {slotIndex + 1}");
        }

        // Verificar si se completó el ritual
        CheckCompletion();
    }

    /// <summary>Verifica si todas las 5 cabezas están presentes</summary>
    private void CheckCompletion()
    {
        for (int i = 0; i < 5; i++)
        {
            if (collectedHeads[i] == null)
                return;
        }

        Debug.Log("🎉 ¡RITUAL COMPLETADO! Todas las cabezas colocadas.");
        isCompleted = true;
        OnHeadsCollected?.Invoke();

        // Efectos visuales
        if (ritualVFX != null)
            ritualVFX.Play();
    }

    /// <summary>Invoca al Demon si se completó el ritual</summary>
    private void InvokeDemon()
    {
        if (!isCompleted)
        {
            Debug.LogWarning("⚠️ El ritual no está completo.");
            return;
        }

        if (activeDemon != null && activeDemon.activeSelf)
        {
            Debug.LogWarning("⚠️ Ya hay un Demon invocado. Espera a que muera.");
            return;
        }

        // Terminar ritual (desactivar cabezas, colliders, itemPickup)
        FinishRitual();

        // Spawear Demon
        Vector3 spawnPos = transform.position + spawnOffset;
        activeDemon = Instantiate(demonPrefab, spawnPos, Quaternion.identity);
        
        // Registrar la muerte del Demon para resetear el altar
        DemonEnemy demonComponent = activeDemon.GetComponent<DemonEnemy>();
        
        // Si no está en el root, buscar en hijos
        if (demonComponent == null)
        {
            demonComponent = activeDemon.GetComponentInChildren<DemonEnemy>();
            Debug.Log("🔍 DemonEnemy encontrado en HIJO del prefab");
        }
        
        if (demonComponent != null)
        {
            demonComponent.OnDemonDeath += ResetAltar;
            Debug.Log("✅ SUSCRIPCION OK: ResetAltar está suscrito a OnDemonDeath del Demon");
        }
        else
        {
            Debug.LogError("❌ ERROR: El Demon prefab NO tiene el script DemonEnemy en root ni en hijos!");
        }

        Debug.Log("💀 ¡EL DEMON HA SIDO INVOCADO!");
        OnDemonSpawned?.Invoke();

        // Efectos visuales
        if (ritualVFX != null)
            ritualVFX.Stop();
    }

    /// <summary>Termina el ritual: desaparecen cabezas visuales del altar</summary>
    private void FinishRitual()
    {
        // Desactivar cabezas visuales en las estacas
        for (int i = 0; i < headVisuals.Length; i++)
        {
            if (headVisuals[i] != null)
            {
                Destroy(headVisuals[i]);
            }
            headVisuals[i] = null;
        }

        Debug.Log("✨ ¡Ritual terminado! Cabezas del altar consumidas.");
    }

    /// <summary>Resetea el altar cuando el Demon muere - permite colocar cabezas nuevamente</summary>
    private void ResetAltar()
    {
        Debug.Log("🔄 ⚠️ RESETALTAR EJECUTADO - Iniciando reset completo del altar");
        
        // Limpiar completamente el estado
        isCompleted = false;
        activeDemon = null;
        
        // Limpiar cabezas recolectadas
        for (int i = 0; i < collectedHeads.Length; i++)
        {
            collectedHeads[i] = null;
        }

        // Destruir visuals
        for (int i = 0; i < headVisuals.Length; i++)
        {
            if (headVisuals[i] != null)
            {
                Destroy(headVisuals[i]);
                headVisuals[i] = null;
            }
        }

        // Disparar evento de reset para notificar al UI
        OnAltarReset?.Invoke();

        Debug.Log("✅ Altar reseteado completamente. Coloca 5 cabezas nuevamente para invocar al Demon.");
    }

    /// <summary>Retorna el estado del altar (para UI)</summary>
    public int GetHeadsCollected() => Array.FindAll(collectedHeads, h => h != null).Length;

    public bool IsRitualComplete() => isCompleted;

    public ItemData GetHeadAt(int index) => index >= 0 && index < 5 ? collectedHeads[index] : null;

    public float GetInteractionRange() => interactionRange;

    /// <summary>Obtiene el prefab visual correcto para una cabeza específica (por nombre o posición)</summary>
    /// <remarks>
    /// Busca primero por nombre de la cabeza, luego por índice de slot.
    /// Esto permite customizar prefabs por tipo de cabeza.
    /// </remarks>
    private GameObject GetHeadDisplayPrefab(int slotIndex, ItemData head)
    {
        if (headDisplayPrefabs == null || headDisplayPrefabs.Length == 0)
            return null;

        // Si solo hay un prefab en el array, usarlo para todas las cabezas
        if (headDisplayPrefabs.Length == 1)
            return headDisplayPrefabs[0];

        // Si hay un prefab para este slot, usarlo
        if (slotIndex >= 0 && slotIndex < headDisplayPrefabs.Length)
            return headDisplayPrefabs[slotIndex];

        // Fallback: usar el primer prefab
        return headDisplayPrefabs[0];
    }
}
