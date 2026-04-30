using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private IconSlotGroupUI _heartGroupUI;

    private void Start()
    {
        if (_playerHealth == null || _heartGroupUI == null) return;

        _heartGroupUI.SetValueInstant(_playerHealth.CurrentHp, _playerHealth.MaxHp);
        _playerHealth.OnHpChanged += HandleHpChanged;
    }

    private void OnDestroy()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnHpChanged -= HandleHpChanged;
        }
    }

    private void HandleHpChanged(int currentHp, int maxHp)
    {
        _heartGroupUI.SetValueAnimated(currentHp, maxHp);
    }
}