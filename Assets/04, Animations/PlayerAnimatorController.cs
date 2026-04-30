using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private PlayerStateController _playerState;
    [SerializeField] private PlayerJump _playerJump;
    [SerializeField] private PlayerInputHandler _playerInput;
    [SerializeField] private PlayerAttack _playerAttack;
    [SerializeField] private PlayerSlam _playerSlam;
    [SerializeField] private PlayerWallClimb _playerWallClimb;

    [Header("Animation Settings")]
    [SerializeField] private float _moveThreshold = 0.1f;
    [SerializeField] private float _jumpMidVelocityThreshold = 1.2f;
    [SerializeField] private float _attackVerticalThreshold = 0.5f;

    private PlayerAnimationState _currentAnimationState;
    private bool _hasAnimationState;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        if (_playerState == null)
            _playerState = GetComponent<PlayerStateController>();

        if (_playerJump == null)
            _playerJump = GetComponent<PlayerJump>();

        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInputHandler>();

        if (_playerAttack == null)
            _playerAttack = GetComponent<PlayerAttack>();

        if (_playerSlam == null)
            _playerSlam = GetComponent<PlayerSlam>();

        if (_playerWallClimb == null)
            _playerWallClimb = GetComponent<PlayerWallClimb>();
    }

    private void Update()
    {
        UpdateSpriteDirection();

        PlayerAnimationState nextAnimationState = DecideAnimationState();
        PlayAnimation(nextAnimationState);
    }

    private PlayerAnimationState DecideAnimationState()
    {
        if (_playerState == null)
        {
            return PlayerAnimationState.SwordIdle;
        }

        if (_playerState.IsDead)
        {
            return PlayerAnimationState.Die;
        }

        if (_playerState.IsHit)
        {
            return PlayerAnimationState.HitDamage;
        }

        if (_playerState.IsDashing)
        {
            return PlayerAnimationState.Dash;
        }

        if (_playerState.IsGrappling)
        {
            return PlayerAnimationState.HookDash;
        }

        if (_playerState.IsSlamming)
        {
            return DecideActiveSlamAnimation();
        }

        if (_playerSlam != null && _playerSlam.IsSlamImpactAnimationActive)
        {
            return PlayerAnimationState.GroundSlam;
        }

        if (_playerWallClimb != null && _playerWallClimb.IsWallJumping)
        {
            return PlayerAnimationState.WallJump;
        }

        if (_playerState.IsWallClimbing)
        {
            return DecideWallAnimation();
        }

        if (_playerAttack != null && _playerAttack.IsAttackAnimationActive)
        {
            return DecideAttackAnimation();
        }

        if (_playerJump != null && _playerJump.IsDoubleJumpAnimationActive)
        {
            return PlayerAnimationState.DoubleJump;
        }

        return DecideNormalAnimation();
    }

    private PlayerAnimationState DecideActiveSlamAnimation()
    {
        if (_playerSlam != null && _playerSlam.IsSlamStarting)
        {
            return PlayerAnimationState.GroundSlam;
        }

        return PlayerAnimationState.SlamFall;
    }

    private PlayerAnimationState DecideNormalAnimation()
    {
        if (_rb == null || _playerJump == null)
        {
            return PlayerAnimationState.SwordIdle;
        }

        if (!_playerJump.IsGrounded)
        {
            if (_rb.linearVelocity.y > _jumpMidVelocityThreshold)
            {
                return PlayerAnimationState.SwordJumpRise;
            }

            if (Mathf.Abs(_rb.linearVelocity.y) <= _jumpMidVelocityThreshold)
            {
                return PlayerAnimationState.SwordJumpMid;
            }

            return PlayerAnimationState.SwordJumpFall;
        }

        if (Mathf.Abs(_rb.linearVelocity.x) > _moveThreshold)
        {
            return PlayerAnimationState.SwordRun;
        }

        return PlayerAnimationState.SwordIdle;
    }

    private PlayerAnimationState DecideAttackAnimation()
    {
        Vector2 attackDirection = GetAttackDirection();

        bool isGrounded = _playerJump != null && _playerJump.IsGrounded;
        bool isMoving = _rb != null && Mathf.Abs(_rb.linearVelocity.x) > _moveThreshold;

        bool isUpAttack = attackDirection.y > _attackVerticalThreshold;
        bool isDownAttack = attackDirection.y < -_attackVerticalThreshold;

        if (isGrounded)
        {
            if (isUpAttack)
            {
                return PlayerAnimationState.GroundSlashUp;
            }

            if (isDownAttack)
            {
                return PlayerAnimationState.GroundSlashDown;
            }

            if (isMoving)
            {
                return PlayerAnimationState.SwordRunSlash;
            }

            return PlayerAnimationState.StandingSlash;
        }

        if (isUpAttack)
        {
            return PlayerAnimationState.AirSlashUp;
        }

        if (isDownAttack)
        {
            return PlayerAnimationState.AirSlashDown;
        }

        return PlayerAnimationState.AirSlash;
    }

    private PlayerAnimationState DecideWallAnimation()
    {
        if (_playerInput == null || _playerWallClimb == null)
        {
            return PlayerAnimationState.WallSlide;
        }

        if (_playerWallClimb.IsWallJumping)
        {
            return PlayerAnimationState.WallJump;
        }

        bool hasVerticalInput = Mathf.Abs(_playerInput.VerticalInput) > 0.1f;

        if (hasVerticalInput && _playerWallClimb.HasWallStamina)
        {
            return PlayerAnimationState.WallClimb;
        }

        return PlayerAnimationState.WallSlide;
    }

    private Vector2 GetAttackDirection()
    {
        if (_playerAttack == null)
        {
            return Vector2.right;
        }

        Vector2 attackDirection = _playerAttack.CurrentAttackDirection;

        if (attackDirection.sqrMagnitude <= 0.0001f)
        {
            return Vector2.right;
        }

        return attackDirection.normalized;
    }

    private void PlayAnimation(PlayerAnimationState nextAnimationState)
    {
        if (_animator == null)
        {
            return;
        }

        if (_hasAnimationState && _currentAnimationState == nextAnimationState)
        {
            return;
        }

        _currentAnimationState = nextAnimationState;
        _hasAnimationState = true;

        _animator.Play(nextAnimationState.ToString(), 0, 0f);
    }

    private void UpdateSpriteDirection()
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        if (_playerAttack != null && _playerAttack.IsAttackAnimationActive)
        {
            Vector2 attackDirection = GetAttackDirection();

            if (Mathf.Abs(attackDirection.x) > 0.1f)
            {
                _spriteRenderer.flipX = attackDirection.x < 0f;
            }

            return;
        }

        if (_playerInput == null)
        {
            return;
        }

        float moveInput = _playerInput.MovementInput;

        if (Mathf.Abs(moveInput) < 0.01f)
        {
            return;
        }

        _spriteRenderer.flipX = moveInput < 0f;
    }
}