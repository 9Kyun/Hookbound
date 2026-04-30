using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private Button _buyButton;

    [Header("Sold Out")]
    [SerializeField] private GameObject _soldOutOverlay;
    [SerializeField] private TMP_Text _soldOutText;

    private ShopItemData _itemData;
    private ShopManager _shopManager;

    public void Setup(ShopItemData itemData, ShopManager shopManager)
    {
        _itemData = itemData;
        _shopManager = shopManager;

        if (_buyButton != null)
        {
            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        Refresh();
    }

    private void OnBuyButtonClicked()
    {
        if (_itemData == null || _shopManager == null) return;

        bool purchased = _shopManager.TryPurchase(_itemData);

        if (purchased)
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        if (_itemData == null) return;

        bool isSoldOut = _shopManager != null && _shopManager.IsSoldOut(_itemData);
        int cost = _shopManager != null ? _shopManager.GetCurrentCost(_itemData) : _itemData.GetCost(0);

        if (_nameText != null)
        {
            if (_itemData.Category == ShopItemCategory.Passive && _shopManager != null)
            {
                int level = _shopManager.GetCurrentLevel(_itemData);
                _nameText.text = $"{_itemData.DisplayName} Lv.{level}/{_itemData.MaxLevel}";
            }
            else
            {
                _nameText.text = _itemData.DisplayName;
            }
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = _itemData.Description;
        }

        if (_costText != null)
        {
            if (isSoldOut)
            {
                _costText.text = _itemData.Category == ShopItemCategory.Passive ? "MAX" : "SOLD OUT";
            }
            else
            {
                string currencyName = _itemData.Category == ShopItemCategory.Passive
                    ? "상점 재화"
                    : "퍽 재화";

                _costText.text = $"{currencyName}: {cost}";
            }
        }

        if (_iconImage != null)
        {
            if (_itemData.Icon != null)
            {
                _iconImage.sprite = _itemData.Icon;
                _iconImage.enabled = true;
            }
            else
            {
                _iconImage.enabled = false;
            }
        }

        SetSoldOut(isSoldOut);
    }

    private void SetSoldOut(bool isSoldOut)
    {
        if (_soldOutOverlay != null)
        {
            _soldOutOverlay.SetActive(isSoldOut);
        }

        if (_soldOutText != null)
        {
            _soldOutText.text = _itemData != null && _itemData.Category == ShopItemCategory.Passive
                ? "MAX"
                : "SOLD OUT";
        }

        if (_buyButton != null)
        {
            _buyButton.interactable = !isSoldOut;
        }
    }
}