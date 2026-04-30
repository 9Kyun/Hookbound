using UnityEngine;

public class TurretEnemyAI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private EnemyStateController _enemyState;
    [SerializeField] private Transform _firePoint;

    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Attack Settings")]
    [SerializeField] private EnemyProjectile _projectilePrefab;
    [SerializeField] private float _detectRange = 10f;
    [SerializeField] private float _attackCooldown = 1.2f;
    [SerializeField] private float _projectileSpeed = 12f;

    private float _attackTimer;

    private void Awake()
    {
        if (_enemyState == null)
        {
            _enemyState = GetComponent<EnemyStateController>();
        }

        if (_target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _target = player.transform;
            }
        }

        _attackTimer = 0f;
    }

    private void Update()
    {
        if (!CanRunAI())
            return;

        UpdateAttackTimer();

        if (!HasTarget())
            return;

        if (!IsTargetInDetectRange())
            return;

        if (_attackTimer > 0f)
            return;

        FireProjectile();
        _attackTimer = _attackCooldown;
    }

    private bool CanRunAI()
    {
        if (_enemyState == null) return false;
        if (_enemyState.IsDead) return false;
        if (_enemyState.IsPulled) return false;
        if (_enemyState.IsStunned) return false;

        return true;
    }

    private void UpdateAttackTimer()
    {
        if (_attackTimer > 0f)
        {
            _attackTimer -= Time.deltaTime;
        }
    }

    private bool HasTarget()
    {
        return _target != null;
    }

    private bool IsTargetInDetectRange()
    {
        float distance = Vector2.Distance(transform.position, _target.position);
        return distance <= _detectRange;
    }

    private void FireProjectile()
    {
        if (_projectilePrefab == null) return;
        if (_firePoint == null) return;
        if (_target == null) return;

        Vector2 fireDirection = (_target.position - _firePoint.position).normalized;

        EnemyProjectile spawnedProjectile = Instantiate(
            _projectilePrefab,
            _firePoint.position,
            Quaternion.identity
        );

        spawnedProjectile.Fire(fireDirection, _projectileSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _detectRange);

        if (_firePoint != null && _target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_firePoint.position, _target.position);
        }
    }
}