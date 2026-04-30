using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] PlayerStateController _playerState;

    [Header("HP Settings")]
    [SerializeField] private int _maxHp = 5;

    public int CurrentHp { get; private set; }
    public int MaxHp => _maxHp;

    public event Action<int, int> OnHpChanged;
    public event Action OnDied;

    private void Awake()
    {
        if (_playerState == null)
        {
            _playerState = GetComponent<PlayerStateController>();
        }

        CurrentHp = _maxHp;
    }

    public void TakeDamage(int amount)
    {
        if (_playerState.IsDead) return;
        if (amount <= 0) return;

        CurrentHp = Mathf.Max(CurrentHp - amount, 0);
        OnHpChanged?.Invoke(CurrentHp, _maxHp);

        Debug.Log(CurrentHp);

        if (CurrentHp == 0)
        {
            bool isDead = _playerState.TryChangeState(PlayerState.Dead);

            if (isDead) OnDied?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (_playerState.IsDead) return;
        if (amount <= 0) return;

        CurrentHp = Mathf.Min(CurrentHp + amount, _maxHp);
        OnHpChanged?.Invoke(CurrentHp, _maxHp);
    }

    public void SetMaxHp(int newMaxHp)
    {
        int previousMaxHp = _maxHp;

        _maxHp = Mathf.Max(1, newMaxHp);

        if (_maxHp > previousMaxHp)
        {
            CurrentHp += _maxHp - previousMaxHp;
        }

        CurrentHp = Mathf.Clamp(CurrentHp, 0, _maxHp);

        OnHpChanged?.Invoke(CurrentHp, _maxHp);
    }
}
