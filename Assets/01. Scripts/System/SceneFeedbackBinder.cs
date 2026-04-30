using UnityEngine;
using UnityEngine.UI;

public class SceneFeedbackBinder : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform _cameraShakePivot;
    [SerializeField] private Image _damageFlashImage;

    private void Start()
    {
        if (GameFeedbackManager.Instance == null)
        {
            Debug.LogWarning("GameFeedbackManager가 씬에 없습니다.");
            return;
        }

        GameFeedbackManager.Instance.SetSceneReferences(
            _cameraShakePivot,
            _damageFlashImage
        );
    }
}