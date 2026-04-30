using TMPro;
using UnityEngine;

public class StartShopCurrencyUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CurrencyManager _currencyManager;

    [Header("UI")]
    [SerializeField] private TMP_Text _shopCurrencyText;
    [SerializeField] private TMP_Text _perkCurrencyText;

    private void Awake()
    {
        if (_currencyManager == null)
        {
            _currencyManager = FindAnyObjectByType<CurrencyManager>();
        }
    }

    private void OnEnable()
    {
        if (_currencyManager != null)
        {
            _currencyManager.OnCurrencyChanged += RefreshCurrencyUI;
            RefreshCurrencyUI(_currencyManager.ShopCurrency, _currencyManager.PerkCurrency);
        }
    }

    private void OnDisable()
    {
        if (_currencyManager != null)
        {
            _currencyManager.OnCurrencyChanged -= RefreshCurrencyUI;
        }
    }

    private void RefreshCurrencyUI(int shopCurrency, int perkCurrency)
    {
        if (_shopCurrencyText != null)
        {
            _shopCurrencyText.text = $"골드 : {shopCurrency}";
        }

        if (_perkCurrencyText != null)
        {
            _perkCurrencyText.text = $"퍽 토큰 : {perkCurrency}";
        }
    }
}