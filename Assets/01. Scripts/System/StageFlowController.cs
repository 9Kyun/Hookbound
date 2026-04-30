using UnityEngine;
using UnityEngine.SceneManagement;

public class StageFlowController : MonoBehaviour
{
    [Header("Stage Settings")]
    [SerializeField] private int _maxSubStage = 3;

    [Header("Components")]
    [SerializeField] private GameFlowController _gameFlowController;

    private RunDataManager _runDataManager;
    private bool _isChangingStage;

    private void Awake()
    {
        _runDataManager = FindAnyObjectByType<RunDataManager>();

        if (_gameFlowController == null)
        {
            _gameFlowController = FindAnyObjectByType<GameFlowController>();
        }
    }

    public void OnGoalReached()
    {
        if (_isChangingStage) return;

        _isChangingStage = true;

        if (_runDataManager == null)
        {
            _runDataManager = FindAnyObjectByType<RunDataManager>();
        }

        if (_gameFlowController == null)
        {
            _gameFlowController = FindAnyObjectByType<GameFlowController>();
        }

        int currentStage = _runDataManager != null
            ? _runDataManager.CurrentStage
            : 1;

        Debug.Log($"Goal Reached. Current Stage: 1-{currentStage}");

        if (currentStage >= _maxSubStage)
        {
            Debug.Log("마지막 스테이지 클리어. ResultScene으로 이동.");

            if (_gameFlowController != null)
            {
                _gameFlowController.EndGame(true);
            }
            else
            {
                Debug.LogError("GameFlowController를 찾지 못했습니다.");
            }

            return;
        }

        if (_runDataManager != null)
        {
            _runDataManager.AdvanceStage();
            Debug.Log($"다음 스테이지로 이동: 1-{_runDataManager.CurrentStage}");
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}