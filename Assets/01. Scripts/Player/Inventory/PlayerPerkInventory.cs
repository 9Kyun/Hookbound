using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPerkInventory : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerPassiveStats _passiveStats;
    [SerializeField] private ShopManager _shopManager;

    [Header("Settings")]
    [SerializeField] private int _fallbackPerkSlotCount = 3;

    private readonly List<ShopItemData> _ownedPerks = new List<ShopItemData>();
    private readonly HashSet<string> _ownedPerkIds = new HashSet<string>();

    private ShopItemData[] _equippedPerks = new ShopItemData[0];

    public IReadOnlyList<ShopItemData> OwnedPerks => _ownedPerks;
    public int EquippedSlotCount => _equippedPerks.Length;

    public event Action OnInventoryChanged;
    public event Action OnEquippedPerksChanged;

    private const string OwnedPerksSaveKey = "OwnedPerks";
    private const string EquippedPerksSaveKey = "EquippedPerks";
    private const char SaveSeparator = '|';

    private void Awake()
    {
        ResolveReferences();
        EnsureEquippedSlotSize();
        LoadPerkData();
    }

    private void Start()
    {
        ResolveReferences();
        EnsureEquippedSlotSize();
        LoadPerkData();

        OnInventoryChanged?.Invoke();
        OnEquippedPerksChanged?.Invoke();
    }

    private void OnEnable()
    {
        if (_passiveStats != null)
        {
            _passiveStats.OnStatsChanged += HandlePassiveStatsChanged;
        }
    }

    private void OnDisable()
    {
        if (_passiveStats != null)
        {
            _passiveStats.OnStatsChanged -= HandlePassiveStatsChanged;
        }
    }

    private void ResolveReferences()
    {
        if (_passiveStats == null)
        {
            _passiveStats = FindAnyObjectByType<PlayerPassiveStats>();
        }

        if (_shopManager == null)
        {
            _shopManager = FindAnyObjectByType<ShopManager>();
        }
    }

    private void HandlePassiveStatsChanged()
    {
        bool changed = EnsureEquippedSlotSize();

        if (changed)
        {
            SaveEquippedPerks();
            OnEquippedPerksChanged?.Invoke();
        }
    }

    private bool EnsureEquippedSlotSize()
    {
        int slotCount = _passiveStats != null
            ? _passiveStats.PerkSlotCount
            : _fallbackPerkSlotCount;

        slotCount = Mathf.Max(1, slotCount);

        if (_equippedPerks != null && _equippedPerks.Length == slotCount)
        {
            return false;
        }

        ShopItemData[] newSlots = new ShopItemData[slotCount];

        if (_equippedPerks != null)
        {
            int copyCount = Mathf.Min(_equippedPerks.Length, newSlots.Length);

            for (int i = 0; i < copyCount; i++)
            {
                newSlots[i] = _equippedPerks[i];
            }
        }

        _equippedPerks = newSlots;
        return true;
    }

    public bool HasPerk(string perkId)
    {
        return _ownedPerkIds.Contains(perkId);
    }

    public bool TryAddPerk(ShopItemData perkItem)
    {
        if (perkItem == null) return false;
        if (perkItem.Category != ShopItemCategory.Perk) return false;
        if (_ownedPerkIds.Contains(perkItem.Id)) return false;

        _ownedPerkIds.Add(perkItem.Id);
        _ownedPerks.Add(perkItem);

        SaveOwnedPerks();

        OnInventoryChanged?.Invoke();
        return true;
    }

    public ShopItemData GetEquippedPerk(int slotIndex)
    {
        if (_equippedPerks == null) return null;
        if (slotIndex < 0 || slotIndex >= _equippedPerks.Length) return null;

        return _equippedPerks[slotIndex];
    }

    public bool IsEquipped(ShopItemData perkItem)
    {
        if (perkItem == null) return false;

        return IsEquipped(perkItem.Id);
    }

    public bool IsEquipped(string perkId)
    {
        if (string.IsNullOrEmpty(perkId)) return false;

        for (int i = 0; i < _equippedPerks.Length; i++)
        {
            if (_equippedPerks[i] == null) continue;
            if (_equippedPerks[i].Id == perkId) return true;
        }

        return false;
    }

    public bool HasEquippedPerk(PerkType perkType)
    {
        if (_equippedPerks == null) return false;

        for (int i = 0; i < _equippedPerks.Length; i++)
        {
            if (_equippedPerks[i] == null) continue;
            if (_equippedPerks[i].PerkType == perkType) return true;
        }

        return false;
    }

    public void ToggleEquipPerk(ShopItemData perkItem)
    {
        if (perkItem == null) return;

        if (IsEquipped(perkItem))
        {
            UnequipPerk(perkItem);
        }
        else
        {
            TryEquipPerk(perkItem);
        }
    }

    public bool TryEquipPerk(ShopItemData perkItem)
    {
        if (perkItem == null) return false;
        if (perkItem.Category != ShopItemCategory.Perk) return false;
        if (!_ownedPerkIds.Contains(perkItem.Id)) return false;
        if (IsEquipped(perkItem)) return false;

        EnsureEquippedSlotSize();

        for (int i = 0; i < _equippedPerks.Length; i++)
        {
            if (_equippedPerks[i] != null) continue;

            _equippedPerks[i] = perkItem;

            SaveEquippedPerks();

            OnEquippedPerksChanged?.Invoke();
            return true;
        }

        Debug.Log("장착 가능한 퍽 슬롯이 없습니다.");
        return false;
    }

    public void UnequipPerk(ShopItemData perkItem)
    {
        if (perkItem == null) return;

        for (int i = 0; i < _equippedPerks.Length; i++)
        {
            if (_equippedPerks[i] == null) continue;
            if (_equippedPerks[i].Id != perkItem.Id) continue;

            _equippedPerks[i] = null;

            SaveEquippedPerks();

            OnEquippedPerksChanged?.Invoke();
            return;
        }
    }

    public void UnequipAt(int slotIndex)
    {
        if (_equippedPerks == null) return;
        if (slotIndex < 0 || slotIndex >= _equippedPerks.Length) return;
        if (_equippedPerks[slotIndex] == null) return;

        _equippedPerks[slotIndex] = null;

        SaveEquippedPerks();

        OnEquippedPerksChanged?.Invoke();
    }

    private void LoadPerkData()
    {
        ResolveReferences();

        LoadOwnedPerks();
        EnsureEquippedSlotSize();
        LoadEquippedPerks();
    }

    private void LoadOwnedPerks()
    {
        _ownedPerks.Clear();
        _ownedPerkIds.Clear();

        if (_shopManager == null) return;

        string saved = PlayerPrefs.GetString(OwnedPerksSaveKey, string.Empty);

        if (string.IsNullOrEmpty(saved)) return;

        string[] ids = saved.Split(SaveSeparator);

        for (int i = 0; i < ids.Length; i++)
        {
            string id = ids[i];

            if (string.IsNullOrEmpty(id)) continue;
            if (_ownedPerkIds.Contains(id)) continue;

            ShopItemData itemData = _shopManager.GetItemById(id);

            if (itemData == null) continue;
            if (itemData.Category != ShopItemCategory.Perk) continue;

            _ownedPerkIds.Add(id);
            _ownedPerks.Add(itemData);
        }
    }

    private void LoadEquippedPerks()
    {
        if (_shopManager == null) return;

        string saved = PlayerPrefs.GetString(EquippedPerksSaveKey, string.Empty);

        if (string.IsNullOrEmpty(saved)) return;

        string[] ids = saved.Split(SaveSeparator);

        int count = Mathf.Min(ids.Length, _equippedPerks.Length);

        for (int i = 0; i < count; i++)
        {
            string id = ids[i];

            if (string.IsNullOrEmpty(id))
            {
                _equippedPerks[i] = null;
                continue;
            }

            if (!_ownedPerkIds.Contains(id))
            {
                _equippedPerks[i] = null;
                continue;
            }

            ShopItemData itemData = _shopManager.GetItemById(id);

            if (itemData == null || itemData.Category != ShopItemCategory.Perk)
            {
                _equippedPerks[i] = null;
                continue;
            }

            _equippedPerks[i] = itemData;
        }
    }

    private void SaveOwnedPerks()
    {
        List<string> ids = new List<string>();

        for (int i = 0; i < _ownedPerks.Count; i++)
        {
            if (_ownedPerks[i] == null) continue;
            if (string.IsNullOrEmpty(_ownedPerks[i].Id)) continue;

            ids.Add(_ownedPerks[i].Id);
        }

        string saved = string.Join(SaveSeparator.ToString(), ids);

        PlayerPrefs.SetString(OwnedPerksSaveKey, saved);
        PlayerPrefs.Save();
    }

    private void SaveEquippedPerks()
    {
        EnsureEquippedSlotSize();

        string[] ids = new string[_equippedPerks.Length];

        for (int i = 0; i < _equippedPerks.Length; i++)
        {
            ids[i] = _equippedPerks[i] != null
                ? _equippedPerks[i].Id
                : string.Empty;
        }

        string saved = string.Join(SaveSeparator.ToString(), ids);

        PlayerPrefs.SetString(EquippedPerksSaveKey, saved);
        PlayerPrefs.Save();
    }

    public void ClearSaveData()
    {
        PlayerPrefs.DeleteKey(OwnedPerksSaveKey);
        PlayerPrefs.DeleteKey(EquippedPerksSaveKey);

        _ownedPerks.Clear();
        _ownedPerkIds.Clear();

        EnsureEquippedSlotSize();

        for (int i = 0; i < _equippedPerks.Length; i++)
        {
            _equippedPerks[i] = null;
        }

        PlayerPrefs.Save();

        OnInventoryChanged?.Invoke();
        OnEquippedPerksChanged?.Invoke();
    }
}