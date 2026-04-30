using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquippedPerkSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _emptyText;
    [SerializeField] private Button _button;

    private int _slotIndex;
    private PlayerPerkInventory _perkInventory;

    private void Awake()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }
    }

    public void Setup(int slotIndex, PlayerPerkInventory perkInventory)
    {
        _slotIndex = slotIndex;
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
        if (_perkInventory == null) return;

        _perkInventory.UnequipAt(_slotIndex);
    }

    public void Refresh()
    {
        if (_perkInventory == null) return;

        ShopItemData equippedPerk = _perkInventory.GetEquippedPerk(_slotIndex);
        bool hasPerk = equippedPerk != null;

        if (_iconImage != null)
        {
            if (hasPerk && equippedPerk.Icon != null)
            {
                _iconImage.sprite = equippedPerk.Icon;
                _iconImage.enabled = true;
            }
            else
            {
                _iconImage.enabled = false;
            }
        }

        if (_nameText != null)
        {
            _nameText.text = hasPerk ? equippedPerk.DisplayName : string.Empty;
        }

        if (_emptyText != null)
        {
            _emptyText.gameObject.SetActive(!hasPerk);
            _emptyText.text = "EMPTY";
        }

        if (_button != null)
        {
            _button.interactable = hasPerk;
        }
    }
}