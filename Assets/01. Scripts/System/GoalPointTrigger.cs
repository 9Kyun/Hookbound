using UnityEngine;

public class GoalPointTrigger : MonoBehaviour
{
    [SerializeField] private StageFlowController _stageFlowController;

    private bool _isUsed;

    private void Awake()
    {
        if (_stageFlowController == null)
        {
            _stageFlowController = FindAnyObjectByType<StageFlowController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isUsed) return;

        PlayerStateController player = collision.GetComponentInParent<PlayerStateController>();

        if (player == null) return;

        _isUsed = true;

        if (_stageFlowController != null)
        {
            _stageFlowController.OnGoalReached();
        }
    }
}