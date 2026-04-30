using System.Collections.Generic;
using UnityEngine;

public class StartShopInventoryUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerPerkInventory _perkInventory;

    [Header("UI")]
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private PerkInventorySlotUI _perkSlotPrefab;

    private readonly List<PerkInventorySlotUI> _createdSlots = new List<PerkInventorySlotUI>();

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
            _perkInventory.OnInventoryChanged += RefreshInventory;
            _perkInventory.OnEquippedPerksChanged += RefreshInventory;
        }

        RefreshInventory();
    }

    private void OnDisable()
    {
        if (_perkInventory != null)
        {
            _perkInventory.OnInventoryChanged -= RefreshInventory;
            _perkInventory.OnEquippedPerksChanged -= RefreshInventory;
        }
    }

    private void RefreshInventory()
    {
        ClearInventory();

        if (_perkInventory == null) return;
        if (_contentRoot == null) return;
        if (_perkSlotPrefab == null) return;

        IReadOnlyList<ShopItemData> ownedPerks = _perkInventory.OwnedPerks;

        for (int i = 0; i < ownedPerks.Count; i++)
        {
            PerkInventorySlotUI slot = Instantiate(_perkSlotPrefab, _contentRoot);
            slot.gameObject.SetActive(true);
            slot.Setup(ownedPerks[i], _perkInventory);

            _createdSlots.Add(slot);
        }
    }

    private void ClearInventory()
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