using UnityEngine;

public class PlayerFloatingStatusUI : MonoBehaviour
{
    [SerializeField] private RectTransform _root;
    [SerializeField] private Transform _target;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 1.2f, 0f);

    private void Awake()
    {
        if (_root == null)
            _root = GetComponent<RectTransform>();

        if (_mainCamera == null)
            _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_root == null || _target == null || _mainCamera == null)
            return;

        Vector3 worldPosition = _target.position + _worldOffset;
        Vector3 screenPosition = _mainCamera.WorldToScreenPoint(worldPosition);

        bool isBehindCamera = screenPosition.z < 0f;
        _root.gameObject.SetActive(!isBehindCamera);

        if (isBehindCamera) return;

        _root.position = screenPosition;
    }
}