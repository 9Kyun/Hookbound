using Unity.VisualScripting;
using UnityEngine;

public class GroundEnemyAI : MonoBehaviour
{
    private enum PatrolState
    {
        Move,
        Idle
    }

    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private EnemyStateController _enemyState;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Transform _wallCheck;
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Move Settings")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _groundCheckDistance = 0.3f;
    [SerializeField] private float _wallCheckDistance = 0.2f;

    [Header("Random Patrol Timing")]
    [SerializeField] private float _minMoveTime = 1f;
    [SerializeField] private float _maxMoveTime = 2.5f;
    [SerializeField] private float _minIdleTime = 0.5f;
    [SerializeField] private float _maxIdleTime = 1.5f;

    [Header("Layer")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _wallLayer;

    [Header("Direction")]
    [SerializeField] private bool _startMoveRight = true;

    private PatrolState _currentState = PatrolState.Move;
    private float _stateTimer;
    private int _moveDirection = 1;

    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }
        if (_enemyState == null)
        {
            _enemyState = GetComponent<EnemyStateController>();
        }
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        _moveDirection = _startMoveRight ? 1 : -1;
        EnterMoveState();
        UpdateVisualDirection();
    }

    private void Update()
    {
        if (!CanRunAI())
        {
            StopHorizontalMove();
            return;
        }

        UpdateStateTimer();
    }

    private void FixedUpdate()
    {
        if (!CanRunAI())
        {
            StopHorizontalMove();
            return;
        }

        switch (_currentState)
        {
            case PatrolState.Move:
                HandleMoveState();
                break;

            case PatrolState.Idle:
                HandleIdleState();
                break;
        }
    }

    private bool CanRunAI()
    {
        if (_enemyState == null) return false;
        if (_enemyState.IsDead) return false;
        if (_enemyState.IsPulled) return false;
        if (_enemyState.IsStunned) return false;

        return true;
    }

    private void UpdateStateTimer()
    {
        _stateTimer -= Time.deltaTime;

        if (_stateTimer > 0) return;

        if (_currentState == PatrolState.Move)
        {
            EnterIdleState();
        }
        else
        {
            EnterMoveState();
        }
    }

    private void HandleMoveState()
    {
        if (ShouldTurn())
        {
            Turn();
        }

        _rb.linearVelocity = new Vector2(_moveDirection * _moveSpeed, _rb.linearVelocity.y);
    }

    private void HandleIdleState()
    {
        StopHorizontalMove();
    }

    private bool ShouldTurn()
    {
        bool hasGroundAhead = CheckGroundAhead();
        bool hitWallAhead = CheckWallAhead();

        return !hasGroundAhead || hitWallAhead;
    }

    private bool CheckGroundAhead()
    {
        if (_groundCheck == null)
        {
            return true;
        }

        RaycastHit2D hit = Physics2D.Raycast(_groundCheck.position, Vector2.down, _groundCheckDistance, _groundLayer);

        return hit.collider != null;
    }

    private bool CheckWallAhead()
    {
        if (_wallCheck == null)
        {
            return false;
        }

        Vector2 direction = _moveDirection > 0 ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(_wallCheck.position, direction, _wallCheckDistance, _wallLayer);

        return hit.collider != null;
    }

    private void Turn()
    {
        _moveDirection *= -1;
        UpdateVisualDirection();
    }

    private void EnterMoveState()
    {
        _currentState = PatrolState.Move;
        _stateTimer = Random.Range(_minMoveTime, _maxMoveTime);
    }

    private void EnterIdleState()
    {
        _currentState = PatrolState.Idle;
        _stateTimer = Random.Range(_minIdleTime, _maxIdleTime);
        StopHorizontalMove();
    }

    private void StopHorizontalMove()
    {
        if (_rb == null) return;

        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
    }
    
    private void UpdateVisualDirection()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.flipX = _moveDirection < 0;
            return;
        }

        if (_visualRoot != null)
        {
            Vector3 scale = _visualRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * (_moveDirection > 0 ? 1f : -1f);
            _visualRoot.localScale = scale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                _groundCheck.position,
                _groundCheck.position + Vector3.down * _groundCheckDistance
            );
        }

        if (_wallCheck != null)
        {
            Gizmos.color = Color.red;

            Vector3 direction = Application.isPlaying
                ? (_moveDirection > 0 ? Vector3.right : Vector3.left)
                : (_startMoveRight ? Vector3.right : Vector3.left);

            Gizmos.DrawLine(
                _wallCheck.position,
                _wallCheck.position + direction * _wallCheckDistance
            );
        }
    }
}
