using UnityEngine;

public class PlayerLifeSteal : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerHealth _playerHealth;

    [Header("Life Steal Settings")]
    [SerializeField] private int _requiredKillCount = 10;
    [SerializeField] private int _healAmount = 1;

    private bool _isLifeStealEnabled;
    private int _currentKillCount;

    private void Awake()
    {
        if (_playerHealth == null)
        {
            _playerHealth = GetComponent<PlayerHealth>();
        }
    }

    public void SetLifeStealEnabled(bool enabled)
    {
        _isLifeStealEnabled = enabled;

        if (!_isLifeStealEnabled)
        {
            _currentKillCount = 0;
        }
    }

    public void NotifyEnemyKilled()
    {
        if (!_isLifeStealEnabled) return;
        if (_playerHealth == null) return;

        _currentKillCount++;

        if (_currentKillCount < _requiredKillCount) return;

        _currentKillCount = 0;
        _playerHealth.Heal(_healAmount);
    }
}