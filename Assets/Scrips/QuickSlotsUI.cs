using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class QuickSlotsUI : MonoBehaviour
{
    [Header("Quick Slots")]
    public Image[] slotImages = new Image[3];
    public TextMeshProUGUI[] amountTexts = new TextMeshProUGUI[3];
    
    [Header("Use Buttons")]
    public Button[] useButtons = new Button[3];

    [Header("Hotkeys")]
    public KeyCode[] hotkeys = new KeyCode[3] { KeyCode.Q, KeyCode.W, KeyCode.E };

    private PlayerInventory inventory;
    private InventoryUI inventoryUI;
    private ItemData[] quickSlotsPotions = new ItemData[3];  // Solo almacenar referencias a ItemData

    void Awake()
    {
        inventory = FindFirstObjectByType<PlayerInventory>();
        inventoryUI = FindFirstObjectByType<InventoryUI>();

        // Asignar listeners a los botones
        for (int i = 0; i < useButtons.Length; i++)
        {
            int index = i; // Closure fix
            if (useButtons[i] != null)
                useButtons[i].onClick.AddListener(() => UsePotion(index));
        }

        // Configurar Image slots para drag/drop
        for (int i = 0; i < slotImages.Length; i++)
        {
            int index = i;
            if (slotImages[i] != null)
            {
                // Asegurar que el Image pueda recibir raycasts
                slotImages[i].raycastTarget = true;

                // Agregar componente para manejar click derecho
                QuickSlotClickHandler handler = slotImages[i].gameObject.GetComponent<QuickSlotClickHandler>();
                if (handler == null)
                {
                    handler = slotImages[i].gameObject.AddComponent<QuickSlotClickHandler>();
                }
                handler.SetupSlot(this, index);

                // Agregar componente para manejar drop
                QuickSlotDropHandler dropHandler = slotImages[i].gameObject.GetComponent<QuickSlotDropHandler>();
                if (dropHandler == null)
                {
                    dropHandler = slotImages[i].gameObject.AddComponent<QuickSlotDropHandler>();
                }
                dropHandler.SetupSlot(this, index);
            }
        }

        // Inicializar slots vacíos
        for (int i = 0; i < quickSlotsPotions.Length; i++)
        {
            quickSlotsPotions[i] = null;
            RefreshSlotUI(i);
        }
    }

    void Update()
    {
        // Detectar hotkeys con el nuevo Input System
        if (Keyboard.current == null) return;

        for (int i = 0; i < hotkeys.Length; i++)
        {
            if (IsKeyDown(hotkeys[i]))
            {
                UsePotion(i);
            }
        }

        // Actualizar UI de los slots constantemente
        for (int i = 0; i < slotImages.Length; i++)
        {
            RefreshSlotUI(i);
        }
    }

    // Convertir KeyCode del inspector a Input System
    private bool IsKeyDown(KeyCode key)
    {
        return key switch
        {
            KeyCode.Q => Keyboard.current.qKey.wasPressedThisFrame,
            KeyCode.W => Keyboard.current.wKey.wasPressedThisFrame,
            KeyCode.E => Keyboard.current.eKey.wasPressedThisFrame,
            KeyCode.R => Keyboard.current.rKey.wasPressedThisFrame,
            KeyCode.T => Keyboard.current.tKey.wasPressedThisFrame,
            KeyCode.A => Keyboard.current.aKey.wasPressedThisFrame,
            KeyCode.S => Keyboard.current.sKey.wasPressedThisFrame,
            KeyCode.D => Keyboard.current.dKey.wasPressedThisFrame,
            KeyCode.F => Keyboard.current.fKey.wasPressedThisFrame,
            _ => false
        };
    }

    // Asignar una poción a un slot rápido (solo guardar la referencia)
    public void AssignPotionToSlot(ItemData potion, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickSlotsPotions.Length) return;
        if (potion == null || potion.itemType != ItemType.Potion) return;

        // Verificar que la poción existe en el inventario
        if (FindPotionInInventory(potion) == -1)
        {
            Debug.LogWarning($"Poción {potion.itemName} no encontrada en el inventario");
            return;
        }

        quickSlotsPotions[slotIndex] = potion;
        RefreshSlotUI(slotIndex);
        Debug.Log($"Poción {potion.itemName} asignada al slot {slotIndex + 1} (hotkey: {hotkeys[slotIndex]})");
    }

    // Encontrar el índice de una poción en el inventario
    private int FindPotionInInventory(ItemData potion)
    {
        if (inventory == null) return -1;

        var allItems = inventory.GetAllItems();
        for (int i = 0; i < allItems.Count; i++)
        {
            if (allItems[i] != null && allItems[i].data == potion)
            {
                return i;
            }
        }
        return -1;
    }

    // Usar poción de un slot rápido
    void UsePotion(int slotIndex)
    {
        if (inventory == null) return;
        if (slotIndex < 0 || slotIndex >= quickSlotsPotions.Length) return;

        ItemData potion = quickSlotsPotions[slotIndex];
        if (potion == null || potion.itemType != ItemType.Potion) return;

        // Verificar que la poción existe en el inventario
        if (GetPotionTotalAmount(potion) <= 0) return;

        // Usar la poción desde el inventario (esto ejecuta el efecto)
        inventory.UsePotion(potion);

        // Verificar si la poción se agotó completamente
        if (GetPotionTotalAmount(potion) <= 0)
        {
            // La poción se agotó, limpiar el slot
            quickSlotsPotions[slotIndex] = null;
        }

        RefreshSlotUI(slotIndex);
        Debug.Log($"Usaste {potion.itemName} desde el slot rápido {slotIndex + 1}");
    }

    // Actualizar la UI del slot
    void RefreshSlotUI(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotImages.Length) return;

        ItemData potion = quickSlotsPotions[slotIndex];

        if (potion == null)
        {
            // Slot vacío
            if (slotImages[slotIndex] != null)
            {
                slotImages[slotIndex].sprite = null;
                slotImages[slotIndex].color = Color.clear;
            }

            if (amountTexts[slotIndex] != null)
            {
                amountTexts[slotIndex].text = "";
                amountTexts[slotIndex].gameObject.SetActive(false);
            }
        }
        else
        {
            // Obtener la cantidad TOTAL de esa poción en el inventario
            int totalAmount = GetPotionTotalAmount(potion);

            if (totalAmount <= 0)
            {
                // La poción fue consumida completamente, limpiar el slot
                quickSlotsPotions[slotIndex] = null;
                RefreshSlotUI(slotIndex);
                return;
            }

            // Slot con poción
            if (slotImages[slotIndex] != null)
            {
                slotImages[slotIndex].sprite = potion.icon;
                slotImages[slotIndex].color = Color.white;
            }

            if (amountTexts[slotIndex] != null)
            {
                amountTexts[slotIndex].text = totalAmount.ToString();
                amountTexts[slotIndex].gameObject.SetActive(true);
            }
        }
    }

    // Obtener la cantidad TOTAL de una poción en el inventario
    private int GetPotionTotalAmount(ItemData potion)
    {
        if (inventory == null || potion == null) return 0;

        int total = 0;
        var allItems = inventory.GetAllItems();
        for (int i = 0; i < allItems.Count; i++)
        {
            if (allItems[i] != null && allItems[i].data == potion)
            {
                total += allItems[i].amount;
            }
        }
        return total;
    }

    // Obtener información de un slot
    public ItemData GetSlotPotion(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickSlotsPotions.Length) return null;
        return quickSlotsPotions[slotIndex];
    }

    // =============================
    // MÉTODOS PÚBLICOS PARA HANDLERS
    // =============================
    public void OnSlotPointerEnter(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotImages.Length) return;
        if (slotImages[slotIndex] != null)
        {
            slotImages[slotIndex].color = new Color(1f, 1f, 1f, 0.8f);
        }
    }

    public void OnSlotPointerExit(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotImages.Length) return;
        
        if (quickSlotsPotions[slotIndex] != null)
        {
            if (slotImages[slotIndex] != null)
                slotImages[slotIndex].color = Color.white;
        }
        else
        {
            if (slotImages[slotIndex] != null)
                slotImages[slotIndex].color = Color.clear;
        }
    }

    public void OnSlotPointerClick(int slotIndex, PointerEventData eventData)
    {
        // Click derecho = desequipar
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            eventData.Use();
            UnequipPotion(slotIndex);
            Debug.Log($"Click derecho en slot {slotIndex + 1}");
            return;
        }
    }

    public void OnSlotDrop(int slotIndex, PointerEventData eventData)
    {
        if (inventoryUI == null) return;
        inventoryUI.DropOnQuickSlot(slotIndex);
        Debug.Log($"Poción dragueada al quick slot {slotIndex + 1}");
    }

    public void UnequipPotion(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickSlotsPotions.Length) return;

        ItemData potion = quickSlotsPotions[slotIndex];
        if (potion == null)
        {
            Debug.Log($"El slot {slotIndex + 1} está vacío");
            return;
        }

        quickSlotsPotions[slotIndex] = null;
        RefreshSlotUI(slotIndex);
        Debug.Log($"Poción {potion.itemName} desequipada del slot {slotIndex + 1}");
    }
}
