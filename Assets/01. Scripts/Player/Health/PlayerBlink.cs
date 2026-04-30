using System.Collections;
using UnityEngine;

public class PlayerBlink : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] _spriteRenderers;
    [SerializeField] private float _blinkInterval = 0.06f;

    private Coroutine _blinkCoroutine;

    private void Awake()
    {
        if (_spriteRenderers == null || _spriteRenderers.Length == 0)
        {
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    public void StartBlink(float duration)
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
        }

        _blinkCoroutine = StartCoroutine(BlinkRoutine(duration));
    }

    private IEnumerator BlinkRoutine(float duration)
    {
        float timer = 0f;
        bool visible = true;

        while (timer < duration)
        {
            visible = !visible;
            SetVisible(visible);

            timer += _blinkInterval;
            yield return new WaitForSecondsRealtime(_blinkInterval);
        }

        SetVisible(true);
        _blinkCoroutine = null;
    }

    private void SetVisible(bool visible)
    {
        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = visible;
            }
        }
    }
}