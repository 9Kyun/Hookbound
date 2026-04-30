using UnityEngine;

public enum DamageDealingType
{
    Contact,
    AttackHitBox,
    Projectile
}

public class DamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int _damage = 1;
    [SerializeField] private bool _destroyOnHit = false;
    [SerializeField] private DamageDealingType _damageType = DamageDealingType.Contact;

    public DamageDealingType DamageType => _damageType;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActiveAndEnabled) return;

        PlayerHitBox playerHitBox = collision.GetComponent<PlayerHitBox>();

        if (playerHitBox == null)
        {
            playerHitBox = collision.GetComponentInParent<PlayerHitBox>();
        }

        if (playerHitBox == null) return;

        PlayerDamageReceiver damageReceiver = collision.GetComponentInParent<PlayerDamageReceiver>();

        if (damageReceiver == null)
        {
            damageReceiver = collision.GetComponent<PlayerDamageReceiver>();
        }

        if (damageReceiver == null) return;

        Vector2 hitDirection = GetHitDirection(collision.transform);
        bool isHit = damageReceiver.ReceiveDamage(_damage, hitDirection, _damageType);

        if (isHit && _destroyOnHit)
        {
            Destroy(gameObject);
        }
    }

    private Vector2 GetHitDirection(Transform targetTransform)
    {
        Vector2 direction = (targetTransform.position - transform.position).normalized;

        if (direction == Vector2.zero)
            direction = Vector2.up;

        return direction;
    }
}