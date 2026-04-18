using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private string savePath;
    private const string SAVE_FILE = "gamesave.json";
    
    private Vector3 spawnPosition; // Posición inicial de spawn

    [Header("Item Database")]
    public List<ItemData> itemDatabase = new List<ItemData>(); // Lista de items para cargar en Inspector

    [Header("Debug")]
    public bool debugSaveMessages = true; // Muestra logs cuando se guarda
    
    // Sistema de guardado con delay para evitar lag
    private float saveDelay = 0.5f; // Esperar 0.5 segundos después del último cambio
    private float saveTimer = 0f;
    private bool pendingSave = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        savePath = Path.Combine(Application.dataPath, SAVE_FILE);
    }

    void Start()
    {
        // Guardar la posición inicial del player como spawn
        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory != null)
        {
            spawnPosition = inventory.transform.position;
        }

        // Limpiar archivo de guardado corrupto si existe
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                if (json.Contains("itemName\":\"\"") || json.Contains("itemName\": null"))
                {
                    Debug.Log("🧹 Detectado gamesave corrupto, eliminando...");
                    File.Delete(savePath);
                }
            }
            catch { }
        }

        // Cargar datos con delay para permitir que la UI se inicialice
        Invoke(nameof(LoadGame), 0.5f);
    }

    void Update()
    {
        // Sistema de guardado con delay
        if (pendingSave)
        {
            saveTimer -= Time.deltaTime;
            if (saveTimer <= 0)
            {
                ExecuteSave();
                pendingSave = false;
            }
        }
    }

    public void SaveGame()
    {
        // Marcar que hay cambios pendientes de guardar
        pendingSave = true;
        saveTimer = saveDelay; // Reiniciar el contador
    }

    private void ExecuteSave()
    {
        GameData data = new GameData();

        // Recopilar datos del player
        PlayerLevel playerLevel = FindFirstObjectByType<PlayerLevel>();
        if (playerLevel != null)
        {
            data.playerLevel = playerLevel.level;
            data.playerXP = playerLevel.currentXP;
            if (debugSaveMessages)
                Debug.Log($"💾 Guardando: Nivel {data.playerLevel}, XP {data.playerXP}");
        }

        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            data.playerHealth = playerHealth.currentHealth;
            data.playerMaxHealth = playerHealth.maxHealth;
        }

        PlayerMana playerMana = FindFirstObjectByType<PlayerMana>();
        if (playerMana != null)
        {
            data.playerMana = playerMana.currentMana;
            data.playerMaxMana = playerMana.maxMana;
        }

        // Guardar estadísticas del personaje
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            data.strength = playerStats.strength;
            data.agility = playerStats.agility;
            data.vitality = playerStats.vitality;
            data.energy = playerStats.energy;
            data.statPoints = playerStats.points;
            if (debugSaveMessages)
                Debug.Log($"💾 Stats: STR {data.strength}, AGI {data.agility}, VIT {data.vitality}, ENE {data.energy}, Puntos {data.statPoints}");
        }

        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory != null)
        {
            data.coins = inventory.coins;
            // NO guardar posición - siempre vuelve al spawn inicial
            data.playerPosition = new Vector3SerializableData(spawnPosition);

            if (debugSaveMessages)
                Debug.Log($"💾 Inventario: {inventory.SlotCount} slots, {inventory.coins} monedas");

            // Guardar items del inventario
            data.inventoryItems.Clear();
            var allItems = inventory.GetAllItems();
            foreach (var stack in allItems)
            {
                if (stack != null && stack.data != null)
                {
                    data.inventoryItems.Add(new ItemStackData
                    {
                        itemName = stack.data.itemName,
                        amount = stack.amount
                    });
                }
            }

            // Guardar items equipados
            data.equippedHead = inventory.head != null ? inventory.head.itemName : "";
            data.equippedChest = inventory.chest != null ? inventory.chest.itemName : "";
            data.equippedGloves = inventory.gloves != null ? inventory.gloves.itemName : "";
            data.equippedBoots = inventory.boots != null ? inventory.boots.itemName : "";
            data.equippedWeaponLeft = inventory.weaponLeft != null ? inventory.weaponLeft.itemName : "";
            data.equippedWeaponRight = inventory.weaponRight != null ? inventory.weaponRight.itemName : "";

            if (debugSaveMessages)
                Debug.Log($"💾 Items equipados guardados");
        }
        else
        {
            Debug.LogWarning("PlayerInventory NO ENCONTRADO!");
        }

        // Serializar a JSON
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        if (debugSaveMessages)
            Debug.Log($"💾 Progreso guardado.");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No hay guardado previo. Iniciando nuevo juego.");
            // Inicializar stats base si no hay guardado
            InitializeDefaultStats();
            return;
        }

        string json = File.ReadAllText(savePath);
        GameData data = JsonUtility.FromJson<GameData>(json);

        // Cargar datos del player
        PlayerLevel playerLevel = FindFirstObjectByType<PlayerLevel>();
        if (playerLevel != null)
        {
            playerLevel.level = data.playerLevel;
            playerLevel.currentXP = data.playerXP;
        }

        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.currentHealth = data.playerHealth;
            playerHealth.maxHealth = data.playerMaxHealth;
            playerHealth.Notify();
        }

        PlayerMana playerMana = FindFirstObjectByType<PlayerMana>();
        if (playerMana != null)
        {
            playerMana.currentMana = data.playerMana;
            playerMana.maxMana = data.playerMaxMana;
        }

        // Cargar estadísticas del personaje
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.strength = data.strength;
            playerStats.agility = data.agility;
            playerStats.vitality = data.vitality;
            playerStats.energy = data.energy;
            playerStats.points = data.statPoints;
            if (debugSaveMessages)
                Debug.Log($"📂 Stats cargadas: STR {data.strength}, AGI {data.agility}, VIT {data.vitality}, ENE {data.energy}");
        }

        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory != null)
        {
            inventory.coins = data.coins;

            // Restaurar posición del player
            CharacterController controller = inventory.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                inventory.transform.position = data.playerPosition.ToVector3();
                controller.enabled = true;
            }
            else
            {
                inventory.transform.position = data.playerPosition.ToVector3();
            }

            // ✅ CRITICO: Limpiar TODOS los slots antes de cargar
            // Esto previene que queden ItemStacks con data null de guardados anteriores
            for (int i = 0; i < inventory.SlotCount; i++)
            {
                inventory.SetStackAt(i, null);
            }

            // Restaurar items del inventario
            if (debugSaveMessages)
                Debug.Log($"📂 Cargando inventario: {data.inventoryItems.Count} items");
            
            for (int i = 0; i < inventory.SlotCount && i < data.inventoryItems.Count; i++)
            {
                var itemData = data.inventoryItems[i];
                
                // Ignorar items vacíos o sin nombre
                if (itemData == null || string.IsNullOrEmpty(itemData.itemName))
                {
                    continue;
                }
                
                // Buscar el ItemData por nombre
                ItemData item = FindItemByName(itemData.itemName);
                if (item != null)
                {
                    inventory.SetStackAt(i, new ItemStack(item, itemData.amount));
                }
                else
                {
                    if (debugSaveMessages)
                        Debug.LogWarning($"⚠️ Item no encontrado en ItemDatabase: {itemData.itemName} (ignorado)");
                    // ✅ No agregar stacks con item null - el slot permanece vacío
                }
            }

            // Restaurar items equipados
            if (debugSaveMessages)
                Debug.Log($"📂 Cargando items equipados");

            if (!string.IsNullOrEmpty(data.equippedHead))
                inventory.head = FindItemByName(data.equippedHead);
            if (!string.IsNullOrEmpty(data.equippedChest))
                inventory.chest = FindItemByName(data.equippedChest);
            if (!string.IsNullOrEmpty(data.equippedGloves))
                inventory.gloves = FindItemByName(data.equippedGloves);
            if (!string.IsNullOrEmpty(data.equippedBoots))
                inventory.boots = FindItemByName(data.equippedBoots);
            if (!string.IsNullOrEmpty(data.equippedWeaponLeft))
                inventory.weaponLeft = FindItemByName(data.equippedWeaponLeft);
            if (!string.IsNullOrEmpty(data.equippedWeaponRight))
                inventory.weaponRight = FindItemByName(data.equippedWeaponRight);

            // Notificar al inventario para refrescar la UI
            inventory.NotifyInventoryChanged();
        }

        // Refrescar la UI del inventario
        InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }

        // Recalcular stats derivados después de cargar
        PlayerDerivedStats derivedStats = FindFirstObjectByType<PlayerDerivedStats>();
        if (derivedStats != null)
        {
            derivedStats.Recalculate();
        }

        if (debugSaveMessages)
            Debug.Log($"📂 Juego cargado.");
    }

    private void InitializeDefaultStats()
    {
        // Inicializar stats base cuando es nuevo juego
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            // Los valores por defecto ya están en el Inspector (10, 10, 10, 10)
            // Solo asegurarse de que no estén vacíos
            if (playerStats.strength == 0) playerStats.strength = 10;
            if (playerStats.agility == 0) playerStats.agility = 10;
            if (playerStats.vitality == 0) playerStats.vitality = 10;
            if (playerStats.energy == 0) playerStats.energy = 10;
            Debug.Log($"Stats inicializados por defecto: STR 10, AGI 10, VIT 10, ENE 10");
        }
    }

    private ItemData FindItemByName(string itemName)
    {
        // Buscar en la lista de itemDatabase
        foreach (var item in itemDatabase)
        {
            if (item != null && item.itemName == itemName)
                return item;
        }

        Debug.LogWarning($"Item no encontrado en ItemDatabase: {itemName}");
        return null;
    }

    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Guardado eliminado.");
        }
    }

    public void ResetPlayerCharacter()
    {
        // Resetear nivel y XP
        PlayerLevel playerLevel = FindFirstObjectByType<PlayerLevel>();
        if (playerLevel != null)
        {
            playerLevel.level = 1;
            playerLevel.currentXP = 0;
            Debug.Log("✨ Nivel resetado a 1");
        }

        // Resetear salud y maná a máximo
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.currentHealth = playerHealth.maxHealth;
            playerHealth.Notify();
        }

        PlayerMana playerMana = FindFirstObjectByType<PlayerMana>();
        if (playerMana != null)
        {
            playerMana.currentMana = playerMana.maxMana;
        }

        // Resetear stats base
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.strength = 10;
            playerStats.agility = 10;
            playerStats.vitality = 10;
            playerStats.energy = 10;
            playerStats.points = 0;
            Debug.Log("⚔️ Stats resetados a valores base (10, 10, 10, 10)");
        }

        // Limpiar inventario
        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        if (inventory != null)
        {
            // Vaciar todos los slots
            for (int i = 0; i < inventory.SlotCount; i++)
            {
                inventory.SetStackAt(i, null);
            }
            inventory.coins = 0;
            Debug.Log("🎒 Inventario vacío - sin items ni monedas");
        }

        // Guardar el progreso resetado
        SaveGame();
        Debug.Log("🔄 ¡Personaje resetado completamente a nivel 1 con stats base!");
    }
}
