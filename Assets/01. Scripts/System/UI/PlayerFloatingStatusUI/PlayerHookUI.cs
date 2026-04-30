using UnityEngine;

public class PlayerHookUI : MonoBehaviour
{
    [SerializeField] private PlayerHook _playerHook;
    [SerializeField] private IconSlotGroupUI _hookGroupUI;

    private int _lastCurrent = -1;
    private int _lastMax = -1;

    private void Start()
    {
        RefreshInstant();
    }

    private void Update()
    {
        if (_playerHook == null || _hookGroupUI == null) return;

        if (_playerHook.CurrentHookCount != _lastCurrent ||
            _playerHook.MaxHookCount != _lastMax)
        {
            _hookGroupUI.SetValueAnimated(_playerHook.CurrentHookCount, _playerHook.MaxHookCount);
            _lastCurrent = _playerHook.CurrentHookCount;
            _lastMax = _playerHook.MaxHookCount;
        }
    }

    private void RefreshInstant()
    {
        if (_playerHook == null || _hookGroupUI == null) return;

        _hookGroupUI.SetValueInstant(_playerHook.CurrentHookCount, _playerHook.MaxHookCount);
        _lastCurrent = _playerHook.CurrentHookCount;
        _lastMax = _playerHook.MaxHookCount;
    }
}