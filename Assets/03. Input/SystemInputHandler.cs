using UnityEngine;
using UnityEngine.InputSystem;

public class SystemInputHandler : MonoBehaviour, GameInputActions.ISystemActions
{
    private GameInputActions _inputActions;

    public bool PausePressed { get; private set; }

    private void Awake()
    {
        _inputActions = new GameInputActions();
        _inputActions.System.SetCallbacks(this);
    }

    private void OnEnable()
    {
        _inputActions.System.Enable();
    }

    private void OnDisable()
    {
        _inputActions.System.Disable();
    }

    private void LateUpdate()
    {
        PausePressed = false;
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PausePressed = true;
        }
    }
}