using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkInventorySlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Button _button;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Equipped Visual")]
    [SerializeField] private GameObject _equippedOverlay;

    private ShopItemData _perkData;
    private PlayerPerkInventory _perkInventory;

    private void Awake()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    public void Setup(ShopItemData perkData, PlayerPerkInventory perkInventory)
    {
        _perkData = perkData;
        _perkInventory = perkInventory;

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnSlotClicked);
        }

        Refresh();
    }

    private void OnSlotClicked()
    {
        if (_perkData == null || _perkInventory == null) return;

        _perkInventory.ToggleEquipPerk(_perkData);
    }

    public void Refresh()
    {
        if (_perkData == null) return;

        if (_nameText != null)
        {
            _nameText.text = _perkData.DisplayName;
        }

        if (_iconImage != null)
        {
            if (_perkData.Icon != null)
            {
                _iconImage.sprite = _perkData.Icon;
                _iconImage.enabled = true;
            }
            else
            {
                _iconImage.enabled = false;
            }
        }

        bool isEquipped = _perkInventory != null && _perkInventory.IsEquipped(_perkData);

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = isEquipped ? 0.35f : 1f;
        }

        if (_equippedOverlay != null)
        {
            _equippedOverlay.SetActive(isEquipped);
        }
    }
}