using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInputHandler _playerInput;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private PlayerStateController _playerState;
    [SerializeField] private PlayerHook _playerHook;
    [SerializeField] private PlayerJump _playerJump;
    [SerializeField] private PlayerDash _playerDash;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _accelerationSpeed = 30f;
    [SerializeField] private float _groundDeccelerationSpeed = 40f;
    [SerializeField] private float _airDeccelerationSpeed = 12f;
    [SerializeField] private float _turnSpeed = 50f;

    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        if (_playerInput == null)
        {
            _playerInput = GetComponent<PlayerInputHandler>();
        }

        if (_playerState == null)
        {
            _playerState = GetComponent<PlayerStateController>();
        }

        if (_playerDash == null)
        {
            _playerDash = GetComponent<PlayerDash>();
        }

        if (_playerHook == null)
        {
            _playerHook = GetComponent<PlayerHook>();
        }

        if (_playerJump == null)
        {
            _playerJump = GetComponent<PlayerJump>();
        }
    }

    private void FixedUpdate()
    {
        PlayerLocomotion();
    }

    private void PlayerLocomotion()
    {
        if (_playerState != null && _playerState.IsDead) return;
        if (_playerState != null && _playerState.IsHit) return;

        if (_playerDash != null && _playerDash.IsDashing) return;

        if (_playerHook != null && (_playerHook.IsHookPreparing || _playerHook.IsGrappling)) return;

        if (_playerState != null && _playerState.IsSlamming) return;

        float movementInput = _playerInput.MovementInput;
        float currentVelocityX = _rb.linearVelocity.x;
        float newVelocityX = currentVelocityX;

        float controlMultiplier = 1f;

        if (_playerHook != null && _playerHook.IsHookInertiaActive)
        {
            controlMultiplier = _playerHook.HookInertiaControlMultiplier;
        }

        bool hasMovementInput = Mathf.Abs(movementInput) > 0.001f;

        if (hasMovementInput)
        {
            float targetVelocityX = movementInput * _moveSpeed;
            float accelRate;

            if (Mathf.Abs(currentVelocityX) > 0.001f &&
                Mathf.Sign(movementInput) != Mathf.Sign(currentVelocityX))
            {
                accelRate = _turnSpeed;
            }
            else
            {
                accelRate = _accelerationSpeed;
            }

            accelRate *= controlMultiplier;
            newVelocityX = Mathf.MoveTowards(
                currentVelocityX,
                targetVelocityX,
                accelRate * Time.fixedDeltaTime
            );
        }
        else
        {
            float decelRate = (_playerJump != null && _playerJump.IsGrounded)
                ? _groundDeccelerationSpeed
                : _airDeccelerationSpeed;

            decelRate *= controlMultiplier;
            newVelocityX = Mathf.MoveTowards(
                currentVelocityX,
                0f,
                decelRate * Time.fixedDeltaTime
            );
        }

        _rb.linearVelocity = new Vector2(newVelocityX, _rb.linearVelocity.y);
    }

    public void SetMoveSpeed(float moveSpeed)
    {
        _moveSpeed = Mathf.Max(0f, moveSpeed);
    }
}
