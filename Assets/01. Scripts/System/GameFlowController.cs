using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string _resultSceneName = "ResultScene";

    [Header("Player")]
    [SerializeField] private Transform _player;
    [SerializeField] private PlayerHealth _playerHealth;

    [Header("Debug")]
    [SerializeField] private bool _enableDebugKeys = true;

    private RunDataManager _runDataManager;
    private bool _isGameEnded;

    private void Awake()
    {
        _runDataManager = FindAnyObjectByType<RunDataManager>();

        if (_player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                _player = playerObject.transform;
            }
        }

        if (_playerHealth == null && _player != null)
        {
            _playerHealth = _player.GetComponent<PlayerHealth>();
        }
    }

    private void OnEnable()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnDied += HandlePlayerDied;
        }
    }

    private void OnDisable()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnDied -= HandlePlayerDied;
        }
    }

    private void Start()
    {
        if (_runDataManager != null && !_runDataManager.IsRunActive)
        {
            _runDataManager.StartNewRun();
        }
    }

    private void Update()
    {
        UpdateReachedHeight();

        if (_enableDebugKeys)
        {
            HandleDebugKeys();
        }
    }

    private void UpdateReachedHeight()
    {
        if (_runDataManager == null) return;
        if (_player == null) return;

        _runDataManager.SetReachedHeight(_player.position.y);
    }

    private void HandleDebugKeys()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            EndGame(false);
        }

        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            EndGame(true);
        }
    }

    private void HandlePlayerDied()
    {
        Debug.Log("Player Died. ResultScene으로 이동.");
        EndGame(false);
    }

    public void EndGame(bool isCleared)
    {
        if (_isGameEnded) return;

        _isGameEnded = true;

        Debug.Log($"Game End. Cleared: {isCleared}");

        if (_runDataManager != null)
        {
            _runDataManager.FinishRun(isCleared);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(_resultSceneName);
    }
}