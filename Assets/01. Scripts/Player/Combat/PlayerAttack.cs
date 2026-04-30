using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInputHandler _playerInput;
    [SerializeField] private PlayerStateController _playerState;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private PlayerLifeSteal _playerLifeSteal;

    [Header("Attack Settings")]
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _attackOffset = 1.2f;
    [SerializeField] private Vector2 _attackBoxSize = new Vector2(1.5f, 1f);
    [SerializeField] private float _attackActiveTime = 0.08f;
    [SerializeField] private float _attackCooldown = 0.15f;
    [SerializeField] private LayerMask _enemyHitBoxLayer;
    [SerializeField] private LayerMask _projectileLayer;
    [SerializeField] private float _attackAnimationDuration = 0.18f;

    [Header("Kill Dash Settings")]
    [SerializeField] private float _killDashTime = 0.08f;
    [SerializeField] private float _killDashExtraDistance = 0.8f;
    [SerializeField] private float _killDashInertia = 6f;

    [Header("Feedback")]
    [SerializeField] private GameFeedbackManager _feedback;

    public bool IsAttackAnimationActive => _attackAnimationTimer > 0f;
    public Vector2 CurrentAttackDirection => _attackDirection;

    private float _attackAnimationTimer;

    private bool _isAttacking;
    private bool _isKillDashing;
    private bool _isAttackPierceEnabled;
    private bool _feedbackPlayedThisAttack;

    private float _attackCooldownTimer;
    private float _killDashTimer;
    private float _killDashSpeed;
    private float _originalGravity;

    private Vector2 _attackCenter;
    private float _attackAngle;
    private Vector2 _attackDirection = Vector2.right;
    private Vector2 _killDashDirection = Vector2.right;

    private readonly HashSet<EnemyHitBox> _hitTargets = new HashSet<EnemyHitBox>();
    private readonly HashSet<EnemyProjectile> _parriedProjectiles = new HashSet<EnemyProjectile>();

    private void Awake()
    {
        if (_playerInput == null)
        {
            _playerInput = GetComponent<PlayerInputHandler>();
        }

        if (_playerState == null)
        {
            _playerState = GetComponent<PlayerStateController>();
        }

        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_playerLifeSteal == null)
        {
            _playerLifeSteal = GetComponent<PlayerLifeSteal>();
        }

        if (_feedback == null)
        {
            _feedback = FindFirstObjectByType<GameFeedbackManager>();
        }
    }

    private void Update()
    {
        UpdateAttackAnimationTimer();
        UpdateCooldown();
        UpdateKillDashTimer();
        HandleAttackInput();
    }

    private void FixedUpdate()
    {
        HandleKillDashMovement();
    }

    private void UpdateCooldown()
    {
        if (_attackCooldownTimer > 0f)
        {
            _attackCooldownTimer -= Time.deltaTime;
        }
    }

    private void UpdateKillDashTimer()
    {
        if (!_isKillDashing) return;

        _killDashTimer -= Time.deltaTime;

        if (_killDashTimer <= 0f)
        {
            EndKillDash();
        }
    }

    private void HandleAttackInput()
    {
        if (!_playerInput.AttackPressed) return;
        if (_attackCooldownTimer > 0f) return;
        if (_isKillDashing) return;
        if (!_playerState.CanAttack()) return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        if (!_playerState.TryChangeState(PlayerState.Attack))
        {
            yield break;
        }

        _isAttacking = true;
        _feedbackPlayedThisAttack = false;
        _attackCooldownTimer = _attackCooldown;

        _hitTargets.Clear();
        _parriedProjectiles.Clear();

        AttackShapeSetup();
        StartAttackAnimation();

        EnemyHitBox killedTarget = ApplyAttackHit();

        if (killedTarget != null)
        {
            StartKillDash(killedTarget.transform.position);

            while (_isKillDashing)
            {
                yield return null;
            }
        }
        else
        {
            float timer = _attackActiveTime;

            while (timer > 0f)
            {
                EnemyHitBox lateKilledTarget = ApplyAttackHit();

                if (lateKilledTarget != null)
                {
                    StartKillDash(lateKilledTarget.transform.position);

                    while (_isKillDashing)
                    {
                        yield return null;
                    }

                    break;
                }

                timer -= Time.deltaTime;
                yield return null;
            }
        }

        _isAttacking = false;

        _hitTargets.Clear();
        _parriedProjectiles.Clear();

        if (!_playerState.IsDead && !_playerState.IsHit)
        {
            _playerState.TryChangeState(PlayerState.Normal);
        }
    }

    private void AttackShapeSetup()
    {
        Vector2 playerPosition = transform.position;
        Vector2 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(_playerInput.MousePosition);

        _attackDirection = (mouseWorldPosition - playerPosition).normalized;

        if (_attackDirection == Vector2.zero)
        {
            _attackDirection = Vector2.right;
        }

        _attackCenter = playerPosition + _attackDirection * _attackOffset;
        _attackAngle = Mathf.Atan2(_attackDirection.y, _attackDirection.x) * Mathf.Rad2Deg;
    }

    private EnemyHitBox ApplyAttackHit()
    {
        ApplyProjectileParry();
        return ApplyEnemyHit();
    }

    private EnemyHitBox ApplyEnemyHit()
    {
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
            _attackCenter,
            _attackBoxSize,
            _attackAngle,
            _enemyHitBoxLayer
        );

        EnemyHitBox killedTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hitCollider in hitColliders)
        {
            EnemyHitBox enemyHitBox = hitCollider.GetComponent<EnemyHitBox>();

            if (enemyHitBox == null)
            {
                enemyHitBox = hitCollider.GetComponentInParent<EnemyHitBox>();
            }

            if (enemyHitBox == null) continue;
            if (_hitTargets.Contains(enemyHitBox)) continue;

            PlayerAttackType attackType = _isAttackPierceEnabled
                ? PlayerAttackType.PiercingMelee
                : PlayerAttackType.Melee;

            EnemyHitResult hitResult = enemyHitBox.ReceiveHit(attackType, _damage);

            if (hitResult == EnemyHitResult.None)
                continue;

            _hitTargets.Add(enemyHitBox);

            if (!_feedbackPlayedThisAttack)
            {
                bool isKill = hitResult == EnemyHitResult.Killed;
                _feedback?.PlayAttackHitFeedback(isKill);
                _feedbackPlayedThisAttack = true;
            }

            if (hitResult != EnemyHitResult.Killed)
                continue;

            if (_playerLifeSteal != null)
            {
                _playerLifeSteal.NotifyEnemyKilled();
            }

            float currentDistance = Vector2.Distance(transform.position, enemyHitBox.transform.position);

            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                killedTarget = enemyHitBox;
            }
        }

        return killedTarget;
    }

    private void ApplyProjectileParry()
    {
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
            _attackCenter,
            _attackBoxSize,
            _attackAngle,
            _projectileLayer
        );

        foreach (Collider2D hitCollider in hitColliders)
        {
            EnemyProjectile enemyProjectile = hitCollider.GetComponentInParent<EnemyProjectile>();

            if (enemyProjectile == null) continue;
            if (_parriedProjectiles.Contains(enemyProjectile)) continue;

            bool wasParried = enemyProjectile.TryParry(_attackDirection);

            if (!wasParried) continue;

            _parriedProjectiles.Add(enemyProjectile);
        }
    }

    private void StartKillDash(Vector2 killedTargetPosition)
    {
        _isKillDashing = true;
        _killDashTimer = _killDashTime;
        _originalGravity = _rb.gravityScale;

        Vector2 destination = killedTargetPosition + (_attackDirection * _killDashExtraDistance);
        Vector2 currentPosition = _rb.position;

        _killDashDirection = (destination - currentPosition).normalized;

        if (_killDashDirection == Vector2.zero)
        {
            _killDashDirection = _attackDirection;
        }

        float dashDistance = Vector2.Distance(currentPosition, destination);
        _killDashSpeed = dashDistance / _killDashTime;

        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero;
    }

    private void HandleKillDashMovement()
    {
        if (!_isKillDashing) return;

        _rb.linearVelocity = _killDashDirection * _killDashSpeed;
    }

    private void EndKillDash()
    {
        _isKillDashing = false;
        _killDashTimer = 0f;

        _rb.gravityScale = _originalGravity;
        _rb.linearVelocity = _killDashDirection * _killDashInertia;
    }

    public void SetAttackPierceEnabled(bool enabled)
    {
        _isAttackPierceEnabled = enabled;
    }

    private void StartAttackAnimation()
    {
        _attackAnimationTimer = _attackAnimationDuration;
    }

    private void UpdateAttackAnimationTimer()
    {
        if (_attackAnimationTimer <= 0f) return;

        _attackAnimationTimer -= Time.deltaTime;

        if (_attackAnimationTimer < 0f)
        {
            _attackAnimationTimer = 0f;
        }
    }

    private void OnDrawGizmos()
    {
        if (!_isAttacking) return;

        Gizmos.color = Color.red;
        Matrix4x4 originalMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            _attackCenter,
            Quaternion.Euler(0f, 0f, _attackAngle),
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, _attackBoxSize);
        Gizmos.matrix = originalMatrix;
    }
}