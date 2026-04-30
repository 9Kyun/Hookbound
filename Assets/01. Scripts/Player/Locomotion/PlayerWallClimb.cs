using System;
using UnityEngine;

[DefaultExecutionOrder(-20)]
public class PlayerWallClimb : MonoBehaviour
{
    private enum WallSide
    {
        None,
        Left,
        Right
    }

    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private PlayerInputHandler _playerInput;
    [SerializeField] private PlayerStateController _playerState;
    [SerializeField] private PlayerJump _playerJump;
    [SerializeField] private PlayerDash _playerDash;
    [SerializeField] private PlayerHook _playerHook;

    [Header("Wall Check")]
    [SerializeField] private Transform _wallCheckLeftPoint;
    [SerializeField] private Transform _wallCheckRightPoint;
    [SerializeField] private Vector2 _wallCheckBoxSize = new Vector2(0.15f, 0.8f);
    [SerializeField] private LayerMask _wallLayer;

    [Header("Wall Climb Settings")]
    [SerializeField] private float _wallClimbSpeed = 5f;
    [SerializeField] private float _wallSlideSpeed = 2f;
    [SerializeField] private float _maxStamina = 5f;
    [SerializeField] private float _wallStaminaDrainPerSecond = 1f;

    [Header("Wall Jump Settings")]
    [SerializeField] private float _wallJumpHorizontalSpeed = 10f;
    [SerializeField] private float _wallJumpForceY = 15f;
    [SerializeField] private float _wallJumpControlLockTime = 0.15f;
    [SerializeField] private float _wallJumpDetachTime = 0.1f;
    [SerializeField] private float _wallJumpGraceTime = 0.12f;
    [SerializeField] private float _slamBlockAfterWallJump = 0.15f;

    public bool IsTouchingLeftWall { get; private set; }
    public bool IsTouchingRightWall { get; private set; }
    public bool IsTouchingWall => IsTouchingLeftWall || IsTouchingRightWall;

    public float CurrentWallStamina { get; private set; }
    public bool HasWallStamina => CurrentWallStamina > 0f;

    public bool IsWallJumping { get; private set; }
    public bool IsSlamInputBlocked => _wallJumpSlamBlockTimer > 0f;

    private float _wallJumpControlTimer;
    private float _wallJumpDetachTimer;
    private float _wallJumpGraceTimer;
    private float _wallJumpSlamBlockTimer;

    private int _wallJumpDirection;

    private WallSide _currentWallSide = WallSide.None;
    private WallSide _lastWallSide = WallSide.None;

    private void Awake()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInputHandler>();

        if (_playerState == null)
            _playerState = GetComponent<PlayerStateController>();

        if (_playerJump == null)
            _playerJump = GetComponent<PlayerJump>();

        if (_playerDash == null)
            _playerDash = GetComponent<PlayerDash>();

        if (_playerHook == null)
            _playerHook = GetComponent<PlayerHook>();

        if (_wallCheckLeftPoint == null)
            _wallCheckLeftPoint = transform.Find("WallCheckLeftPoint");

        if (_wallCheckRightPoint == null)
            _wallCheckRightPoint = transform.Find("WallCheckRightPoint");

