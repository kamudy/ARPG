using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Tooltip")]
    public ItemTooltipUI tooltip;

    [Header("Refs")]
    public PlayerInventory inventory;

    [Header("UI")]
    public GameObject inventoryPanel;
    public TMP_Text coinsText;

    [Header("Slots")]
    public UIItemSlot slotPrefab;      // ItemSlot.prefab
    public Transform itemGrid;         // ScrollView/Viewport/ItemGrid
    public int maxSlotsUI = 60;

    [Header("Drag Icon")]
    public Canvas canvas;
    public RectTransform dragIcon;
    public UnityEngine.UI.Image dragIconImage;

    private readonly List<UIItemSlot> slots = new();

    // drag state
    private bool dragging;
    private int dragFromIndex = -1;

    // inventory open
    private bool isOpen;

    // input
    private PlayerInputActions inputActions;

    public bool IsDragging => dragging;
    public int DragFromIndex => dragFromIndex;
    public bool IsOpen => isOpen;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        
        // Si no está asignado, buscar en la escena
        if (inventory == null)
            inventory = FindFirstObjectByType<PlayerInventory>();
    }

    void OnEnable()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<PlayerInventory>();
            
        if (inventory != null)
        {
            inventory.OnInventoryChanged += Refresh;
            Debug.Log("✅ InventoryUI suscrito a OnInventoryChanged");
        }
        else
        {
            Debug.LogWarning("❌ InventoryUI: PlayerInventory no encontrado en OnEnable");
        }

        inputActions.Player.Enable();
        inputActions.Player.ToggleInventory.performed += OnToggleInventory;
    }

    void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= Refresh;

        inputActions.Player.ToggleInventory.performed -= OnToggleInventory;
        inputActions.Player.Disable();
    }

    void Start()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        CreateDragIconIfNeeded();
        CreateSlots();

        isOpen = false;
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        // Refrescar después de crear slots
        Refresh();
    }

    public void RefreshUI()
    {
        // Método público para refrescar desde SaveManager u otros scripts
        Refresh();
    }

    // =========================
    // INVENTORY TOGGLE
    // =========================
    void OnToggleInventory(InputAction.CallbackContext ctx)
    {
        ToggleInventory();
    }

    void ToggleInventory()
    {
        isOpen = !isOpen;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(isOpen);

        // cancelar drag si se cierra
        if (!isOpen)
        {
            EndDrag();
            HideTooltip();
        }

        // Actualizar UI cuando se abre
        if (isOpen)
            Refresh();
    }

    // =========================
    // TOOLTIP API (lo llama UIItemSlot)
    // =========================
    public void ShowTooltip(ItemData item)
    {
        if (tooltip == null || item == null) return;

        PlayerStats stats = inventory != null ? inventory.GetComponent<PlayerStats>() : null;
        PlayerLevel lvl = inventory != null ? inventory.GetComponent<PlayerLevel>() : null;

        tooltip.Show(item, stats, lvl);
    }

    public void HideTooltip()
    {
        if (tooltip == null) return;
        tooltip.Hide();
    }

    // =========================
    // SETUP
    // =========================
    void CreateDragIconIfNeeded()
    {
        if (dragIcon != null && dragIconImage != null) return;

        GameObject go = new GameObject("DragIcon");
        go.transform.SetParent(canvas.transform, false);

        dragIcon = go.AddComponent<RectTransform>();
        dragIcon.sizeDelta = new Vector2(48, 48);

        dragIconImage = go.AddComponent<UnityEngine.UI.Image>();
        dragIconImage.raycastTarget = false;

        dragIcon.gameObject.SetActive(false);
    }

    void CreateSlots()
    {
        if (slots.Count > 0) return;

        int count = (inventory != null) ? inventory.SlotCount : maxSlotsUI;

        for (int i = 0; i < count; i++)
        {
            UIItemSlot s = Instantiate(slotPrefab, itemGrid);
            s.Setup(this, i);
            s.Clear();
            slots.Add(s);
        }
    }

    // =========================
    // REFRESH UI
    // =========================
    public void Refresh()
    {
        if (inventory == null) return;
        
        // Proteger si los slots no están inicializados
        if (slots == null || slots.Count == 0)
        {
            Debug.Log("⚠️ InventoryUI.Refresh(): Slots no inicializados, saltando refresh");
            return;
        }

        Debug.Log($"🔄 InventoryUI.Refresh() - Actualizando UI con {inventory.GetAllItems().Count} items");

        if (coinsText != null)
            coinsText.text = $"Monedas: {inventory.coins}";

        for (int i = 0; i < slots.Count; i++)
        {
            var stack = inventory.GetStackAt(i);
            
            // ✅ VALIDACION: Si el stack tiene data null, limpiar silenciosamente (sin warning)
            if (stack == null || stack.data == null)
            {
                slots[i].Clear();
            }
            else
            {
                Debug.Log($"  📦 Slot {i}: {stack.data.name} x{stack.amount}");
                slots[i].Set(stack);
            }
        }
    }

    // =========================
    // DRAG API (UIItemSlot)
    // =========================
    public void BeginDrag(int index)
    {
         if (inventory == null) return;

    var stack = inventory.GetStackAt(index);
    if (stack == null || stack.data == null) return;

    dragging = true;
    dragFromIndex = index;

    if (dragIconImage != null)
    {
        dragIconImage.sprite = stack.data.icon;
        dragIconImage.color = Color.white;
        dragIconImage.raycastTarget = false;
    }

    if (dragIcon != null)
    {
        dragIcon.gameObject.SetActive(true);

        // ✅ que no aparezca en el centro
        if (Mouse.current != null)
            dragIcon.position = Mouse.current.position.ReadValue();

        // ✅ arriba de todo
        dragIcon.SetAsLastSibling();
    }
    }

    public void Drag(Vector2 screenPos)
    {
        if (!dragging) return;
        if (dragIcon != null)
            dragIcon.position = screenPos;
    }

    public void EndDrag()
    {
        dragging = false;
        dragFromIndex = -1;

        if (dragIcon != null)
            dragIcon.gameObject.SetActive(false);
    }

    public void DropOn(int targetIndex)
    {
        if (!dragging || inventory == null) return;
        if (dragFromIndex < 0) return;

        inventory.SwapSlots(dragFromIndex, targetIndex);
        EndDrag();
    }

    // =========================
    // DROP EN EQUIPAMIENTO
    // =========================
    public void DropOnEquipment(EquipmentSlotType target)
    {
        if (!dragging || inventory == null) return;

        bool toRightHand = true;
        if (target == EquipmentSlotType.WeaponLeft) toRightHand = false;
        if (target == EquipmentSlotType.WeaponRight) toRightHand = true;

        inventory.EquipFromSlot(dragFromIndex, toRightHand);
        EndDrag();
    }

    // =========================
    // DROP EN QUICK SLOTS
    // =========================
    public void DropOnQuickSlot(int slotIndex)
    {
        if (!dragging || inventory == null) return;
        if (dragFromIndex < 0) return;

        var stack = inventory.GetStackAt(dragFromIndex);
        if (stack == null || stack.data == null) return;

        // Solo permitir pociones
        if (stack.data.itemType != ItemType.Potion)
        {
            Debug.LogWarning("Solo se pueden equipar pociones en quick slots");
            EndDrag();
            return;
        }

        QuickSlotsUI quickSlots = FindFirstObjectByType<QuickSlotsUI>();
        if (quickSlots != null)
        {
            quickSlots.AssignPotionToSlot(stack.data, slotIndex);
        }

        EndDrag();
    }

    // =========================
    // CLICK DERECHO: USAR / EQUIPAR / ASIGNAR A QUICK SLOTS
    // =========================
    public void RightClick(int index)
    {
        if (inventory == null) return;

        var stack = inventory.GetStackAt(index);
        if (stack == null || stack.data == null) return;

        ItemData item = stack.data;

        if (item.itemType == ItemType.Potion)
        {
            // Intentar asignar a un quick slot vacío en lugar de usar directamente
            QuickSlotsUI quickSlots = FindFirstObjectByType<QuickSlotsUI>();
            if (quickSlots != null)
            {
                // Encontrar el primer slot vacío
                for (int i = 0; i < 3; i++)
                {
                    if (quickSlots.GetSlotPotion(i) == null)
                    {
                        quickSlots.AssignPotionToSlot(item, i);
                        Debug.Log($"Poción {item.itemName} asignada al slot rápido {i + 1}");
                        return;
                    }
                }
                Debug.LogWarning("Todos los slots rápidos están llenos");
            }
            return;
        }

        if (item.itemType == ItemType.Armor)
        {
            inventory.Equip(item);
            return;
        }

        if (item.itemType == ItemType.Weapon)
        {
            bool shift = Keyboard.current != null &&
                         (Keyboard.current.leftShiftKey.isPressed ||
                          Keyboard.current.rightShiftKey.isPressed);

            // sin shift = derecha | con shift = izquierda
            inventory.Equip(item, toRightHand: !shift);
            return;
        }
    }
}
