using UnityEngine;

public class EnemyHookReceiver : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private EnemyData _enemyData;
    [SerializeField] private EnemyStateController _enemyState;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private DamageDealer _contactDamageDealer;
    [SerializeField] private Collider2D _contactAttackTrigger;

    public bool IsBeingPulled { get; private set; }

    private float _pullTimer;
    private Vector2 _pullVelocity;

    private void Awake()
    {
        if (_enemyData == null)
            _enemyData = GetComponent<EnemyData>();

        if (_enemyState == null)
            _enemyState = GetComponent<EnemyStateController>();

        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        if (_contactDamageDealer == null)
            _contactDamageDealer = GetComponentInChildren<DamageDealer>(true);

        if (_contactAttackTrigger == null && _contactDamageDealer != null)
            _contactAttackTrigger = _contactDamageDealer.GetComponent<Collider2D>();

        if (_rb != null && _enemyData != null)
            _rb.gravityScale = _enemyData.DefaultGravityScale;
    }

    private void Update()
    {
        UpdatePullTimer();
    }

    private void FixedUpdate()
    {
        HandlePullMovement();
    }

    public bool CanBeHooked()
    {
        if (_enemyData == null || _enemyState == null)
            return false;

        if (_enemyState.IsDead)
            return false;

        switch (_enemyData.AttackReaction)
        {
            case AttackReactionType.Normal:
                return true;

            case AttackReactionType.Armored:
                return !_enemyState.HasArmor;

            case AttackReactionType.WeakPointOnly:
                return _enemyState.IsWeakPointOpen;
        }

        return false;
    }

    public bool TryStartPull(
        Vector2 hookDirection,
        float enemyPullDistance,
        float hookDuration)
    {
        if (IsBeingPulled)
            return false;

        if (!CanBeHooked())
            return false;

        if (_enemyData == null || _enemyState == null || _rb == null)
            return false;

        if (_enemyData.PullRate <= 0f)
            return true;

        IsBeingPulled = true;
        _pullTimer = hookDuration;
        _enemyState.StartPulled();

        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero;

        SetContactAttackEnabled(false);

        if (hookDuration <= 0f)
        {
            _pullVelocity = Vector2.zero;
            return true;
        }

        Vector2 enemyMoveVector = -hookDirection * enemyPullDistance;
        _pullVelocity = enemyMoveVector / hookDuration;

        return true;
    }

    private void UpdatePullTimer()
    {
        if (!IsBeingPulled)
            return;

        _pullTimer -= Time.deltaTime;

        if (_pullTimer <= 0f)
        {
            EndPulled();
        }
    }

    private void HandlePullMovement()
    {
        if (!IsBeingPulled)
            return;

        _rb.linearVelocity = _pullVelocity;
    }

    public void ForceEndPull()
    {
        EndPulled();
    }

    private void EndPulled()
    {
        if (!IsBeingPulled)
            return;

        IsBeingPulled = false;
        _pullTimer = 0f;
        _pullVelocity = Vector2.zero;

        _rb.linearVelocity = Vector2.zero;

        if (_enemyData != null)
            _rb.gravityScale = _enemyData.DefaultGravityScale;

        if (_enemyState != null)
            _enemyState.EndPulled();

        SetContactAttackEnabled(true);
    }

    private void SetContactAttackEnabled(bool enabled)
    {
        if (_contactDamageDealer != null)
            _contactDamageDealer.enabled = enabled;

        if (_contactAttackTrigger != null)
            _contactAttackTrigger.enabled = enabled;
    }
}