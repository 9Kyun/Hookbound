using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private PlayerInputHandler _playerInput;
    [SerializeField] private PlayerStateController _playerState;

    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 15f;
    [SerializeField] private float _jumpCutMultiplier = 0.5f;
    [SerializeField] private float _fallGravityMultiplier = 2.5f;
    [SerializeField] private float _lowJumpGravityMultiplier = 2f;
    [SerializeField] private float _maxFallSpeed = 5f;

    [Header("Ground Check")]
    [SerializeField] private Collider2D _bodyCollider;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundMinNormalY = 0.5f;
    [SerializeField] private float _footContactTolerance = 0.08f;
    [SerializeField] private float _groundedMaxYVelocity = 0.1f;

    public bool IsGrounded { get; private set; }

    // 더블점프 이후 착지 전까지 true 유지
    public bool IsDoubleJumpAnimationActive { get; private set; }

    private bool _jumpQueued;
    private bool _jumpCutQueued;
    private readonly ContactPoint2D[] _groundContacts = new ContactPoint2D[8];

    private int _extraJumpCount;
    private int _remainingExtraJumpCount;

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

        if (_bodyCollider == null)
        {
            _bodyCollider = GetComponent<Collider2D>();
        }
    }

    private void Update()
    {
        if (_playerState != null && (_playerState.IsSlamming || _playerState.IsWallClimbing))
        {
            _jumpQueued = false;
            _jumpCutQueued = false;
            return;
        }

        if (_playerInput.JumpPressed)
        {
            _jumpQueued = true;
        }

        if (_playerInput.JumpReleased)
        {
            _jumpCutQueued = true;
        }
    }

    private void FixedUpdate()
    {
        GroundCheck();

        // 착지하면 더블점프 애니메이션 상태 해제
        if (IsGrounded)
        {
            EndDoubleJumpAnimation();
        }

        if (_playerState != null && (_playerState.IsSlamming || _playerState.IsWallClimbing)) return;

        HandleJumpPhysics();
        HandleFallVelocity();
    }

    private void HandleJumpPhysics()
    {
        if (IsGrounded)
        {
            _remainingExtraJumpCount = _extraJumpCount;
        }

        if (_jumpQueued && _playerState.IsNormal)
        {
            if (IsGrounded)
            {
                Jump();
                EndDoubleJumpAnimation();
            }
            else if (_remainingExtraJumpCount > 0)
            {
                _remainingExtraJumpCount--;
                Jump();
                StartDoubleJumpAnimation();
            }
        }

        if (_jumpCutQueued && _rb.linearVelocity.y > 0f)
        {
            _rb.linearVelocity = new Vector2(
                _rb.linearVelocity.x,
                _rb.linearVelocity.y * _jumpCutMultiplier
            );
        }

        _jumpQueued = false;
        _jumpCutQueued = false;
    }

    private void HandleFallVelocity()
    {
        // 떨어질 때
        if (_rb.linearVelocity.y < 0f)
        {
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (_fallGravityMultiplier - 1) * Time.fixedDeltaTime;
        }

        // 점프 끊을 때
        else if (_rb.linearVelocity.y > 0f && !_playerInput.JumpHeld)
        {
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (_lowJumpGravityMultiplier - 1) * Time.fixedDeltaTime;
        }

        _rb.linearVelocity = new Vector2(
            _rb.linearVelocity.x,
            Mathf.Clamp(_rb.linearVelocity.y, -_maxFallSpeed, 100f)
        );
    }

    private void Jump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
    }

    private void GroundCheck()
    {
        IsGrounded = false;

        if (_bodyCollider == null || _rb == null)
            return;

        // 위로 올라가는 중에는 바닥 판정을 하지 않음
        if (_rb.linearVelocity.y > _groundedMaxYVelocity)
            return;

        float footLimitY = _bodyCollider.bounds.min.y + _footContactTolerance;

        int contactCount = _bodyCollider.GetContacts(_groundContacts);

        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = _groundContacts[i];

            if (contact.collider == null)
                continue;

            bool isGroundLayer = ((_groundLayer.value & (1 << contact.collider.gameObject.layer)) != 0);
            if (!isGroundLayer)
                continue;

            // 바닥면에 가까운 normal만 인정
            if (contact.normal.y < _groundMinNormalY)
                continue;

            // 접촉점이 몸 아래쪽 근처가 아니면 무시
            if (contact.point.y > footLimitY)
                continue;

            IsGrounded = true;
            return;
        }
    }

    public void SetExtraJumpCount(int extraJumpCount)
    {
        _extraJumpCount = Mathf.Max(0, extraJumpCount);

        if (_remainingExtraJumpCount > _extraJumpCount)
        {
            _remainingExtraJumpCount = _extraJumpCount;
        }

        if (_extraJumpCount <= 0)
        {
            EndDoubleJumpAnimation();
        }
    }

    private void StartDoubleJumpAnimation()
    {
        IsDoubleJumpAnimationActive = true;
    }

    private void EndDoubleJumpAnimation()
    {
        IsDoubleJumpAnimationActive = false;
    }
}