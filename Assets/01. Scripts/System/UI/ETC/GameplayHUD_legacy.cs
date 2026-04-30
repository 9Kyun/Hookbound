using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameplayHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private PlayerHook _playerHook;
    [SerializeField] private PlayerDash _playerDash;
    [SerializeField] private CurrencyManager _currencyManager;

    [Header("Heart UI")]
    [SerializeField] private Transform _heartContainer;
    [SerializeField] private IconSlotUI _heartSlotPrefab;

    [Header("Hook UI")]
    [SerializeField] private Transform _hookContainer;
    [SerializeField] private IconSlotUI _hookSlotPrefab;

    [Header("Dash UI")]
    [SerializeField] private Transform _dashContainer;
    [SerializeField] private IconSlotUI _dashSlotPrefab;

    [Header("Currency UI")]
    [SerializeField] private TMP_Text _rerollCurrencyText;
    [SerializeField] private TMP_Text _shopCurrencyText;

    private readonly List<IconSlotUI> _heartSlots = new();
    private readonly List<IconSlotUI> _hookSlots = new();
    private readonly List<IconSlotUI> _dashSlots = new();

    private int _lastHp = -1;
    private int _lastMaxHp = -1;
    private int _lastHookCount = -1;
    private int _lastHookMax = -1;
    private int _lastDashCount = -1;
    private int _lastDashMax = -1;

    private bool _isCurrencyEventSubscribed;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        TrySubscribeCurrencyEvent();
    }

    private void Start()
    {
        ResolveReferences();

        InitializeAll();
        RefreshAllInstant();

        TrySubscribeCurrencyEvent();
        RefreshCurrencyFromManager();
    }

    private void OnDisable()
    {
        UnsubscribeCurrencyEvent();
    }

    private void Update()
    {
        RefreshIfChanged();
    }

    private void ResolveReferences()
    {
        if (_currencyManager == null)
        {
            _currencyManager = CurrencyManager.Instance;
        }
    }

    private void TrySubscribeCurrencyEvent()
    {
        ResolveReferences();

        if (_currencyManager == null) return;
        if (_isCurrencyEventSubscribed) return;

        _currencyManager.OnCurrencyChanged += RefreshCurrency;
        _isCurrencyEventSubscribed = true;

        RefreshCurrencyFromManager();
    }

    private void UnsubscribeCurrencyEvent()
    {
        if (_currencyManager == null) return;
        if (!_isCurrencyEventSubscribed) return;

        _currencyManager.OnCurrencyChanged -= RefreshCurrency;
        _isCurrencyEventSubscribed = false;
    }

    private void InitializeAll()
    {
        if (_playerHealth != null)
        {
            BuildSlots(_heartSlots, _heartContainer, _heartSlotPrefab, _playerHealth.MaxHp);
        }

        if (_playerHook != null)
        {
            BuildSlots(_hookSlots, _hookContainer, _hookSlotPrefab, _playerHook.MaxHookCount);
        }

        if (_playerDash != null)
        {
            BuildSlots(_dashSlots, _dashContainer, _dashSlotPrefab, _playerDash.MaxDashCount);
        }
    }

    private void RefreshIfChanged()
    {
        if (_playerHealth != null &&
            (_playerHealth.CurrentHp != _lastHp || _playerHealth.MaxHp != _lastMaxHp))
        {
            RefreshHeartsAnimated();
        }

        if (_playerHook != null &&
            (_playerHook.CurrentHookCount != _lastHookCount || _playerHook.MaxHookCount != _lastHookMax))
        {
            RefreshHooksAnimated();
        }

        if (_playerDash != null &&
            (_playerDash.CurrentDashCount != _lastDashCount || _playerDash.MaxDashCount != _lastDashMax))
        {
            RefreshDashesAnimated();
        }
    }

    private void RefreshAllInstant()
    {
        RefreshHeartsInstant();
        RefreshHooksInstant();
        RefreshDashesInstant();
        RefreshCurrencyFromManager();
    }

    private void RefreshHeartsInstant()
    {
        if (_playerHealth == null) return;

        UpdateSlotsInstant(_heartSlots, _playerHealth.CurrentHp);
        _lastHp = _playerHealth.CurrentHp;
        _lastMaxHp = _playerHealth.MaxHp;
    }

    private void RefreshHeartsAnimated()
    {
        if (_playerHealth == null) return;

        UpdateSlotsAnimated(_heartSlots, _playerHealth.CurrentHp);
        _lastHp = _playerHealth.CurrentHp;
        _lastMaxHp = _playerHealth.MaxHp;
    }

    private void RefreshHooksInstant()
    {
        if (_playerHook == null) return;

        UpdateSlotsInstant(_hookSlots, _playerHook.CurrentHookCount);
        _lastHookCount = _playerHook.CurrentHookCount;
        _lastHookMax = _playerHook.MaxHookCount;
    }

    private void RefreshHooksAnimated()
    {
        if (_playerHook == null) return;

        UpdateSlotsAnimated(_hookSlots, _playerHook.CurrentHookCount);
        _lastHookCount = _playerHook.CurrentHookCount;
        _lastHookMax = _playerHook.MaxHookCount;
    }

    private void RefreshDashesInstant()
    {
        if (_playerDash == null) return;

        UpdateSlotsInstant(_dashSlots, _playerDash.CurrentDashCount);
        _lastDashCount = _playerDash.CurrentDashCount;
        _lastDashMax = _playerDash.MaxDashCount;
    }

    private void RefreshDashesAnimated()
    {
        if (_playerDash == null) return;

        UpdateSlotsAnimated(_dashSlots, _playerDash.CurrentDashCount);
        _lastDashCount = _playerDash.CurrentDashCount;
        _lastDashMax = _playerDash.MaxDashCount;
    }

    private void RefreshCurrencyFromManager()
    {
        if (_currencyManager == null) return;

        RefreshCurrency(
            _currencyManager.ShopCurrency,
            _currencyManager.PerkCurrency
        );
    }

    private void RefreshCurrency(int shopCurrency, int rerollCurrency)
    {
        if (_shopCurrencyText != null)
        {
            _shopCurrencyText.text = shopCurrency.ToString();
        }

        if (_rerollCurrencyText != null)
        {
            _rerollCurrencyText.text = rerollCurrency.ToString();
        }
    }

    private void BuildSlots(List<IconSlotUI> slotList, Transform parent, IconSlotUI prefab, int count)
    {
        if (parent == null) return;
        if (prefab == null) return;

        slotList.Clear();

        for (int i = 0; i < count; i++)
        {
            IconSlotUI slot = Instantiate(prefab, parent);
            slotList.Add(slot);
        }
    }

    private void UpdateSlotsInstant(List<IconSlotUI> slots, int currentValue)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetFilledInstant(i < currentValue);
        }
    }

    private void UpdateSlotsAnimated(List<IconSlotUI> slots, int currentValue)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetFilledAnimated(i < currentValue);
        }
    }
}