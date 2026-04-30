using System.Collections;
using UnityEngine;

public class PlayerSlam : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private PlayerInputHandler _playerInput;
    [SerializeField] private PlayerStateController _playerState;
    [SerializeField] private PlayerJump _playerJump;
    [SerializeField] private PlayerSlamAttackBox _slamAttackBox;
    [SerializeField] private PlayerWallClimb _playerWallClimb;
    [SerializeField] private PlayerHook _playerHook;
    [SerializeField] private PlayerDash _playerDash;

    [Header("Slam Settings")]
    [SerializeField] private float _slamSpeed = 20f;
    [SerializeField] private float _slamStartPause = 0.1f;
    [SerializeField] private float _slamBounceForce = 10f;
    [SerializeField] private float _slamImpactAnimationDuration = 0.18f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask _groundLayer;

    public bool IsSlamImpactAnimationActive => _slamImpactAnimationTimer > 0f;

    private float _slamImpactAnimationTimer;

    private bool _isPauseActive;
    private float _originalGravity;

    public bool IsSlamStarting => _playerState != null && _playerState.IsSlamming && _isPauseActive;
    public bool IsSlamFalling => _playerState != null && _playerState.IsSlamming && !_isPauseActive;

    private Coroutine _slamPauseCoroutine;

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

        if (_playerJump == null)
        {
            _playerJump = GetComponent<PlayerJump>();
        }

        if (_slamAttackBox == null)
        {
            _slamAttackBox = GetComponentInChildren<PlayerSlamAttackBox>();
        }

        if (_playerWallClimb == null)
        {
            _playerWallClimb = GetComponent<PlayerWallClimb>();
        }

        if (_playerHook == null)
        {
            _playerHook = GetComponent<PlayerHook>();
        }

        if (_playerDash == null)
        {
            _playerDash = GetComponent<PlayerDash>();
        }
    }

    private void Update()
    {
        UpdateSlamAnimationTimer();
        HandleSlamInput();
    }

    private void FixedUpdate()
    {
        HandleSlamMovement();
    }

    private void HandleSlamInput()
    {
        if (!_playerInput.JumpPressed) return;
        if (_playerJump == null || _playerJump.IsGrounded) return;

        if (_playerInput.VerticalInput > -0.5f) return;

        if (_playerWallClimb != null)
        {
            if (_playerState != null && _playerState.IsWallClimbing) return;
            if (_playerWallClimb.IsWallJumping) return;
            if (_playerWallClimb.IsSlamInputBlocked) return;
        }

        StartSlam();
    }

    private void StartSlam()
    {
        if (!_playerState.TryChangeState(PlayerState.Slam)) return;

        _isPauseActive = true;
        _originalGravity = _rb.gravityScale;

        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero;

        _slamPauseCoroutine = StartCoroutine(SlamPauseRoutine());
    }

    private IEnumerator SlamPauseRoutine()
    {
        yield return new WaitForSeconds(_slamStartPause);

        _isPauseActive = false;
        _slamPauseCoroutine = null;
    }

    private void HandleSlamMovement()
    {
        if (!_playerState.IsSlamming) return;
        if (_isPauseActive) return;

        if (_slamAttackBox != null)
        {
            _slamAttackBox.StartSlamAttack();
        }

        _rb.linearVelocity = new Vector2(0f, -_slamSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_playerState.IsSlamming) return;

        bool hitGround = ((_groundLayer.value & (1 << collision.gameObject.layer)) != 0);

        if (!hitGround) return;

        EndSlam();
    }

    private void EndSlam()
    {
        _isPauseActive = false;

        if (_slamPauseCoroutine != null)
        {
            StopCoroutine(_slamPauseCoroutine);
            _slamPauseCoroutine = null;
        }

        if (_slamAttackBox != null)
        {
            _slamAttackBox.EndSlamAttack();
        }

        _rb.gravityScale = _originalGravity;
        _rb.linearVelocity = Vector2.zero;

        StartSlamImpactAnimation();

        _playerState.TryChangeState(PlayerState.Normal);
    }

    public void BounceAfterSlamKill()
    {
        _isPauseActive = false;

        if (_slamPauseCoroutine != null)
        {
            StopCoroutine(_slamPauseCoroutine);
            _slamPauseCoroutine = null;
        }

        if (_slamAttackBox != null)
        {
            _slamAttackBox.EndSlamAttack();
        }

        RechargeMovementResources();

        _rb.gravityScale = _originalGravity;
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _slamBounceForce);

        StartSlamImpactAnimation();

        _playerState.TryChangeState(PlayerState.Normal);
    }

    public void CancelSlam()
    {
        _isPauseActive = false;

        if (_slamPauseCoroutine != null)
        {
            StopCoroutine(_slamPauseCoroutine);
            _slamPauseCoroutine = null;
        }

        if (_slamAttackBox != null)
        {
            _slamAttackBox.EndSlamAttack();
        }

        _rb.gravityScale = _originalGravity;
        _playerState.TryChangeState(PlayerState.Normal);
    }

    private void RechargeMovementResources()
    {
        if (_playerHook != null)
        {
            _playerHook.RechargeHook();
        }

        if (_playerDash != null)
        {
            _playerDash.RechargeDash();
        }
    }

    private void StartSlamImpactAnimation()
    {
        _slamImpactAnimationTimer = _slamImpactAnimationDuration;
    }

    private void UpdateSlamAnimationTimer()
    {
        if (_slamImpactAnimationTimer <= 0f) return;

        _slamImpactAnimationTimer -= Time.deltaTime;

        if (_slamImpactAnimationTimer < 0f)
        {
            _slamImpactAnimationTimer = 0f;
        }
    }
}