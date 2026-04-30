using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePauseController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _pauseMenuRoot;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _quitButton;

    [Header("Scene")]
    [SerializeField] private string _quitSceneName = "StartScene";

    private bool _isPaused;

    private void Awake()
    {
        if (_pauseMenuRoot != null)
        {
            _pauseMenuRoot.SetActive(false);
        }

        if (_continueButton != null)
        {
            _continueButton.onClick.AddListener(Resume);
        }

        if (_retryButton != null)
        {
            _retryButton.onClick.AddListener(RetryCurrentScene);
        }

        if (_quitButton != null)
        {
            _quitButton.onClick.AddListener(QuitToScene);
        }
    }

    private void OnDestroy()
    {
        if (_continueButton != null)
        {
            _continueButton.onClick.RemoveListener(Resume);
        }

        if (_retryButton != null)
        {
            _retryButton.onClick.RemoveListener(RetryCurrentScene);
        }

        if (_quitButton != null)
        {
            _quitButton.onClick.RemoveListener(QuitToScene);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (_isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        if (_isPaused) return;

        _isPaused = true;
        Time.timeScale = 0f;

        if (_pauseMenuRoot != null)
        {
            _pauseMenuRoot.SetActive(true);
        }
    }

    public void Resume()
    {
        if (!_isPaused) return;

        _isPaused = false;
        Time.timeScale = 1f;

        if (_pauseMenuRoot != null)
        {
            _pauseMenuRoot.SetActive(false);
        }
    }

    public void RetryCurrentScene()
    {
        Time.timeScale = 1f;

        RunDataManager runDataManager = FindAnyObjectByType<RunDataManager>();

        if (runDataManager != null)
        {
            runDataManager.StartNewRun();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToScene()
    {
        Time.timeScale = 1f;

        RunDataManager runDataManager = FindAnyObjectByType<RunDataManager>();

        if (runDataManager != null)
        {
            runDataManager.ClearRunData();
        }

        SceneManager.LoadScene(_quitSceneName);
    }
}