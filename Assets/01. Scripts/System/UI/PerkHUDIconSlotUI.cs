using UnityEngine;
using UnityEngine.UI;

public class PerkHUDIconSlotUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;

    private void Awake()
    {
        if (_iconImage == null)
        {
            _iconImage = GetComponent<Image>();
        }
    }

    public void Setup(Sprite icon)
    {
        if (_iconImage == null) return;

        _iconImage.sprite = icon;
        _iconImage.enabled = icon != null;
        _iconImage.preserveAspect = true;
    }
}