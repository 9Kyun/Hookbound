using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private EnemyStateController _enemyState;

    [Header("HP Settings")]
    [SerializeField] private int _maxHp = 1;
    [SerializeField] private bool _destroyOnDeath = true;

    public int CurrentHp { get; private set; }
    public int MaxHp => _maxHp;

    public event Action<int, int> OnHpChanged;
    public event Action OnDied;

    private void Awake()
    {
        if (_enemyState == null)
        {
            _enemyState = GetComponent<EnemyStateController>();
        }

        CurrentHp = _maxHp;
    }

    public EnemyHitResult ApplyDamage(int amount)
    {
        if (amount <= 0) return EnemyHitResult.None;
        if (_enemyState != null && _enemyState.IsDead) return EnemyHitResult.None;

        CurrentHp = Mathf.Max(CurrentHp - amount, 0);
        OnHpChanged?.Invoke(CurrentHp, _maxHp);

        if (CurrentHp > 0)
        {
            return EnemyHitResult.Damaged;
        }

        if (_enemyState != null)
        {
            _enemyState.Die();
        }

        OnDied?.Invoke();

        if (_destroyOnDeath)
        {
            Destroy(gameObject);
        }

        return EnemyHitResult.Killed;
    }
}
