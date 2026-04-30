using UnityEngine;

public class EnemyTargetSensor : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Detect Settings")]
    [SerializeField] private float _detectRange = 5f;
    [SerializeField] private float _attackRange = 2f;

    public Transform Target => _target;
    public bool HasTarget => _target != null;

    private void Awake()
    {
        if (_target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _target = player.transform;
            }
        }
    }

    public float GetDistanceToTarget()
    {
        if (_target == null) return float.MaxValue;
        return Vector2.Distance(transform.position, _target.position);
    }

    public Vector2 GetDirectionToTarget()
    {
        if (_target == null) return Vector2.zero;
        return (_target.position - transform.position).normalized;
    }

    public bool IsTargetInDetectRange()
    {
        return GetDistanceToTarget() <= _detectRange;
    }

    public bool IsTargetInAttackRange()
    {
        return GetDistanceToTarget() <= _attackRange;
    }
}
