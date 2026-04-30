using System.Collections;
using UnityEngine;

public class PlayerDamageReceiver : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private PlayerHealth _health;
    [SerializeField] private PlayerStateController _playerState;
    [SerializeField] private PlayerDash _playerDash;
    [SerializeField] private PlayerJustEvade _playerJustEvade;
    [SerializeField] private PlayerHook _playerHook;
    [SerializeField] private PlayerSlam _playerSlam;
    [SerializeField] private PlayerBlink _playerBlink;

    [Header("Feedback")]
    [SerializeField] private GameFeedbackManager _feedback;

    [Header("Damage Receive Settings")]
    [SerializeField] private float _invincibleDuration = 0.5f;
    [SerializeField] private float _hitDuration = 0.3f;
    [SerializeField] private float _knockBackForce = 5f;

    private bool _isInvincible;
    private float _baseInvincibleDuration;

    private Coroutine _hitCoroutine;
    private Coroutine _invincibleCoroutine;

    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        if (_health == null)
        {
            _health = GetComponent<PlayerHealth>();
        }

        if (_playerState == null)
        {
            _playerState = GetComponent<PlayerStateController>();
        }

        if (_playerDash == null)
        {
            _playerDash = GetComponent<PlayerDash>();
        }

        if (_playerJustEvade == null)
        {
            _playerJustEvade = GetComponent<PlayerJustEvade>();
        }

        if (_playerHook == null)
        {
            _playerHook = GetComponent<PlayerHook>();
        }

        if (_playerSlam == null)
        {
            _playerSlam = GetComponent<PlayerSlam>();
        }

        if (_playerBlink == null)
        {
            _playerBlink = GetComponent<PlayerBlink>();
        }

        if (_feedback == null)
        {
            _feedback = FindFirstObjectByType<GameFeedbackManager>();
        }

        _baseInvincibleDuration = _invincibleDuration;
    }

    public bool ReceiveDamage(int damage, Vector2 hitDirection, DamageDealingType damageType)
    {
        Debug.Log($"ReceiveDamage Called / damageType: {damageType} / object: {name}");

        if (damage <= 0) return false;
        if (_health == null || _playerState == null) return false;
        if (_playerState.IsDead) return false;
        if (_isInvincible) return false;

        if (TryJustEvade(damageType))
        {
            StartInvincibility();
            return false;
        }

        if (_playerSlam != null && _playerState.IsSlamming)
        {
            _playerSlam.CancelSlam();
        }

        CancelActiveMovementStates();

        _health.TakeDamage(damage);

        _feedback?.PlayPlayerDamageFeedback();

        if (_playerBlink != null)
        {
            _playerBlink.StartBlink(_invincibleDuration);
        }

        if (_playerState.IsDead)
        {
            return true;
        }

        ApplyKnockBack(hitDirection);
        EnterHitState();
        StartInvincibility();

        return true;
    }

    private void CancelActiveMovementStates()
    {
        if (_playerHook != null && (_playerHook.IsGrappling || _playerHook.IsHookPreparing))
        {
            _playerHook.CancelHookByDamage();
        }

        if (_playerDash != null && _playerDash.IsDashing)
        {
            _playerDash.CancelDash();
        }
    }

    private bool TryJustEvade(DamageDealingType damageType)
    {
        if (_playerDash == null) return false;
        if (_playerState == null) return false;
        if (_playerJustEvade == null) return false;
        if (!_playerDash.IsJustEvadeWindowActive) return false;
        if (!_playerState.CanEvade()) return false;
        if (!CanJustEvadeDamageType(damageType)) return false;

        _playerJustEvade.StartJustEvade();
        return true;
    }

    private bool CanJustEvadeDamageType(DamageDealingType damageType)
    {
        return damageType == DamageDealingType.Projectile;
    }

    private void ApplyKnockBack(Vector2 hitDirection)
    {
        if (_rb == null) return;

        if (hitDirection == Vector2.zero)
        {
            hitDirection = Vector2.up;
        }

        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(hitDirection.normalized * _knockBackForce, ForceMode2D.Impulse);
    }

    private void EnterHitState()
    {
        if (_hitCoroutine != null)
        {
            StopCoroutine(_hitCoroutine);
        }

        _hitCoroutine = StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        _playerState.TryChangeState(PlayerState.Hit);

        yield return new WaitForSeconds(_hitDuration);

        if (!_playerState.IsDead)
        {
            _playerState.TryChangeState(PlayerState.Normal);
        }

        _hitCoroutine = null;
    }

    private void StartInvincibility()
    {
        if (_invincibleCoroutine != null)
        {
            StopCoroutine(_invincibleCoroutine);
        }

        _invincibleCoroutine = StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        _isInvincible = true;

        yield return new WaitForSeconds(_invincibleDuration);

        _isInvincible = false;
        _invincibleCoroutine = null;
    }

    public void SetInvincibleBonusDuration(float bonusDuration)
    {
        _invincibleDuration = _baseInvincibleDuration + Mathf.Max(0f, bonusDuration);
    }
}