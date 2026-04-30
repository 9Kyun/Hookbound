using UnityEngine;

public class PlayerDashUI : MonoBehaviour
{
    [SerializeField] private PlayerDash _playerDash;
    [SerializeField] private IconSlotGroupUI _dashGroupUI;

    private int _lastCurrent = -1;
    private int _lastMax = -1;

    private void Start()
    {
        RefreshInstant();
    }

    private void Update()
    {
        if (_playerDash == null || _dashGroupUI == null) return;

        if (_playerDash.CurrentDashCount != _lastCurrent ||
            _playerDash.MaxDashCount != _lastMax)
        {
            _dashGroupUI.SetValueAnimated(_playerDash.CurrentDashCount, _playerDash.MaxDashCount);
            _lastCurrent = _playerDash.CurrentDashCount;
            _lastMax = _playerDash.MaxDashCount;
        }
    }

    private void RefreshInstant()
    {
        if (_playerDash == null || _dashGroupUI == null) return;

        _dashGroupUI.SetValueInstant(_playerDash.CurrentDashCount, _playerDash.MaxDashCount);
        _lastCurrent = _playerDash.CurrentDashCount;
        _lastMax = _playerDash.MaxDashCount;
    }
}