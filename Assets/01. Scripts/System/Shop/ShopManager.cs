using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CurrencyManager _currencyManager;
    [SerializeField] private PlayerPerkInventory _perkInventory;
    [SerializeField] private PlayerPassiveStats _passiveStats;

    [Header("Passive Items")]
    [SerializeField] private List<ShopItemData> _passiveItems = new List<ShopItemData>();

    [Header("Perk Items")]
    [SerializeField] private List<ShopItemData> _perkItems = new List<ShopItemData>();

    private readonly Dictionary<string, ShopItemData> _itemLookup = new Dictionary<string, ShopItemData>();

    public IReadOnlyList<ShopItemData> PassiveItems => _passiveItems;
    public IReadOnlyList<ShopItemData> PerkItems => _perkItems;

    public event Action OnShopDataChanged;

    private void Awake()
    {
        ResolveReferences();
        BuildItemLookup();
    }

    private void ResolveReferences()
    {
        if (_currencyManager == null)
        {
            _currencyManager = FindAnyObjectByType<CurrencyManager>();
        }

        if (_perkInventory == null)
        {
            _perkInventory = FindAnyObjectByType<PlayerPerkInventory>();
        }

        if (_passiveStats == null)
        {
            _passiveStats = FindAnyObjectByType<PlayerPassiveStats>();
        }
    }

    private void BuildItemLookup()
    {
        _itemLookup.Clear();

        for (int i = 0; i < _passiveItems.Count; i++)
        {
            RegisterItem(_passiveItems[i]);
        }

        for (int i = 0; i < _perkItems.Count; i++)
        {
            RegisterItem(_perkItems[i]);
        }
    }

    private void RegisterItem(ShopItemData itemData)
    {
        if (itemData == null) return;
        if (string.IsNullOrEmpty(itemData.Id)) return;

        if (_itemLookup.ContainsKey(itemData.Id))
        {
            Debug.LogWarning($"중복된 상점 아이템 ID가 있습니다: {itemData.Id}");
            return;
        }

        _itemLookup.Add(itemData.Id, itemData);
    }

    public ShopItemData GetItemById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;

        if (_itemLookup.Count == 0)
        {
            BuildItemLookup();
        }

        _itemLookup.TryGetValue(itemId, out ShopItemData itemData);
        return itemData;
    }

    public bool TryPurchase(ShopItemData itemData)
    {
        if (itemData == null) return false;

        ResolveReferences();

        switch (itemData.Category)
        {
            case ShopItemCategory.Passive:
                return TryPurchasePassive(itemData);

            case ShopItemCategory.Perk:
                return TryPurchasePerk(itemData);
        }

        return false;
    }

    private bool TryPurchasePassive(ShopItemData itemData)
    {
        if (_currencyManager == null || _passiveStats == null) return false;

        int currentLevel = _passiveStats.GetLevel(itemData.PassiveType);

        if (itemData.IsPassiveMaxLevel(currentLevel))
        {
            return false;
        }

        int cost = itemData.GetCost(currentLevel);

        if (!_currencyManager.TrySpend(CurrencyType.ShopCurrency, cost))
        {
            return false;
        }

        bool upgraded = _passiveStats.TryUpgrade(itemData.PassiveType, itemData.MaxLevel);

        if (!upgraded)
        {
            return false;
        }

        OnShopDataChanged?.Invoke();
        return true;
    }

    private bool TryPurchasePerk(ShopItemData itemData)
    {
        if (_currencyManager == null || _perkInventory == null) return false;
        if (_perkInventory.HasPerk(itemData.Id)) return false;

        int cost = itemData.GetCost(0);

        if (!_currencyManager.TrySpend(CurrencyType.PerkCurrency, cost))
        {
            return false;
        }

        bool added = _perkInventory.TryAddPerk(itemData);

        if (!added)
        {
            return false;
        }

        OnShopDataChanged?.Invoke();
        return true;
    }

    public bool IsSoldOut(ShopItemData itemData)
    {
        if (itemData == null) return false;

        ResolveReferences();

        if (itemData.Category == ShopItemCategory.Perk)
        {
            return _perkInventory != null && _perkInventory.HasPerk(itemData.Id);
        }

        if (itemData.Category == ShopItemCategory.Passive)
        {
            if (_passiveStats == null) return false;

            int currentLevel = _passiveStats.GetLevel(itemData.PassiveType);
            return itemData.IsPassiveMaxLevel(currentLevel);
        }

        return false;
    }

    public int GetCurrentLevel(ShopItemData itemData)
    {
        if (itemData == null) return 0;
        if (itemData.Category != ShopItemCategory.Passive) return 0;

        ResolveReferences();

        if (_passiveStats == null) return 0;

        return _passiveStats.GetLevel(itemData.PassiveType);
    }

    public int GetCurrentCost(ShopItemData itemData)
    {
        if (itemData == null) return 0;

        int currentLevel = GetCurrentLevel(itemData);
        return itemData.GetCost(currentLevel);
    }

    public void NotifyShopDataChanged()
    {
        OnShopDataChanged?.Invoke();
    }
}