using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShopInteractor : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _interactionPromptRoot;
    [SerializeField] private TMP_Text _interactionPromptText;

    [Header("Shop UI")]
    [SerializeField] private StartShopUIManager _shopUIManager;

    private ShopInteractionTarget _currentShopTarget;

    private void Awake()
    {
        if (_shopUIManager == null)
        {
            _shopUIManager = FindAnyObjectByType<StartShopUIManager>();
        }

        HidePrompt();
    }

    private void Update()
    {
        HandleInteractionInput();
        HandleCloseInput();
    }

    private void HandleInteractionInput()
    {
        if (_currentShopTarget == null) return;
        if (_shopUIManager != null && _shopUIManager.IsShopActive) return;
        if (Keyboard.current == null) return;
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        HidePrompt();
        _currentShopTarget.Interact();
    }

    private void HandleCloseInput()
    {
        if (_shopUIManager == null) return;
        if (!_shopUIManager.IsShopActive) return;
        if (Keyboard.current == null) return;
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        _shopUIManager.CloseShop();

        if (_currentShopTarget != null)
        {
            ShowPrompt(_currentShopTarget.InteractionText);
        }
    }

    public void SetCurrentShopTarget(ShopInteractionTarget shopTarget)
    {
        if (shopTarget == null) return;

        _currentShopTarget = shopTarget;

        if (_shopUIManager != null && _shopUIManager.IsShopActive) return;

        ShowPrompt(shopTarget.InteractionText);
    }

    public void ClearCurrentShopTarget(ShopInteractionTarget shopTarget)
    {
        if (_currentShopTarget != shopTarget) return;

        _currentShopTarget = null;
        HidePrompt();
    }

    private void ShowPrompt(string text)
    {
        if (_interactionPromptRoot != null)
        {
            _interactionPromptRoot.SetActive(true);
        }

        if (_interactionPromptText != null)
        {
            _interactionPromptText.text = text;
        }
    }

    private void HidePrompt()
    {
        if (_interactionPromptRoot != null)
        {
            _interactionPromptRoot.SetActive(false);
        }
    }
}