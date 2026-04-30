using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameHUDUIManager : MonoBehaviour
{
    [Header("Stage UI")]
    [SerializeField] private TMP_Text _stageText;

    [Header("Currency UI")]
    [SerializeField] private TMP_Text _shopCurrencyText;
    [SerializeField] private TMP_Text _perkCurrencyText;

    [Header("Run UI")]
    [SerializeField] private TMP_Text _killCountText;

    [Header("Perk UI")]
    [SerializeField] private Transform _perkIconContent;
    [SerializeField] private PerkHUDIconSlotUI _perkIconSlotPrefab;

    private PlayerPerkInventory _perkInventory;
    private RunDataManager _runDataManager;
    private CurrencyManager _currencyManager;

    private readonly List<PerkHUDIconSlotUI> _createdPerkIcons = new List<PerkHUDIconSlotUI>();

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (_perkInventory != null)
        {
            _perkInventory.OnEquippedPerksChanged += RefreshPerkIcons;
        }

        if (_runDataManager != null)
        {
            _runDataManager.OnRunDataChanged += RefreshRunHUD;
        }

        if (_currencyManager != null)
        {
            _currencyManager.OnCurrencyChanged += HandleCurrencyChanged;
        }

        RefreshAll();
    }

    private void OnDisable()
    {
        if (_perkInventory != null)
        {
            _perkInventory.OnEquippedPerksChanged -= RefreshPerkIcons;
        }

        if (_runDataManager != null)
        {
            _runDataManager.OnRunDataChanged -= RefreshRunHUD;
        }

        if (_currencyManager != null)
        {
            _currencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
        }
    }

    private void ResolveReferences()
    {
        if (_perkInventory == null)
        {
            _perkInventory = FindAnyObjectByType<PlayerPerkInventory>();
        }

        if (_runDataManager == null)
        {
            _runDataManager = FindAnyObjectByType<RunDataManager>();
        }

        if (_currencyManager == null)
        {
            _currencyManager = FindAnyObjectByType<CurrencyManager>();
        }
    }

    private void HandleCurrencyChanged(int shopCurrency, int perkCurrency)
    {
        RefreshRunHUD();
    }

    private void RefreshAll()
    {
        RefreshRunHUD();
        RefreshPerkIcons();
    }

    private void RefreshRunHUD()
    {
        ResolveReferences();

        RefreshStageText();
        RefreshCurrencyText();
        RefreshKillCountText();
    }

    private void RefreshStageText()
    {
        if (_stageText == null) return;

        int subStage = 1;

        if (_runDataManager != null)
        {
            subStage = Mathf.Clamp(_runDataManager.CurrentStage, 1, 3);
        }

        _stageText.text = $"1-{subStage}";
    }

    private void RefreshCurrencyText()
    {
        int baseShopCurrency = 0;
        int basePerkCurrency = 0;

        int runShopCurrency = 0;
        int runPerkCurrency = 0;

        if (_currencyManager != null)
        {
            baseShopCurrency = _currencyManager.ShopCurrency;
            basePerkCurrency = _currencyManager.PerkCurrency;
        }

        if (_runDataManager != null && _runDataManager.IsRunActive)
        {
            runShopCurrency = _runDataManager.EarnedShopCurrency;
            runPerkCurrency = _runDataManager.EarnedPerkCurrency;
        }

        if (_shopCurrencyText != null)
        {
            _shopCurrencyText.text = $"{baseShopCurrency + runShopCurrency}";
        }

        if (_perkCurrencyText != null)
        {
            _perkCurrencyText.text = $"{basePerkCurrency + runPerkCurrency}";
        }
    }

    private void RefreshKillCountText()
    {
        if (_killCountText == null) return;

        int killCount = 0;

        if (_runDataManager != null)
        {
            killCount = _runDataManager.KillCount;
        }

        _killCountText.text = $"{killCount}";
    }

    private void RefreshPerkIcons()
    {
        ClearPerkIcons();

        if (_perkInventory == null) return;
        if (_perkIconContent == null) return;
        if (_perkIconSlotPrefab == null) return;

        for (int i = 0; i < _perkInventory.EquippedSlotCount; i++)
        {
            ShopItemData equippedPerk = _perkInventory.GetEquippedPerk(i);

            if (equippedPerk == null) continue;
            if (equippedPerk.Icon == null) continue;

            PerkHUDIconSlotUI slot = Instantiate(_perkIconSlotPrefab, _perkIconContent);
            slot.Setup(equippedPerk.Icon);

            _createdPerkIcons.Add(slot);
        }
    }

    private void ClearPerkIcons()
    {
        for (int i = 0; i < _createdPerkIcons.Count; i++)
        {
            if (_createdPerkIcons[i] != null)
            {
                Destroy(_createdPerkIcons[i].gameObject);
            }
        }

        _createdPerkIcons.Clear();
    }
}