        CurrentWallStamina = _maxStamina;
    }

    private void Update()
    {
        CheckWall();
        UpdateTimers();
        RechargeWallStamina();
        HandleWallJumpInput();
        HandleWallClimbState();
        UpdateWallStamina();
    }

    private void FixedUpdate()
    {
        if (IsWallJumping)
        {
            HandleWallJumpMovement();
            return;
        }

        HandleWallMovement();
    }

    private void CheckWall()
    {
        IsTouchingLeftWall = false;
        IsTouchingRightWall = false;

        if (_wallCheckLeftPoint != null)
        {
            IsTouchingLeftWall = Physics2D.OverlapBox(
                _wallCheckLeftPoint.position,
                _wallCheckBoxSize,
                0f,
                _wallLayer
            );
        }

        if (_wallCheckRightPoint != null)
        {
            IsTouchingRightWall = Physics2D.OverlapBox(
                _wallCheckRightPoint.position,
                _wallCheckBoxSize,
                0f,
                _wallLayer
            );
        }

        UpdateCurrentWallSide();
    }

    private void UpdateCurrentWallSide()
    {
        WallSide detectedSide = WallSide.None;

        if (IsTouchingLeftWall && !IsTouchingRightWall)
        {
            detectedSide = WallSide.Left;
        }
        else if (!IsTouchingLeftWall && IsTouchingRightWall)
        {
            detectedSide = WallSide.Right;
        }
        else if (IsTouchingLeftWall && IsTouchingRightWall)
        {
            float movementInput = _playerInput != null ? _playerInput.MovementInput : 0f;

            if (movementInput < -0.1f)
            {
                detectedSide = WallSide.Left;
            }
            else if (movementInput > 0.1f)
            {
                detectedSide = WallSide.Right;
            }
            else
            {
                detectedSide = _lastWallSide;
            }
        }

        _currentWallSide = detectedSide;

        if (_currentWallSide != WallSide.None)
        {
            _lastWallSide = _currentWallSide;
            _wallJumpGraceTimer = _wallJumpGraceTime;
        }
    }

    private void UpdateTimers()
    {
        if (_wallJumpControlTimer > 0f)
        {
            _wallJumpControlTimer -= Time.deltaTime;

            if (_wallJumpControlTimer <= 0f)
            {
                _wallJumpControlTimer = 0f;
                IsWallJumping = false;
            }
        }

        if (_wallJumpDetachTimer > 0f)
        {
            _wallJumpDetachTimer -= Time.deltaTime;

            if (_wallJumpDetachTimer < 0f)
                _wallJumpDetachTimer = 0f;
        }

        if (_wallJumpGraceTimer > 0f)
        {
            _wallJumpGraceTimer -= Time.deltaTime;

            if (_wallJumpGraceTimer < 0f)
                _wallJumpGraceTimer = 0f;
        }

        if (_wallJumpSlamBlockTimer > 0f)
        {
            _wallJumpSlamBlockTimer -= Time.deltaTime;

            if (_wallJumpSlamBlockTimer < 0f)
                _wallJumpSlamBlockTimer = 0f;
        }
    }

    private void RechargeWallStamina()
    {
        if (_playerJump == null) return;

        if (_playerJump.IsGrounded)
        {
            CurrentWallStamina = _maxStamina;
        }
    }

    private void UpdateWallStamina()
    {
        if (_playerState == null) return;
        if (!_playerState.IsWallClimbing) return;
        if (_playerJump != null && _playerJump.IsGrounded) return;

        CurrentWallStamina -= _wallStaminaDrainPerSecond * Time.deltaTime;
        CurrentWallStamina = Mathf.Max(CurrentWallStamina, 0f);
    }

    private void HandleWallClimbState()
    {
        if (_playerState == null) return;

        if (CanAttachToWall())
        {
            if (!_playerState.IsWallClimbing)
            {
                _playerState.TryChangeState(PlayerState.WallClimb);
            }
        }
        else
        {
            ExitWallClimb();
        }
    }

    private bool CanAttachToWall()
    {
        if (_playerState == null)
            return false;
        if (_playerJump != null && _playerJump.IsGrounded)
            return false;
        if (_playerDash != null && _playerDash.IsDashing)
            return false;
        if (_playerHook != null && _playerHook.IsGrappling)
            return false;
        if (!(_playerState.IsNormal || _playerState.IsWallClimbing))
            return false;
        if (_playerState.IsDead || _playerState.IsHit || _playerState.IsSlamming)
            return false;
        if (_currentWallSide == WallSide.None)
            return false;
        if (IsWallJumping)
            return false;
        if (_wallJumpDetachTimer > 0f)
            return false;
        if (!IsPressingTowardCurrentWall())
            return false;

        return true;
    }

    private bool IsPressingTowardCurrentWall()
    {
        float movementInput = _playerInput.MovementInput;

        if (_currentWallSide == WallSide.Left && movementInput < -0.1f)
            return true;

        if (_currentWallSide == WallSide.Right && movementInput > 0.1f)
            return true;

        return false;
    }

    private void HandleWallMovement()
    {
        if (_playerState == null) return;
        if (!_playerState.IsWallClimbing) return;

        float targetY = -_wallSlideSpeed;

        if (HasWallStamina)
        {
            float verticalInput = _playerInput.VerticalInput;

            if (verticalInput > 0.1f)
            {
                targetY = _wallClimbSpeed;
            }
            else if (verticalInput < -0.1f)
            {
                targetY = -_wallClimbSpeed;
            }
            else
            {
                targetY = -_wallSlideSpeed;
            }
        }
        else
        {
            targetY = -_wallSlideSpeed;
        }

        _rb.linearVelocity = new Vector2(0f, targetY);
    }

    private void HandleWallJumpInput()
    {
        if (!_playerInput.JumpPressed) return;
        if (_playerJump != null && _playerJump.IsGrounded) return;
        if (_playerDash != null && _playerDash.IsDashing) return;
        if (_playerHook != null && _playerHook.IsGrappling) return;
        if (_playerState != null && (_playerState.IsDead || _playerState.IsHit || _playerState.IsSlamming)) return;

        WallSide jumpWallSide = GetWallJumpAvailableSide();

        if (jumpWallSide == WallSide.None)
            return;

        StartWallJump(jumpWallSide);
    }

    private WallSide GetWallJumpAvailableSide()
    {
        if (_currentWallSide != WallSide.None)
            return _currentWallSide;

        if (_wallJumpGraceTimer > 0f)
            return _lastWallSide;

        return WallSide.None;
    }

    private void StartWallJump(WallSide jumpWallSide)
    {
        _wallJumpDirection = GetJumpDirectionFromWallSide(jumpWallSide);

        if (_wallJumpDirection == 0)
            return;

        ExitWallClimb();

        IsWallJumping = true;
        _wallJumpControlTimer = _wallJumpControlLockTime;
        _wallJumpDetachTimer = _wallJumpDetachTime;
        _wallJumpSlamBlockTimer = _slamBlockAfterWallJump;

        _rb.linearVelocity = new Vector2(
            _wallJumpDirection * _wallJumpHorizontalSpeed,
            _wallJumpForceY
        );
    }

    private int GetJumpDirectionFromWallSide(WallSide wallSide)
    {
        if (wallSide == WallSide.Left) return 1;
        if (wallSide == WallSide.Right) return -1;
        return 0;
    }

    private void HandleWallJumpMovement()
    {
        if (_wallJumpControlTimer <= 0f) return;

        _rb.linearVelocity = new Vector2(
            _wallJumpDirection * _wallJumpHorizontalSpeed,
            _rb.linearVelocity.y
        );
    }

    private void ExitWallClimb()
    {
        if (_playerState != null && _playerState.IsWallClimbing)
        {
            _playerState.TryChangeState(PlayerState.Normal);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        if (_wallCheckLeftPoint != null)
        {
            Gizmos.DrawWireCube(_wallCheckLeftPoint.position, _wallCheckBoxSize);
        }

        if (_wallCheckRightPoint != null)
        {
            Gizmos.DrawWireCube(_wallCheckRightPoint.position, _wallCheckBoxSize);
        }
    }
}