using System.Collections.Generic;
using UnityEngine;

public class StartShopEquippedPerkUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerPerkInventory _perkInventory;

    [Header("UI")]
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private EquippedPerkSlotUI _equippedSlotPrefab;

    private readonly List<EquippedPerkSlotUI> _createdSlots = new List<EquippedPerkSlotUI>();

    private void Awake()
    {
        if (_perkInventory == null)
        {
            _perkInventory = FindAnyObjectByType<PlayerPerkInventory>();
        }
    }

    private void OnEnable()
    {
        if (_perkInventory != null)
        {
            _perkInventory.OnEquippedPerksChanged += RefreshSlots;
        }

        RefreshSlots();
    }

    private void OnDisable()
    {
        if (_perkInventory != null)
        {
            _perkInventory.OnEquippedPerksChanged -= RefreshSlots;
        }
    }

    private void RefreshSlots()
    {
        ClearSlots();

        if (_perkInventory == null) return;
        if (_contentRoot == null) return;
        if (_equippedSlotPrefab == null) return;

        for (int i = 0; i < _perkInventory.EquippedSlotCount; i++)
        {
            EquippedPerkSlotUI slot = Instantiate(_equippedSlotPrefab, _contentRoot);
            slot.gameObject.SetActive(true);
            slot.Setup(i, _perkInventory);

            _createdSlots.Add(slot);
        }
    }

    private void ClearSlots()
    {
        for (int i = 0; i < _createdSlots.Count; i++)
        {
            if (_createdSlots[i] != null)
            {
                Destroy(_createdSlots[i].gameObject);
            }
        }

        _createdSlots.Clear();
    }
}