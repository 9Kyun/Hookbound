using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _shopCurrencyText;
    [SerializeField] private TMP_Text _rerollCurrencyText;

    private bool _isSubscribed;

    private void OnEnable()
    {
        TrySubscribe();
        RefreshFromManager();
    }

    private void Start()
    {
        TrySubscribe();
        RefreshFromManager();
    }

    private void Update()
    {
        if (!_isSubscribed)
        {
            TrySubscribe();
            RefreshFromManager();
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (_isSubscribed) return;
        if (CurrencyManager.Instance == null) return;

        CurrencyManager.Instance.OnCurrencyChanged += RefreshCurrencyUI;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed) return;
        if (CurrencyManager.Instance == null) return;

        CurrencyManager.Instance.OnCurrencyChanged -= RefreshCurrencyUI;
        _isSubscribed = false;
    }

    private void RefreshFromManager()
    {
        if (CurrencyManager.Instance == null) return;

        RefreshCurrencyUI(
            CurrencyManager.Instance.ShopCurrency,
            CurrencyManager.Instance.PerkCurrency
        );
    }

    private void RefreshCurrencyUI(int shopCurrency, int rerollCurrency)
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
}