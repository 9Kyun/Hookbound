using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartSceneIntroController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup _titleUICanvasGroup;
    [SerializeField] private Button _startButton;

    [Header("Camera")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _playerTarget;
    [SerializeField] private Vector3 _cameraTargetOffset = new Vector3(0f, 1f, -10f);

    [Header("Camera Zoom")]
    [SerializeField] private float _introDuration = 1.2f;
    [SerializeField] private float _targetOrthographicSize = 5f;
    [SerializeField] private float _targetFieldOfView = 40f;
    [SerializeField] private AnimationCurve _zoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Player")]
    [SerializeField] private PlayerControlLocker _playerControlLocker;

    [Header("Camera Follow Scripts")]
    [SerializeField] private MonoBehaviour[] _cameraFollowScripts;

    private bool _isIntroPlaying;
    private bool _isIntroFinished;

    private Vector3 _cameraStartPosition;
    private float _startOrthographicSize;
    private float _startFieldOfView;

    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_startButton != null)
        {
            _startButton.onClick.AddListener(StartIntro);
        }

        LockPlayer();
        SetCameraFollowScriptsEnabled(false);
    }

    private void Start()
    {
        if (_mainCamera == null) return;

        _cameraStartPosition = _mainCamera.transform.position;
        _startOrthographicSize = _mainCamera.orthographicSize;
        _startFieldOfView = _mainCamera.fieldOfView;
    }

    private void OnDestroy()
    {
        if (_startButton != null)
        {
            _startButton.onClick.RemoveListener(StartIntro);
        }
    }

    public void StartIntro()
    {
        if (_isIntroPlaying || _isIntroFinished) return;

        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        _isIntroPlaying = true;

        if (_titleUICanvasGroup != null)
        {
            _titleUICanvasGroup.interactable = false;
            _titleUICanvasGroup.blocksRaycasts = false;
        }

        float timer = 0f;

        Vector3 targetCameraPosition = GetTargetCameraPosition();

        while (timer < _introDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / _introDuration);
            float curvedT = _zoomCurve.Evaluate(t);

            UpdateTitleUIFade(curvedT);
            UpdateCameraZoom(targetCameraPosition, curvedT);

            yield return null;
        }

        UpdateTitleUIFade(1f);
        UpdateCameraZoom(targetCameraPosition, 1f);

        if (_titleUICanvasGroup != null)
        {
            _titleUICanvasGroup.gameObject.SetActive(false);
        }

        UnlockPlayer();
        SetCameraFollowScriptsEnabled(true);

        _isIntroPlaying = false;
        _isIntroFinished = true;
    }

    private Vector3 GetTargetCameraPosition()
    {
        if (_playerTarget == null || _mainCamera == null)
        {
            return _cameraStartPosition;
        }

        return _playerTarget.position + _cameraTargetOffset;
    }

    private void UpdateTitleUIFade(float t)
    {
        if (_titleUICanvasGroup == null) return;

        _titleUICanvasGroup.alpha = 1f - t;
    }

    private void UpdateCameraZoom(Vector3 targetPosition, float t)
    {
        if (_mainCamera == null) return;

        _mainCamera.transform.position = Vector3.Lerp(_cameraStartPosition, targetPosition, t);

        if (_mainCamera.orthographic)
        {
            _mainCamera.orthographicSize = Mathf.Lerp(
                _startOrthographicSize,
                _targetOrthographicSize,
                t
            );
        }
        else
        {
            _mainCamera.fieldOfView = Mathf.Lerp(
                _startFieldOfView,
                _targetFieldOfView,
                t
            );
        }
    }

    private void LockPlayer()
    {
        if (_playerControlLocker != null)
        {
            _playerControlLocker.LockControls();
        }
    }

    private void UnlockPlayer()
    {
        if (_playerControlLocker != null)
        {
            _playerControlLocker.UnlockControls();
        }
    }

    private void SetCameraFollowScriptsEnabled(bool enabled)
    {
        if (_cameraFollowScripts == null) return;

        for (int i = 0; i < _cameraFollowScripts.Length; i++)
        {
            if (_cameraFollowScripts[i] == null) continue;

            _cameraFollowScripts[i].enabled = enabled;
        }
    }
}