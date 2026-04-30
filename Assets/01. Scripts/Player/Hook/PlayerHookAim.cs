using UnityEngine;

public class PlayerHookAim : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInputHandler _playerInput;
    [SerializeField] private Camera _mainCamera;

    [Header("Aim Settings")]
    [SerializeField] private float _hookLengthMax = 20f;
    [SerializeField] private float _aimRadius = 3f;
    [SerializeField] private LayerMask _hookableLayer;
    [SerializeField] private LayerMask _obstacleLayer;

    [Header("Aim Visual")]
    [SerializeField] private Transform _targetMarker;

    public HookableTarget CurrentTarget { get; private set; }
    public HookTargetType CurrentTargetType { get; private set; }
    public Vector2 HookAimDirection { get; private set; }
    public Vector2 CurrentTargetPoint { get; private set; }
    public float HookDistance { get; private set; }

    private Vector2 MouseWorldPosition;

    private void Awake()
    {
        if (_playerInput == null)
        {
            _playerInput = GetComponent<PlayerInputHandler>();
        }

        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_targetMarker != null)
        {
            _targetMarker.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateMousePosition();
        DetectHookableTarget();
        UpdateTargetMarker();
    }

    private void UpdateMousePosition()
    {
        if (_mainCamera == null || _playerInput == null) return;

        MouseWorldPosition = _mainCamera.ScreenToWorldPoint(_playerInput.MousePosition);
    }

    private void DetectHookableTarget()
    {
        CurrentTarget = null;
        CurrentTargetType = HookTargetType.Static;
        HookAimDirection = Vector2.zero;
        CurrentTargetPoint = Vector2.zero;
        HookDistance = 0f;

        Collider2D[] hookableObjects = Physics2D.OverlapCircleAll(
            MouseWorldPosition,
            _aimRadius,
            _hookableLayer
        );

        float closestDistance = float.MaxValue;

        foreach (Collider2D target in hookableObjects)
        {
            HookableTarget hookableTarget = target.GetComponent<HookableTarget>();

            if (hookableTarget == null)
            {
                hookableTarget = target.GetComponentInParent<HookableTarget>();
            }

            if (hookableTarget == null) continue;
            if (!hookableTarget.CanBeHooked) continue;

            Vector2 playerPosition = transform.position;
            Vector2 targetPosition = target.ClosestPoint(MouseWorldPosition);

            float currentTargetDistance = Vector2.Distance(MouseWorldPosition, targetPosition);
            float currentPlayerDistance = Vector2.Distance(playerPosition, targetPosition);

            if (currentPlayerDistance > _hookLengthMax) continue;

            RaycastHit2D wallHit = Physics2D.Linecast(playerPosition, targetPosition, _obstacleLayer);

            if (wallHit.collider != null) continue;

            if (currentTargetDistance < closestDistance)
            {
                closestDistance = currentTargetDistance;

                CurrentTarget = hookableTarget;
                CurrentTargetType = hookableTarget.TargetType;
                CurrentTargetPoint = targetPosition;
                HookDistance = Vector2.Distance(targetPosition, playerPosition);
                HookAimDirection = (targetPosition - playerPosition).normalized;
            }
        }
    }

    private void UpdateTargetMarker()
    {
        if (_targetMarker == null) return;

        if (CurrentTarget == null)
        {
            _targetMarker.gameObject.SetActive(false);
            return;
        }

        _targetMarker.gameObject.SetActive(true);
        _targetMarker.position = CurrentTargetPoint;
    }

    public void SetHookLengthMax(float hookLengthMax)
    {
        _hookLengthMax = Mathf.Max(0f, hookLengthMax);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(MouseWorldPosition, _aimRadius);

        if (CurrentTarget != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, CurrentTargetPoint);
        }
    }
}