using UnityEngine;

public class ShopInteractionTarget : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private StartShopUIManager _shopUIManager;
    [SerializeField] private ShopDialogueProvider _dialogueProvider;

    [Header("Interaction")]
    [SerializeField] private string _interactionText = "E : 상점 이용";

    public string InteractionText => _interactionText;

    private void Awake()
    {
        if (_dialogueProvider == null)
        {
            _dialogueProvider = GetComponent<ShopDialogueProvider>();
        }

        if (_shopUIManager == null)
        {
            _shopUIManager = FindAnyObjectByType<StartShopUIManager>();
        }
    }

    public void Interact()
    {
        Debug.Log("ShopInteractionTarget Interact 호출됨");

        if (_shopUIManager == null)
        {
            Debug.LogError("Shop UI Manager가 연결되지 않음");
            return;
        }

        string dialogue = string.Empty;

        if (_dialogueProvider != null)
        {
            dialogue = _dialogueProvider.GetRandomDialogue();
        }

        _shopUIManager.OpenShop(dialogue);
    }
}