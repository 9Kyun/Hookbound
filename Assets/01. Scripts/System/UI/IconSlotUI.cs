using UnityEngine;
using UnityEngine.UI;

public class IconSlotUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private Sprite _filledSprite;
    [SerializeField] private Sprite _emptySprite;
    [SerializeField] private Animator _animator;

    private bool _isFilled;

    public void SetFilledInstant(bool filled)
    {
        _isFilled = filled;
        _iconImage.sprite = filled ? _filledSprite : _emptySprite;
    }

    public void SetFilledAnimated(bool filled)
    {
        if (_isFilled == filled) return;

        _isFilled = filled;
        _iconImage.sprite = filled ? _filledSprite : _emptySprite;

        if (_animator != null)
        {
            _animator.SetTrigger(filled ? "Fill" : "Empty");
        }
    }
}
