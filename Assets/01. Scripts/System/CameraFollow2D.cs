using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Follow Settings")]
    [SerializeField] private float _offsetY = 2f;
    [SerializeField] private float _ySmoothTime = 0.1f;

    private float _yVelocity;

    private void LateUpdate()
    {
        if (_target == null)
            return;

        float targetY = _target.position.y + _offsetY;

        float newY = Mathf.SmoothDamp(transform.position.y, targetY, ref _yVelocity, _ySmoothTime);

        transform.position = new Vector3(0, newY, -10f);
    }
}