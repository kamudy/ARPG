// Auto-generated PlayerInputActions stub for Input System
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputActions
{
    public PlayerInputActionMap Player { get; private set; }

    public PlayerInputActions()
    {
        Player = new PlayerInputActionMap();
    }

    public void Enable()
    {
        Player?.Enable();
    }

    public void Disable()
    {
        Player?.Disable();
    }

    [System.Serializable]
    public class PlayerInputActionMap
    {
        public InputAction Move { get; set; } = new InputAction("Move", InputActionType.Value);
        public InputAction Look { get; set; } = new InputAction("Look", InputActionType.Value);
        public InputAction Fire { get; set; } = new InputAction("Fire", InputActionType.Button);
        public InputAction Sprint { get; set; } = new InputAction("Sprint", InputActionType.Button);
        public InputAction Dash { get; set; } = new InputAction("Dash", InputActionType.Button);
        public InputAction Interact { get; set; } = new InputAction("Interact", InputActionType.Button);
        public InputAction Inventory { get; set; } = new InputAction("Inventory", InputActionType.Button);
        public InputAction Character { get; set; } = new InputAction("Character", InputActionType.Button);
        public InputAction Shop { get; set; } = new InputAction("Shop", InputActionType.Button);
        public InputAction Click { get; set; } = new InputAction("Click", InputActionType.Button);
        public InputAction ToggleInventory { get; set; } = new InputAction("ToggleInventory", InputActionType.Button);
        public InputAction OpenCharacter { get; set; } = new InputAction("OpenCharacter", InputActionType.Button);

        public void Enable()
        {
            Move?.Enable();
            Look?.Enable();
            Fire?.Enable();
            Sprint?.Enable();
            Dash?.Enable();
            Interact?.Enable();
            Inventory?.Enable();
            Character?.Enable();
            Shop?.Enable();
            Click?.Enable();
            ToggleInventory?.Enable();
            OpenCharacter?.Enable();
        }

        public void Disable()
        {
            Move?.Disable();
            Look?.Disable();
            Fire?.Disable();
            Sprint?.Disable();
            Dash?.Disable();
            Interact?.Disable();
            Inventory?.Disable();
            Character?.Disable();
            Shop?.Disable();
            Click?.Disable();
            ToggleInventory?.Disable();
            OpenCharacter?.Disable();
        }
    }
}
