using UnityEngine;

public class CameraFollowWithBounds : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Follow Settings")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 1f, -10f);
    [SerializeField] private float _smoothTime = 0.15f;

    [Header("Bounds")]
    [SerializeField] private BoxCollider2D _cameraBounds;

    private Camera _camera;
    private Vector3 _velocity;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (_target == null) return;
        if (_camera == null) return;

        Vector3 targetPosition = _target.position + _offset;
        targetPosition.z = transform.position.z;

        Vector3 clampedPosition = ClampCameraPosition(targetPosition);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            clampedPosition,
            ref _velocity,
            _smoothTime
        );
    }

    private Vector3 ClampCameraPosition(Vector3 targetPosition)
    {
        if (_cameraBounds == null)
        {
            return targetPosition;
        }

        Bounds bounds = _cameraBounds.bounds;

        float cameraHalfHeight = _camera.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * _camera.aspect;

        float minX = bounds.min.x + cameraHalfWidth;
        float maxX = bounds.max.x - cameraHalfWidth;
        float minY = bounds.min.y + cameraHalfHeight;
        float maxY = bounds.max.y - cameraHalfHeight;

        float clampedX = targetPosition.x;
        float clampedY = targetPosition.y;

        if (minX <= maxX)
        {
            clampedX = Mathf.Clamp(targetPosition.x, minX, maxX);
        }
        else
        {
            clampedX = bounds.center.x;
        }

        if (minY <= maxY)
        {
            clampedY = Mathf.Clamp(targetPosition.y, minY, maxY);
        }
        else
        {
            clampedY = bounds.center.y;
        }

        return new Vector3(clampedX, clampedY, targetPosition.z);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void SetBounds(BoxCollider2D cameraBounds)
    {
        _cameraBounds = cameraBounds;
    }
}