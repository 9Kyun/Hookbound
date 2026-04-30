using UnityEngine;

public class ShopInteractionTrigger : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private ShopInteractionTarget _shopTarget;

    private void Awake()
    {
        if (_shopTarget == null)
        {
            _shopTarget = GetComponentInParent<ShopInteractionTarget>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerShopInteractor interactor = collision.GetComponentInParent<PlayerShopInteractor>();

        if (interactor == null) return;

        interactor.SetCurrentShopTarget(_shopTarget);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerShopInteractor interactor = collision.GetComponentInParent<PlayerShopInteractor>();

        if (interactor == null) return;

        interactor.ClearCurrentShopTarget(_shopTarget);
    }
}