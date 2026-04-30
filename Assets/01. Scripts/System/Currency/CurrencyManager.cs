using System;
using UnityEngine;

public enum CurrencyType
{
    ShopCurrency,
    PerkCurrency
}

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Test Start Currency")]
    [SerializeField] private int _startShopCurrency = 1000;
    [SerializeField] private int _startPerkCurrency = 30;
    [SerializeField] private bool _resetCurrencyOnPlay = false;

    public int ShopCurrency { get; private set; }
    public int PerkCurrency { get; private set; }

    public event Action<int, int> OnCurrencyChanged;

    private const string ShopCurrencyKey = "ShopCurrency";
    private const string PerkCurrencyKey = "PerkCurrency";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (_resetCurrencyOnPlay)
        {
            ShopCurrency = _startShopCurrency;
            PerkCurrency = _startPerkCurrency;
            SaveCurrency();
        }
        else
        {
            LoadCurrency();
        }

        NotifyCurrencyChanged();
    }

    public void AddCurrency(CurrencyType currencyType, int amount)
    {
        if (amount <= 0) return;

        switch (currencyType)
        {
            case CurrencyType.ShopCurrency:
                ShopCurrency += amount;
                break;

            case CurrencyType.PerkCurrency:
                PerkCurrency += amount;
                break;
        }

        SaveCurrency();
        NotifyCurrencyChanged();
    }

    public bool TrySpend(CurrencyType currencyType, int amount)
    {
        if (amount <= 0) return true;

        switch (currencyType)
        {
            case CurrencyType.ShopCurrency:
                if (ShopCurrency < amount) return false;
                ShopCurrency -= amount;
                break;

            case CurrencyType.PerkCurrency:
                if (PerkCurrency < amount) return false;
                PerkCurrency -= amount;
                break;
        }

        SaveCurrency();
        NotifyCurrencyChanged();
        return true;
    }

    private void LoadCurrency()
    {
        ShopCurrency = PlayerPrefs.GetInt(ShopCurrencyKey, _startShopCurrency);
        PerkCurrency = PlayerPrefs.GetInt(PerkCurrencyKey, _startPerkCurrency);
    }

    private void SaveCurrency()
    {
        PlayerPrefs.SetInt(ShopCurrencyKey, ShopCurrency);
        PlayerPrefs.SetInt(PerkCurrencyKey, PerkCurrency);
        PlayerPrefs.Save();
    }

    private void NotifyCurrencyChanged()
    {
        OnCurrencyChanged?.Invoke(ShopCurrency, PerkCurrency);
    }
}