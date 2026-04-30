using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Collider2D _projectileCollider;
    [SerializeField] private DamageDealer _damageDealer;
    [SerializeField] private HookableTarget _hookableTarget;

    [Header("Projectile Settings")]
    [SerializeField] private float _speed = 12f;
    [SerializeField] private float _lifeTime = 3f;
    [SerializeField] private LayerMask _environmentLayer;

    [Header("Parry Settings")]
    [SerializeField] private int _parryDamage = 1;
    [SerializeField] private bool _destroyOnParryHit = true;

    public bool IsReflected { get; private set; }
    public bool IsProjectileHookMoving { get; private set; }

    private Vector2 _moveDirection = Vector2.right;
    private float _lifeTimer;
    private float _projectileHookTimer;
    private float _projectileHookSpeed;

    private readonly HashSet<EnemyHitBox> _hitTargets = new HashSet<EnemyHitBox>();

    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }
        if (_projectileCollider == null)
        {
            _projectileCollider = GetComponent<Collider2D>();
        }
        if (_damageDealer == null)
        {
            _damageDealer = GetComponentInChildren<DamageDealer>(true);
        }
        if (_hookableTarget == null)
        {
            _hookableTarget = GetComponentInChildren<HookableTarget>(true);
        }
    }

    private void OnEnable()
    {
        _lifeTimer = _lifeTime;

        _projectileHookTimer = 0f;
        _projectileHookSpeed = 0f;
        IsReflected = false;
        IsProjectileHookMoving = false;
        _hitTargets.Clear();

        SetHostileDamageEnabled(true);

        if (_hookableTarget != null)
        {
            _hookableTarget.SetCanBeHooked(false);
        }
    }

    private void Update()
    {
        UpdateLifeTimer();
        UpdateProjectileHookTimer();
    }

    private void FixedUpdate()
    {
        MoveProjectile();
    }

    public void Fire(Vector2 direction, float speed)
    {
        _moveDirection = direction.normalized;

        if (_moveDirection == Vector2.zero)
        {
            _moveDirection = Vector2.right;
        }

        _speed = speed;
        RotateToDirection();
    }

    public bool TryParry(Vector2 direction)
    {
        if (IsReflected)
            return false;

        _moveDirection = direction.normalized;

        if (_moveDirection==Vector2.zero)
        {
            _moveDirection = Vector2.right;
        }

        IsReflected = true;
        IsProjectileHookMoving = false;
        _projectileHookTimer = 0f;
        _projectileHookSpeed = 0f;
        _hitTargets.Clear();

        SetHostileDamageEnabled(false);

        if (_hookableTarget != null)
        {
            _hookableTarget.SetCanBeHooked(true);
        }

        RotateToDirection();
        
        if (_rb != null)
        {
            _rb.linearVelocity = _moveDirection * _speed;
        }

        return true;
    }

    public bool TryStartProjectileHook(Vector2 playerPosition, float hookDuration, float crossDistance)
    {
        if (!IsReflected) return false;
        if (_rb == null) return false;
        if (hookDuration <= 0f) return false;

        Vector2 projectilePosition = _rb.position;
        Vector2 toPlayer = playerPosition - projectilePosition;
        float distance = toPlayer.magnitude;

        if (distance <= 0.0001f)
        {
            return false;
        }

        _moveDirection = toPlayer.normalized;
        _projectileHookSpeed = (distance * 0.5f + crossDistance) / hookDuration;
        _projectileHookTimer = hookDuration;
        IsProjectileHookMoving = true;

        RotateToDirection();
        return true;
    }

    public void ForceEndProjectileHookMovement()
    {
        if (!IsReflected) return;

        IsProjectileHookMoving = false;
        _projectileHookTimer = 0f;
        _projectileHookSpeed = 0f;

        if (_rb != null)
        {
            _rb.linearVelocity = _moveDirection * _speed;
        }
    }

    private void MoveProjectile()
    {
        if (_rb == null) return;

        if (IsProjectileHookMoving)
        {
            _rb.linearVelocity = _moveDirection * _projectileHookSpeed;
            return;
        }

        _rb.linearVelocity = _moveDirection * _speed;
    }

    private void UpdateLifeTimer()
    {
        _lifeTimer -= Time.deltaTime;

        if (_lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateProjectileHookTimer()
    {
        if (!IsProjectileHookMoving) return;

        _projectileHookTimer -= Time.deltaTime;

        if (_projectileHookTimer <= 0f)
        {
            ForceEndProjectileHookMovement();
        }
    }

    private void RotateToDirection()
    {
        float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int targetLayerMask = 1 << collision.gameObject.layer;

        if ((_environmentLayer.value & targetLayerMask) != 0)
        {
            Destroy(gameObject);
            return;
        }

        if (!IsReflected) return;

        PlayerHitBox playerHitBox = collision.GetComponent<PlayerHitBox>();

        if (playerHitBox == null)
        {
            playerHitBox = collision.GetComponentInParent<PlayerHitBox>();
        }

        if (playerHitBox != null)
        {
            return;
        }

        EnemyHitBox enemyHitBox = collision.GetComponent<EnemyHitBox>();

        if (enemyHitBox == null)
        {
            enemyHitBox = collision.GetComponentInParent<EnemyHitBox>();
        }

        if (enemyHitBox == null) return;
        if (_hitTargets.Contains(enemyHitBox)) return;

        EnemyHitResult hitResult = enemyHitBox.ReceiveHit(PlayerAttackType.ParryProjectile, _parryDamage);

        if (hitResult == EnemyHitResult.None) return;

        _hitTargets.Add(enemyHitBox);

        if (_destroyOnParryHit)
        {
            Destroy(gameObject);
        }
    }

    private void SetHostileDamageEnabled(bool enabled)
    {
        if (_damageDealer != null)
        {
            _damageDealer.enabled = enabled;
        }
    }
}