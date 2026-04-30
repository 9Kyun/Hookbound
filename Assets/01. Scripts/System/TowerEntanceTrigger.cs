using UnityEngine;

public class TowerEntranceTrigger : MonoBehaviour
{
    [SerializeField] private StartSceneFlowManager _flowManager;

    private bool _isUsed;

    private void Awake()
    {
        if (_flowManager == null)
        {
            _flowManager = FindAnyObjectByType<StartSceneFlowManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isUsed) return;

        PlayerStateController player = collision.GetComponentInParent<PlayerStateController>();

        if (player == null) return;

        _isUsed = true;
        _flowManager.EnterGameScene();
    }
}