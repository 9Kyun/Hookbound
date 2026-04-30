using UnityEngine;

public class EnemyCurrencyDropper : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private CurrencyPickUp _shopCurrencyPrefab;
    [SerializeField] private CurrencyPickUp _rerollCurrencyPrefab;

    [Header("Shop Currency Drop")]
    [SerializeField] private int _shopCurrencyAmount = 1;
    [SerializeField] private int _shopCurrencyDropCount = 1;

    [Header("Reroll Currency Drop")]
    [SerializeField, Range(0f, 1f)] private float _rerollDropChance = 0.05f;
    [SerializeField] private int _rerollCurrencyAmount = 1;

    [Header("Drop Position")]
    [SerializeField] private float _dropScatterRadius = 0.5f;

    private bool _hasDropped;
    
    public void DropCurrency()
    {
        if (_hasDropped) return;
        _hasDropped = true;

        DropShopCurrency();
        TryDropRerollCurrency();
    }

    private void DropShopCurrency()
    {
        if (_shopCurrencyPrefab == null) return;
        if (_shopCurrencyAmount <= 0) return;
        if (_shopCurrencyDropCount <= 0) return;

        for (int i =0; i< _shopCurrencyDropCount; i++)
        {
            CurrencyPickUp pickUp = Instantiate(_shopCurrencyPrefab, GetDropPosition(), Quaternion.identity);

            pickUp.SetCurrency(CurrencyType.ShopCurrency, _shopCurrencyAmount);
        }
    }

    private void TryDropRerollCurrency()
    {
        if (_rerollCurrencyPrefab == null) return;
        if (_rerollCurrencyAmount <= 0) return;

        float randomValue = Random.value;

        if (randomValue > _rerollDropChance) return;

        CurrencyPickUp pickUp = Instantiate(_rerollCurrencyPrefab, GetDropPosition(), Quaternion.identity);

        pickUp.SetCurrency(CurrencyType.PerkCurrency, _rerollCurrencyAmount);
    }

    private Vector2 GetDropPosition()
    {
        Vector2 randomOffset = Random.insideUnitCircle * _dropScatterRadius;
        return (Vector2)transform.position + randomOffset;
    }
}
