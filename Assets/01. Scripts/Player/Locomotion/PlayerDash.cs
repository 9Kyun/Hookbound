using System;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInputHandler _playerInput;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private PlayerStateController _playerState;
    [SerializeField] private PlayerJump _playerJump;

    [Header("Dash Settings")]
    [SerializeField] private float _dashDistance;
    [SerializeField] private float _dashTime;
    [SerializeField] private float _dashInertia;

    [SerializeField] private int _maxDashCount = 3;
    [SerializeField] private float _dashCooldown = 0.5f;

    [Header("Just Evade Settings")]
    [SerializeField] private float _justEvadeWindow = 0.08f;

    public bool IsDashing { get; private set; }
    public bool IsJustEvadeWindowActive { get; private set; }

    public int CurrentDashCount { get; private set; }
    public int MaxDashCount => _maxDashCount;

    private Vector2 _dashDirection = Vector2.up;
    private float _dashSpeed;
    private float _dashTimer;
    private float _justEvadeTimer;
    private float _originalGravity;

    public float _dashCooldownTimer;
    private bool _wasGroundedLastFrame;
    private bool _dashQueued;

    private int _baseMaxDashCount;
    private int _bonusMaxDashCount;

    private void Awake()
    {
        if (_playerInput == null)
        {
            _playerInput = GetComponent<PlayerInputHandler>();
        }
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }
        if (_playerState == null)
        {
            _playerState = GetComponent<PlayerStateController>();
        }
        if (_playerJump== null)
        {
            _playerJump = GetComponent<PlayerJump>();
        }

        _dashSpeed = _dashDistance / _dashTime;
        _baseMaxDashCount = _maxDashCount;
        RefreshMaxDashCount(true);
    }

    private void Update()
    {
        HandleDashInput();
        UpdateDashTimers();
        UpdateDashCooldown();
    }

    private void FixedUpdate()
    {
        TryStartDash();
        Dash();
        HandleGroundRecharge();
    }

    private void HandleDashInput()
    {
        if (!_playerInput.DashPressed) return;

        _dashQueued = true;
    }

    private void TryStartDash()
    {
        if (!_dashQueued) return;

        _dashQueued = false;
        DashSetup();
    }

    private void HandleDashDirection()
    {
        _dashDirection = new Vector2(_playerInput.MovementInput, _playerInput.VerticalInput).normalized;

        if (_dashDirection == Vector2.zero)
        {
            _dashDirection = Vector2.up;
        }
    }

    private void DashSetup()
    {
        if (CurrentDashCount <= 0) return;
        if (_dashCooldownTimer > 0f) return;
        if (!_playerState.TryChangeState(PlayerState.Dash)) return;

        HandleDashDirection();

        CurrentDashCount--;
        _dashCooldownTimer = _dashCooldown;

        IsDashing = true;
        IsJustEvadeWindowActive = true;
        _dashTimer = _dashTime;
        _justEvadeTimer = _justEvadeWindow;
        _originalGravity = _rb.gravityScale;

        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero;
    }

    private void Dash()
    {
        if (!IsDashing) return;
        _rb.linearVelocity = _dashDirection * _dashSpeed;
    }

    private void UpdateDashTimers()
    {
        if (!IsDashing) return;

        _dashTimer -= Time.deltaTime;

        if (IsJustEvadeWindowActive)
        {
            _justEvadeTimer -= Time.deltaTime;

            if (_justEvadeTimer <= 0f)
            {
                EndJustEvade();
            }
        }

        if (_dashTimer <= 0f)
        {
            EndDash();
        }
    }

    private void UpdateDashCooldown()
    {
        if (_dashCooldownTimer > 0f)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void HandleGroundRecharge()
    {
        if (_playerJump == null) return;

        bool isGrounded = _playerJump.IsGrounded;

        if (!_wasGroundedLastFrame && isGrounded)
        {
            RechargeDash();
        }

        _wasGroundedLastFrame = isGrounded;
    }

    public void RechargeDash()
    {
        CurrentDashCount = _maxDashCount;
    }

    private void EndDash()
    {
        IsDashing = false;
        _dashTimer = 0f;

        EndJustEvade();

        _rb.gravityScale = _originalGravity;

        _rb.linearVelocity = _dashDirection * _dashInertia;

        if (!_playerState.IsDead && !_playerState.IsHit)
        {
            _playerState.TryChangeState(PlayerState.Normal);
        }
    }

    private void EndJustEvade()
    {
        IsJustEvadeWindowActive = false;
        _justEvadeTimer = 0f;
    }

    public void CancelDash()
    {
        if (!IsDashing) return;

        IsDashing = false;
        _dashTimer = 0f;

        IsJustEvadeWindowActive = false;
        _justEvadeTimer = 0f;

        _rb.gravityScale = _originalGravity;
        _rb.linearVelocity = Vector2.zero;

        if (!_playerState.IsDead && !_playerState.IsHit)
        {
            _playerState.TryChangeState(PlayerState.Normal);
        }
    }

    public void SetBonusMaxDashCount(int bonusCount)
    {
        _bonusMaxDashCount = Mathf.Max(0, bonusCount);
        RefreshMaxDashCount(false);
    }

    private void RefreshMaxDashCount(bool refill)
    {
        int previousMaxCount = _maxDashCount;

        _maxDashCount = Mathf.Max(1, _baseMaxDashCount + _bonusMaxDashCount);

        if (refill)
        {
            CurrentDashCount = _maxDashCount;
            return;
        }

        int maxCountDifference = _maxDashCount - previousMaxCount;

        if (maxCountDifference > 0)
        {
            CurrentDashCount += maxCountDifference;
        }

        CurrentDashCount = Mathf.Clamp(CurrentDashCount, 0, _maxDashCount);
    }
}
