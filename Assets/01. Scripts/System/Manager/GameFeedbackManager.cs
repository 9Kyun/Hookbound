using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameFeedbackManager : MonoBehaviour
{
    public static GameFeedbackManager Instance { get; private set; }

    [Header("Camera Shake")]
    [SerializeField] private Transform _shakeTarget;
    [SerializeField] private float _attackShakeDuration = 0.08f;
    [SerializeField] private float _attackShakePower = 0.08f;
    [SerializeField] private float _slamShakeDuration = 0.12f;
    [SerializeField] private float _slamShakePower = 0.14f;
    [SerializeField] private float _damageShakeDuration = 0.1f;
    [SerializeField] private float _damageShakePower = 0.12f;

    [Header("Hit Stop")]
    [SerializeField] private float _attackHitStopDuration = 0.04f;
    [SerializeField] private float _killHitStopDuration = 0.06f;
    [SerializeField] private float _slamHitStopDuration = 0.06f;

    [Header("Damage Flash")]
    [SerializeField] private Image _damageFlashImage;
    [SerializeField] private float _damageFlashInTime = 0.02f;
    [SerializeField] private float _damageFlashHoldTime = 0.03f;
    [SerializeField] private float _damageFlashOutTime = 0.12f;
    [SerializeField, Range(0f, 1f)] private float _damageFlashAlpha = 0.85f;

    private Coroutine _hitStopCoroutine;
    private Coroutine _shakeCoroutine;
    private Coroutine _flashCoroutine;

    private bool _isHitStopped;
    private float _savedTimeScale;
    private float _savedFixedDeltaTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetFlashAlpha(0f);
    }

    public void PlayAttackHitFeedback(bool isKill)
    {
        StartHitStop(isKill ? _killHitStopDuration : _attackHitStopDuration);
        StartShake(_attackShakeDuration, _attackShakePower);
    }

    public void PlaySlamHitFeedback()
    {
        StartHitStop(_slamHitStopDuration);
        StartShake(_slamShakeDuration, _slamShakePower);
    }

    public void PlayPlayerDamageFeedback()
    {
        StartShake(_damageShakeDuration, _damageShakePower);
        StartDamageFlash();
    }

    private void StartHitStop(float duration)
    {
        if (_hitStopCoroutine != null)
        {
            StopCoroutine(_hitStopCoroutine);
            RestoreHitStopTime();
        }

        _hitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        _isHitStopped = true;
        _savedTimeScale = Time.timeScale;
        _savedFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        yield return new WaitForSecondsRealtime(duration);

        RestoreHitStopTime();
        _hitStopCoroutine = null;
    }

    private void RestoreHitStopTime()
    {
        if (!_isHitStopped) return;

        Time.timeScale = _savedTimeScale <= 0f ? 1f : _savedTimeScale;
        Time.fixedDeltaTime = _savedFixedDeltaTime <= 0f ? 0.02f : _savedFixedDeltaTime;

        _isHitStopped = false;
    }

    private void StartShake(float duration, float power)
    {
        if (_shakeTarget == null) return;

        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _shakeTarget.localPosition = Vector3.zero;
        }

        _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, power));
    }

    private IEnumerator ShakeRoutine(float duration, float power)
    {
        float timer = duration;

        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;

            Vector2 randomOffset = Random.insideUnitCircle * power;
            _shakeTarget.localPosition = new Vector3(randomOffset.x, randomOffset.y, 0f);

            yield return null;
        }

        _shakeTarget.localPosition = Vector3.zero;
        _shakeCoroutine = null;
    }

    private void StartDamageFlash()
    {
        if (_damageFlashImage == null) return;

        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }

        _flashCoroutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        yield return FadeFlash(0f, _damageFlashAlpha, _damageFlashInTime);

        SetFlashAlpha(_damageFlashAlpha);
        yield return new WaitForSecondsRealtime(_damageFlashHoldTime);

        yield return FadeFlash(_damageFlashAlpha, 0f, _damageFlashOutTime);

        SetFlashAlpha(0f);
        _flashCoroutine = null;
    }

    private IEnumerator FadeFlash(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            SetFlashAlpha(to);
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);

            SetFlashAlpha(Mathf.Lerp(from, to, t));

            yield return null;
        }

        SetFlashAlpha(to);
    }

    private void SetFlashAlpha(float alpha)
    {
        if (_damageFlashImage == null) return;

        Color color = _damageFlashImage.color;
        color.a = alpha;
        _damageFlashImage.color = color;
    }

    public void SetSceneReferences(Transform shakeTarget, Image damageFlashImage)
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = null;
        }

        if (_shakeTarget != null)
        {
            _shakeTarget.localPosition = Vector3.zero;
        }

        _shakeTarget = shakeTarget;
        _damageFlashImage = damageFlashImage;

        SetFlashAlpha(0f);
    }
}