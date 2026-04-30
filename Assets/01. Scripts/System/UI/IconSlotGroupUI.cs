using System.Collections.Generic;
using UnityEngine;

public class IconSlotGroupUI : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private IconSlotUI _slotPrefab;
    [SerializeField] private Transform _slotParent;

    private readonly List<IconSlotUI> _slots = new List<IconSlotUI>();
    private int _currentMax = -1;

    public void SetValueInstant(int current, int max)
    {
        RebuildIfNeeded(max);
        RefreshInstant(current);
    }

    public void SetValueAnimated(int current, int max)
    {
        RebuildIfNeeded(max);
        RefreshAnimated(current);
    }

    private void RebuildIfNeeded(int max)
    {
        if (_currentMax == max) return;

        ClearSlots();

        for (int i = 0; i < max; i++)
        {
            IconSlotUI slot = Instantiate(_slotPrefab, _slotParent);
            _slots.Add(slot);
        }

        _currentMax = max;
    }

    private void RefreshInstant(int current)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].SetFilledInstant(i < current);
        }
    }

    private void RefreshAnimated(int current)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].SetFilledAnimated(i < current);
        }
    }

    private void ClearSlots()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] != null)
            {
                Destroy(_slots[i].gameObject);
            }
        }

        _slots.Clear();
    }
}