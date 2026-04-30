using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HookRopeVisual : MonoBehaviour
{
    [SerializeField] private PlayerHook _playerHook;
    [SerializeField] private PlayerHookAim _hookAim;
    [SerializeField] private Transform _playerRoot;
    [SerializeField] private bool _showAimPreviewLine = false;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        if (_playerRoot == null)
        {
            _playerRoot = transform.root;
        }

        if (_playerHook == null)
        {
            _playerHook = GetComponentInParent<PlayerHook>();
        }

        if (_hookAim == null)
        {
            _hookAim = GetComponentInParent<PlayerHookAim>();
        }

        _lineRenderer.positionCount = 2;
        _lineRenderer.enabled = false;
    }

    private void LateUpdate()
    {
        if (_playerHook != null && (_playerHook.IsHookPreparing || _playerHook.IsGrappling))
        {
            DrawLine(_playerRoot.position, _playerHook.ActiveHookPoint);
            return;
        }

        if (_showAimPreviewLine && _hookAim != null && _hookAim.CurrentTarget != null)
        {
            DrawLine(_playerRoot.position, _hookAim.CurrentTargetPoint);
            return;
        }

        _lineRenderer.enabled = false;
    }

    private void DrawLine(Vector2 start, Vector2 end)
    {
        _lineRenderer.enabled = true;
        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, end);
    }
}