using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartShopListUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private ShopManager _shopManager;

    [Header("Tab Buttons")]
    [SerializeField] private Button _passiveTabButton;
    [SerializeField] private Button _perkTabButton;

    [Header("Scroll View")]
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private ShopItemSlotUI _shopItemSlotPrefab;

    private readonly List<ShopItemSlotUI> _createdSlots = new List<ShopItemSlotUI>();
    private ShopItemCategory _currentCategory = ShopItemCategory.Passive;

    private void Awake()
    {
        if (_shopManager == null)
        {
            _shopManager = FindAnyObjectByType<ShopManager>();
        }

        if (_passiveTabButton != null)
        {
            _passiveTabButton.onClick.AddListener(ShowPassiveItems);
        }

        if (_perkTabButton != null)
        {
            _perkTabButton.onClick.AddListener(ShowPerkItems);
        }
    }

    private void OnEnable()
    {
        if (_shopManager != null)
        {
            _shopManager.OnShopDataChanged += RefreshCurrentList;
        }

        ShowPassiveItems();
    }

    private void OnDisable()
    {
        if (_shopManager != null)
        {
            _shopManager.OnShopDataChanged -= RefreshCurrentList;
        }
    }

    private void OnDestroy()
    {
        if (_passiveTabButton != null)
        {
            _passiveTabButton.onClick.RemoveListener(ShowPassiveItems);
        }

        if (_perkTabButton != null)
        {
            _perkTabButton.onClick.RemoveListener(ShowPerkItems);
        }
    }

    public void ShowPassiveItems()
    {
        _currentCategory = ShopItemCategory.Passive;

        if (_shopManager == null) return;

        RefreshList(_shopManager.PassiveItems);
    }

    public void ShowPerkItems()
    {
        _currentCategory = ShopItemCategory.Perk;

        if (_shopManager == null) return;

        RefreshList(_shopManager.PerkItems);
    }

    private void RefreshCurrentList()
    {
        if (_currentCategory == ShopItemCategory.Passive)
        {
            ShowPassiveItems();
        }
        else
        {
            ShowPerkItems();
        }
    }

    private void RefreshList(IReadOnlyList<ShopItemData> items)
    {
        ClearList();

        if (_contentRoot == null) return;
        if (_shopItemSlotPrefab == null) return;

        for (int i = 0; i < items.Count; i++)
        {
            ShopItemSlotUI slot = Instantiate(_shopItemSlotPrefab, _contentRoot);
            slot.gameObject.SetActive(true);
            slot.Setup(items[i], _shopManager);

            _createdSlots.Add(slot);
        }
    }

    private void ClearList()
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