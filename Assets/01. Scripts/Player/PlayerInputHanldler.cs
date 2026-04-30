using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour, GameInputActions.IPlayerActions
{
    public GameInputActions gameInputActions { get; private set; }
    public float MovementInput { get; private set; }
    public float VerticalInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpReleased { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool DashPressed { get; private set; }
    public Vector2 MousePosition { get; private set; }
    public bool HookPressed { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool InteractPressed { get; private set; }



    private void Awake()
    {
        gameInputActions = new GameInputActions();
    }

    private void OnEnable()
    {
        gameInputActions.Enable();
        gameInputActions.Player.SetCallbacks(this);
    }

    private void OnDisable()
    {
        gameInputActions.Disable();
        gameInputActions.Player.RemoveCallbacks(this);
    }

    private void LateUpdate()
    {
        JumpPressed = false;
        JumpReleased = false;
        DashPressed = false;
        HookPressed = false;
        AttackPressed = false;
        InteractPressed = false;
    }


    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            AttackPressed = true;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            DashPressed = true;
        }
    }

    public void OnHook(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            HookPressed = true;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InteractPressed = true;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            JumpPressed = true;
            JumpHeld = true;
        }
        if (context.canceled)
        {
            JumpReleased = true;
            JumpHeld = false;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        MousePosition = context.ReadValue<Vector2>();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        MovementInput = context.ReadValue<float>();
    }

    public void OnVertical(InputAction.CallbackContext context)
    {
        VerticalInput = context.ReadValue<float>();
    }
}
