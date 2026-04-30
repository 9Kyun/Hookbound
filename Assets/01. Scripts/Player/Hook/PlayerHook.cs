using System.Collections;
using UnityEngine;

public class PlayerHook : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInputHandler _playerInput;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private PlayerHookAim _hookAim;
    [SerializeField] private PlayerStateController _playerState;
    [SerializeField] private PlayerDash _playerDash;
    [SerializeField] private PlayerJump _playerJump;

    [Header("Hook Settings")]
    [SerializeField] private float _hookTime = 0.2f;
    [SerializeField] private float _hookInertia = 12f;
    [SerializeField] private float _hookStoppingGapDistance = 0.3f;

    [Header("Hook Presentation")]
    [SerializeField] private float _hookStartPause = 0.06f;
    [SerializeField] private TrailRenderer _hookTrail;
    [SerializeField] private float _hookTrailTime = 0.15f;

    [Header("Airborne Hook Settings")]
    [SerializeField] private float _airborneHookOvershootDistance = 2f;

    [Header("Light Enemy Hook Settings")]
    [SerializeField] private float _lightEnemyPlayerOffset = 2f;

    [Header("Projectile Hook Settings")]
    [SerializeField] private float _projectileHookCrossDistance = 0.5f;

    [Header("Hook Inertia Control")]
    [SerializeField] private float _hookInertiaDuration = 0.12f;
    [SerializeField, Range(0.05f, 1f)] private float _hookInertiaControlMultiplier = 0.25f;

    [SerializeField] private int _maxHookCount = 3;
    [SerializeField] private float _hookUseCooldown = 0.15f;

    public bool IsGrappling { get; private set; }
    public bool IsHookPreparing { get; private set; }

    public bool IsHookInertiaActive => _hookInertiaTimer > 0f;
    public float HookInertiaControlMultiplier => IsHookInertiaActive ? _hookInertiaControlMultiplier : 1f;

    public int CurrentHookCount { get; private set; }
    public int MaxHookCount => _maxHookCount;

    public Vector2 ActiveHookPoint
    {
        get
        {
            if (_activeHookFollowTarget != null)
            {
                return _activeHookFollowTarget.TransformPoint(_activeHookLocalPoint);
            }

            return _activeHookPoint;
        }
    }

    private Vector2 _hookDirection = Vector2.zero;
    private Vector2 _enemyPullDirection = Vector2.zero;

    private float _hookSpeed;
    private float _hookTimer;
    private float _hookCooldownTimer;
    private float _hookInertiaTimer;
    private float _originalGravity;
    private bool _wasGroundedLastFrame;
    private float _currentHookRatio = 1f;

    private bool _hookQueued;
    private HookableTarget _queuedHookTarget;
    private Vector2 _queuedHookDirection;
    private float _queuedHookDistance;
    private Vector2 _queuedHookTargetPoint;

    private int _baseMaxHookCount;
    private int _bonusMaxHookCount;
    private float _hookSpeedBonusPercent;

    private EnemyHookReceiver _activeEnemyHookReceiver;
    private EnemyProjectile _activeProjectileHook;

    private Coroutine _hookPrepareCoroutine;

    private Transform _activeHookFollowTarget;
    private Vector2 _activeHookPoint;
    private Vector3 _activeHookLocalPoint;

    private void Awake()
    {
        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInputHandler>();

        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        if (_hookAim == null)
            _hookAim = GetComponent<PlayerHookAim>();

        if (_playerState == null)
            _playerState = GetComponent<PlayerStateController>();

        if (_playerDash == null)
            _playerDash = GetComponent<PlayerDash>();

        if (_playerJump == null)
            _playerJump = GetComponent<PlayerJump>();

        if (_hookTrail != null)
        {
            _hookTrail.emitting = false;
            _hookTrail.time = _hookTrailTime;
            _hookTrail.Clear();
        }

        _baseMaxHookCount = _maxHookCount;
        RefreshMaxHookCount(true);
    }

    private void Update()
    {
        HandleHookInput();
        UpdateHookTimer();
        UpdateHookCooldown();
        UpdateHookInertiaTimer();
    }

    private void FixedUpdate()
    {
        TryStartQueuedHook();
        HandleGrappleMovement();
        HandleGroundRecharge();
    }

    private void HandleHookInput()
    {
        if (!_playerInput.HookPressed) return;
        if (_hookAim == null) return;
        if (_hookAim.CurrentTarget == null) return;
        if (_hookAim.HookAimDirection == Vector2.zero) return;

        _hookQueued = true;
        _queuedHookTarget = _hookAim.CurrentTarget;
        _queuedHookDirection = _hookAim.HookAimDirection;
        _queuedHookDistance = _hookAim.HookDistance;
        _queuedHookTargetPoint = _hookAim.CurrentTargetPoint;
    }

    private void TryStartQueuedHook()
    {
        if (!_hookQueued) return;

        HookableTarget queuedTarget = _queuedHookTarget;
        Vector2 queuedDirection = _queuedHookDirection;
        float queuedDistance = _queuedHookDistance;
        Vector2 queuedTargetPoint = _queuedHookTargetPoint;

        ClearQueuedHook();

        HookSetup(queuedTarget, queuedDirection, queuedDistance, queuedTargetPoint);
    }

    private void ClearQueuedHook()
    {
        _hookQueued = false;
        _queuedHookTarget = null;
        _queuedHookDirection = Vector2.zero;
        _queuedHookDistance = 0f;
        _queuedHookTargetPoint = Vector2.zero;
    }

    private void UpdateHookTimer()
    {
        if (!IsGrappling) return;

        _hookTimer -= Time.deltaTime;

        if (_hookTimer <= 0f)
        {
            EndHook(true);
        }
    }

    private void UpdateHookCooldown()
    {
        if (_hookCooldownTimer > 0f)
        {
            _hookCooldownTimer -= Time.deltaTime;
        }
    }

    private void UpdateHookInertiaTimer()
    {
        if (_hookInertiaTimer <= 0f) return;

        _hookInertiaTimer -= Time.deltaTime;

        if (_hookInertiaTimer < 0f)
        {
            _hookInertiaTimer = 0f;
        }
    }

    private void HandleGroundRecharge()
    {
        if (_playerJump == null) return;

        bool isGrounded = _playerJump.IsGrounded;

        if (!_wasGroundedLastFrame && isGrounded)
        {
            RechargeHook();
        }

        _wasGroundedLastFrame = isGrounded;
    }

    private void HookSetup(
        HookableTarget currentTarget,
        Vector2 hookAimDirection,
        float hookDistance,
        Vector2 hookTargetPoint)
    {
        if (currentTarget == null) return;
        if (hookAimDirection == Vector2.zero) return;
        if (CurrentHookCount <= 0) return;
        if (_hookCooldownTimer > 0f) return;
        if (!_playerState.TryChangeState(PlayerState.Grapple)) return;

        ClearHookInertia();

        _hookDirection = hookAimDirection;
        _enemyPullDirection = hookAimDirection;
        _currentHookRatio = 1f;
        _activeEnemyHookReceiver = null;
        _activeProjectileHook = null;

        EnemyHookReceiver enemyHookReceiver = null;
        float enemyPullDistance = 0f;

        EnemyProjectile projectile = currentTarget.GetComponentInParent<EnemyProjectile>();

        if (projectile != null && projectile.IsReflected)
        {
            if (!CalculateProjectileHookData(projectile))
            {
                _playerState.TryChangeState(PlayerState.Normal);
                return;
            }
        }
        else
        {
            EnemyData enemyData = currentTarget.GetComponentInParent<EnemyData>();
            enemyHookReceiver = currentTarget.GetComponentInParent<EnemyHookReceiver>();

            Transform enemyReferenceTransform = enemyHookReceiver != null
                ? enemyHookReceiver.transform
                : currentTarget.transform;

            float playerTravelDistance;

            CalculateHookTravelData(
                currentTarget,
                enemyData,
                enemyReferenceTransform,
                hookAimDirection,
                hookDistance,
                out playerTravelDistance,
                out enemyPullDistance
            );

            if (playerTravelDistance <= 0.01f)
            {
                _playerState.TryChangeState(PlayerState.Normal);
                return;
            }

            float hookSpeedMultiplier = 1f + (_hookSpeedBonusPercent / 100f);
            _hookSpeed = (playerTravelDistance / _hookTime) * hookSpeedMultiplier;

            if (enemyHookReceiver != null && !enemyHookReceiver.CanBeHooked())
            {
                _playerState.TryChangeState(PlayerState.Normal);
                return;
            }
        }

        CurrentHookCount--;
        _hookCooldownTimer = _hookUseCooldown;

        IsHookPreparing = true;
        IsGrappling = false;
        _hookTimer = 0f;

        _originalGravity = _rb.gravityScale;

        SetupActiveHookPoint(currentTarget, hookTargetPoint);

        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero;

        if (_hookPrepareCoroutine != null)
        {
            StopCoroutine(_hookPrepareCoroutine);
        }

        _hookPrepareCoroutine = StartCoroutine(
            HookPrepareRoutine(enemyHookReceiver, enemyPullDistance, projectile)
        );
    }

    private void SetupActiveHookPoint(HookableTarget currentTarget, Vector2 hookTargetPoint)
    {
        _activeHookPoint = hookTargetPoint;
        _activeHookFollowTarget = null;
        _activeHookLocalPoint = Vector3.zero;

        EnemyData enemyData = currentTarget.GetComponentInParent<EnemyData>();
        EnemyProjectile projectile = currentTarget.GetComponentInParent<EnemyProjectile>();

        bool shouldFollowTarget = enemyData != null || projectile != null;

        if (!shouldFollowTarget)
        {
            return;
        }

        _activeHookFollowTarget = currentTarget.transform;
        _activeHookLocalPoint = currentTarget.transform.InverseTransformPoint(hookTargetPoint);
    }

    private IEnumerator HookPrepareRoutine(
        EnemyHookReceiver enemyHookReceiver,
        float enemyPullDistance,
        EnemyProjectile projectile)
    {
        yield return new WaitForSecondsRealtime(_hookStartPause);

        _hookPrepareCoroutine = null;

        if (_playerState.IsDead || _playerState.IsHit)
        {
            EndHook(false);
            yield break;
        }

        if (projectile != null)
        {
            bool isProjectileHookStarted = projectile.TryStartProjectileHook(
                _rb.position,
                _hookTime,
                _projectileHookCrossDistance
            );

            if (!isProjectileHookStarted)
            {
                AbortPreparedHook();
                yield break;
            }

            _activeProjectileHook = projectile;
        }
        else if (enemyHookReceiver != null)
        {
            bool isPullStarted = enemyHookReceiver.TryStartPull(
                _enemyPullDirection,
                enemyPullDistance,
                _hookTime
            );

            if (!isPullStarted)
            {
                AbortPreparedHook();
                yield break;
            }

            if (enemyHookReceiver.IsBeingPulled)
            {
                _activeEnemyHookReceiver = enemyHookReceiver;
            }
        }

        IsHookPreparing = false;
        IsGrappling = true;
        _hookTimer = _hookTime;

        if (_hookTrail != null)
        {
            _hookTrail.time = _hookTrailTime;
            _hookTrail.Clear();
            _hookTrail.emitting = true;
        }
    }

    private void AbortPreparedHook()
    {
        if (_hookPrepareCoroutine != null)
        {
            StopCoroutine(_hookPrepareCoroutine);
            _hookPrepareCoroutine = null;
        }

        IsHookPreparing = false;
        IsGrappling = false;
        _hookTimer = 0f;

        _rb.gravityScale = _originalGravity;
        _rb.linearVelocity = Vector2.zero;

        ClearActiveHookPoint();

        if (_hookTrail != null)
        {
            _hookTrail.emitting = false;
            _hookTrail.Clear();
        }

        if (CurrentHookCount < _maxHookCount)
        {
            CurrentHookCount++;
        }

        if (!_playerState.IsDead && !_playerState.IsHit)
        {
            _playerState.TryChangeState(PlayerState.Normal);
        }
    }

    private bool CalculateProjectileHookData(EnemyProjectile projectile)
    {
        Vector2 playerPosition = _rb.position;
        Vector2 projectilePosition = projectile.transform.position;
        Vector2 playerMoveVector = projectilePosition - playerPosition;
        float distance = playerMoveVector.magnitude;

        if (distance <= 0.0001f)
        {
            return false;
        }

        _hookDirection = playerMoveVector.normalized;
        _enemyPullDirection = Vector2.zero;
        _currentHookRatio = 1f;

        float playerTravelDistance = distance * 0.5f + _projectileHookCrossDistance;
        float hookSpeedMultiplier = 1f + (_hookSpeedBonusPercent / 100f);

        _hookSpeed = (playerTravelDistance / _hookTime) * hookSpeedMultiplier;

        return true;
    }

    private void CalculateHookTravelData(
        HookableTarget currentTarget,
        EnemyData enemyData,
        Transform enemyReferenceTransform,
        Vector2 hookAimDirection,
        float hookDistance,
        out float playerTravelDistance,
        out float enemyPullDistance)
    {
        playerTravelDistance = 0f;
        enemyPullDistance = 0f;

        float baseDistance = hookDistance;

        if (enemyData != null)
        {
            float pullRate = Mathf.Clamp01(enemyData.PullRate);

            switch (enemyData.WeightType)
            {
                case WeightType.Light:
                    CalculateLightEnemyTravelData(
                        enemyReferenceTransform.position,
                        baseDistance,
                        pullRate,
                        hookAimDirection,
                        out playerTravelDistance,
                        out enemyPullDistance
                    );
                    return;

                case WeightType.Heavy:
                    enemyPullDistance = baseDistance * pullRate;
                    playerTravelDistance = Mathf.Max(
                        baseDistance - enemyPullDistance - _hookStoppingGapDistance,
                        0f
                    );

                    _hookDirection = hookAimDirection;
                    _enemyPullDirection = hookAimDirection;
                    _currentHookRatio = 1f - pullRate;
                    return;

                case WeightType.Static:
                default:
                    break;
            }
        }

        _hookDirection = hookAimDirection;
        _enemyPullDirection = hookAimDirection;
        _currentHookRatio = 1f;

        if (currentTarget.TargetType == HookTargetType.Airborne)
        {
            playerTravelDistance = Mathf.Max(baseDistance + _airborneHookOvershootDistance, 0f);
            return;
        }

        playerTravelDistance = Mathf.Max(baseDistance - _hookStoppingGapDistance, 0f);
    }

    private void CalculateLightEnemyTravelData(
        Vector2 enemyStartPosition,
        float baseDistance,
        float pullRate,
        Vector2 hookAimDirection,
        out float playerTravelDistance,
        out float enemyPullDistance)
    {
        Vector2 playerStartPosition = _rb.position;

        enemyPullDistance = baseDistance * pullRate;
        _enemyPullDirection = hookAimDirection;

        Vector2 enemyFinalPosition =
            enemyStartPosition - (_enemyPullDirection * enemyPullDistance);

        Vector2 playerTargetPosition =
            enemyFinalPosition + Vector2.up * _lightEnemyPlayerOffset;

        Vector2 playerMoveVector = playerTargetPosition - playerStartPosition;

        if (playerMoveVector.sqrMagnitude <= 0.0001f)
        {
            _hookDirection = Vector2.up;
            playerTravelDistance = 0f;
            _currentHookRatio = 1f;
            return;
        }

        _hookDirection = playerMoveVector.normalized;
        playerTravelDistance = playerMoveVector.magnitude;
        _currentHookRatio = 1f;
    }

    private void HandleGrappleMovement()
    {
        if (!IsGrappling) return;

        _rb.linearVelocity = _hookDirection * _hookSpeed;
    }

    private void EndHook(bool applyInertia)
    {
        if (!IsGrappling && !IsHookPreparing) return;

        if (_hookPrepareCoroutine != null)
        {
            StopCoroutine(_hookPrepareCoroutine);
            _hookPrepareCoroutine = null;
        }

        IsHookPreparing = false;
        IsGrappling = false;
        _hookTimer = 0f;

        _rb.gravityScale = _originalGravity;

        if (_hookTrail != null)
        {
            _hookTrail.emitting = false;
        }

        ClearActiveHookPoint();

        if (_activeEnemyHookReceiver != null)
        {
            _activeEnemyHookReceiver.ForceEndPull();
            _activeEnemyHookReceiver = null;
        }

        if (_activeProjectileHook != null)
        {
            _activeProjectileHook.ForceEndProjectileHookMovement();
            _activeProjectileHook = null;
        }

        if (applyInertia)
        {
            _rb.linearVelocity = _hookDirection * (_hookInertia * _currentHookRatio);
            StartHookInertia();
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
            ClearHookInertia();
        }

        if (_playerDash != null)
        {
            _playerDash.RechargeDash();
        }

        if (!_playerState.IsDead && !_playerState.IsHit)
        {
            _playerState.TryChangeState(PlayerState.Normal);
        }

        _currentHookRatio = 1f;
    }

    private void ClearActiveHookPoint()
    {
        _activeHookFollowTarget = null;
        _activeHookPoint = Vector2.zero;
        _activeHookLocalPoint = Vector3.zero;
    }

    private void StartHookInertia()
    {
        _hookInertiaTimer = _hookInertiaDuration;
    }

    private void ClearHookInertia()
    {
        _hookInertiaTimer = 0f;
    }

    public void CancelHookByDamage()
    {
        ClearQueuedHook();
        EndHook(false);
    }

    public void RechargeHook()
    {
        CurrentHookCount = _maxHookCount;
    }

    public void SetBonusMaxHookCount(int bonusCount)
    {
        _bonusMaxHookCount = Mathf.Max(0, bonusCount);
        RefreshMaxHookCount(false);
    }

    public void SetHookSpeedBonusPercent(float bonusPercent)
    {
        _hookSpeedBonusPercent = Mathf.Max(0f, bonusPercent);
    }

    private void RefreshMaxHookCount(bool refill)
    {
        int previousMaxCount = _maxHookCount;

        _maxHookCount = Mathf.Max(1, _baseMaxHookCount + _bonusMaxHookCount);

        if (refill)
        {
            CurrentHookCount = _maxHookCount;
            return;
        }

        int maxCountDifference = _maxHookCount - previousMaxCount;

        if (maxCountDifference > 0)
        {
            CurrentHookCount += maxCountDifference;
        }

        CurrentHookCount = Mathf.Clamp(CurrentHookCount, 0, _maxHookCount);
    }
}