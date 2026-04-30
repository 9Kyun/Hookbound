using UnityEngine;

public class PlayerCurrencyMagnet : MonoBehaviour
{
    [Header("Magnet Perk")]
    [SerializeField] private bool _isMagnetPerkActive;

    [Header("Magnet Settings")]
    [SerializeField] private float _magnetRadius = 8f;
    [SerializeField] private LayerMask _currencyLayer;

    private readonly Collider2D[] _results = new Collider2D[32];

    private void Update()
    {
        if (!_isMagnetPerkActive) return;

        DetectCurrency();
    }

    private void DetectCurrency()
    {
        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            _magnetRadius,
            _results,
            _currencyLayer
        );

        for (int i = 0; i < count; i++)
        {
            CurrencyPickUp pickUp = _results[i].GetComponentInParent<CurrencyPickUp>();

            if (pickUp == null) continue;

            pickUp.StartMagnet(transform);
        }
    }

    public void SetMagnetPerkActive(bool active)
    {
        _isMagnetPerkActive = active;
    }

    public void SetMagnetEnabled(bool active)
    {
        SetMagnetPerkActive(active);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _magnetRadius);
    }
}