using System;
using UnityEngine;

[Serializable]
public class ShopItemData
{
    [Header("Basic")]
    [SerializeField] private string _id;
    [SerializeField] private ShopItemCategory _category;
    [SerializeField] private string _displayName;

    [TextArea]
    [SerializeField] private string _description;

    [SerializeField] private Sprite _icon;

    [Header("Type")]
    [SerializeField] private PassiveType _passiveType;
    [SerializeField] private PerkType _perkType;

    [Header("Cost")]
    [SerializeField] private int _baseCost = 100;
    [SerializeField] private int _costIncreasePerLevel = 50;

    [Header("Passive Only")]
    [SerializeField] private int _maxLevel = 1;

    public string Id => _id;
    public ShopItemCategory Category => _category;
    public string DisplayName => _displayName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public PassiveType PassiveType => _passiveType;
    public PerkType PerkType => _perkType;
    public int MaxLevel => _maxLevel;

    public int GetCost(int currentLevel)
    {
        if (_category == ShopItemCategory.Perk)
        {
            return _baseCost;
        }

        return _baseCost + (_costIncreasePerLevel * currentLevel);
    }

    public bool IsPassiveMaxLevel(int currentLevel)
    {
        if (_category != ShopItemCategory.Passive)
        {
            return false;
        }

        return currentLevel >= _maxLevel;
    }
}