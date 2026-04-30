using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneFlowManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string _gameSceneName = "GameScene";

    [Header("Transition")]
    [SerializeField] private CanvasGroup _transitionCanvasGroup;
    [SerializeField] private float _fadeDuration = 0.6f;

    [Header("Player")]
    [SerializeField] private PlayerControlLocker _playerControlLocker;

    private bool _isTransitioning;

    private void Awake()
    {
        if (_transitionCanvasGroup != null)
        {
            _transitionCanvasGroup.alpha = 0f;
            _transitionCanvasGroup.gameObject.SetActive(false);
        }
    }

    public void EnterGameScene()
    {
        if (_isTransitioning) return;

        StartCoroutine(EnterGameSceneRoutine());
    }

    private IEnumerator EnterGameSceneRoutine()
    {
        _isTransitioning = true;

        if (_playerControlLocker != null)
        {
            _playerControlLocker.LockControls();
        }

        if (_transitionCanvasGroup != null)
        {
            _transitionCanvasGroup.gameObject.SetActive(true);

            float timer = 0f;

            while (timer < _fadeDuration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / _fadeDuration);
                _transitionCanvasGroup.alpha = t;

                yield return null;
            }

            _transitionCanvasGroup.alpha = 1f;
        }

        RunDataManager runDataManager = FindAnyObjectByType<RunDataManager>();

        if (runDataManager != null)
        {
            runDataManager.StartNewRun();
        }

        SceneManager.LoadScene(_gameSceneName);
    }
}