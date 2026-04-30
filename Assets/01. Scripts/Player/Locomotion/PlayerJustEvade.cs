using UnityEngine;
using System.Collections;

public class PlayerJustEvade : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private PlayerStateController _playerState;
    [SerializeField] private PlayerInputHandler _playerInput;
    [SerializeField] private PlayerDash _playerDash;
    [SerializeField] private Camera _mainCamera;

    [Header("Just Evade Settings")]
    [SerializeField] private float _bulletTimeScale = 0.1f;
    [SerializeField] private float _bulletTimeDuration = 1f;
    [SerializeField] private float _chargeDistance = 10f;
    [SerializeField] private float _chargeActiveTime = 2f;
    [SerializeField] private float _chargeInertia = 10f;

    private Vector2 _chargeDirection = Vector2.up;
    private float _chargeSpeed;
    private float _chargeTimer;
    private float _originalGravity;
    private float _originalTimeScale = 1f;
    private float _originalFixedDeltaTime = 0.02f;


    private Coroutine _bulletTimeCoroutine;


    private void Awake()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
        }
        if (_playerState == null)
        {
            _playerState = GetComponent<PlayerStateController>();
        }
        if (_playerInput == null)
        {
            _playerInput = GetComponent<PlayerInputHandler>();
        }
        if (_playerDash == null)
        {
            _playerDash = GetComponent<PlayerDash>();
        }
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        _chargeSpeed = _chargeDistance / _chargeActiveTime;
    }

    private void Update()
    {
        HandleChargeInput();
        HandleChargeTimer();
    }

    private void FixedUpdate()
    {
        Charge();
    }


    public void StartJustEvade()
    {
        if (_playerDash != null)
        {
            _playerDash.CancelDash();
        }

        if (_playerState != null)
        {
            _playerState.TryChangeState(PlayerState.Evade);
        }

        StartBulletTime();
    }

    private void StartBulletTime()
    {
        if (_bulletTimeCoroutine != null)
        {
            StopCoroutine(_bulletTimeCoroutine);
        }

        _bulletTimeCoroutine = StartCoroutine(BulletTimeRoutine());
    }

    private IEnumerator BulletTimeRoutine()
    {
        _originalTimeScale = Time.timeScale;
        _originalFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = _bulletTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(_bulletTimeDuration);

        RestoreTimeScale();

        if (_playerState.IsEvading)
        {
            _playerState.TryChangeState(PlayerState.Normal);
        }

        _bulletTimeCoroutine = null;
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = _originalTimeScale;
        Time.fixedDeltaTime = _originalFixedDeltaTime;
    }


    private void HandleChargeInput()
    {
        if (!_playerState.IsEvading) return;

        if (!_playerInput.AttackPressed && !_playerInput.JumpPressed) return;

        ChargeSetup();
    }

    private void HandleChargeTimer()
    {
        if (!_playerState.IsCharging) return;

        _chargeTimer -= Time.deltaTime;

        if (_chargeTimer <= 0f)
        {
            EndCharge();
        }
    }

    private void HandleChargeDirection()
    {
        Vector2 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(_playerInput.MousePosition);
        Vector2 playerPosition = transform.position;
        _chargeDirection = (mouseWorldPosition - playerPosition).normalized;
    }

    private void ChargeSetup()
    {
        if (_playerState == null) return;
        if (!_playerState.CanCharge()) return;
        if (!_playerState.TryChangeState(PlayerState.Charging)) return;

        if (_bulletTimeCoroutine != null)
        {
            StopCoroutine(_bulletTimeCoroutine);
            _bulletTimeCoroutine = null;
            RestoreTimeScale();
        }

        _chargeTimer = _chargeActiveTime;
        _originalGravity = _rb.gravityScale;

        HandleChargeDirection();
        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero;
    }

    private void Charge()
    {
        if (!_playerState.IsCharging) return;
        _rb.linearVelocity = _chargeDirection * _chargeSpeed;
    }

    private void EndCharge()
    {
        RestoreTimeScale();

        _playerState.TryChangeState(PlayerState.Normal);
        _chargeTimer = 0f;

        _rb.gravityScale = _originalGravity; 
        _rb.linearVelocity = _chargeDirection * _chargeInertia;
    }
}
