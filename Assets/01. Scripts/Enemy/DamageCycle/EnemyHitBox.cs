using System.Runtime.InteropServices;
using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private EnemyDamageReceiver _enemyDamageReceiver;

    private void Awake()
    {
        if (_enemyDamageReceiver == null)
        {
            _enemyDamageReceiver = GetComponentInParent<EnemyDamageReceiver>();
        }
    }

    public EnemyHitResult ReceiveHit(PlayerAttackType attackType, int damage = 1)
    {
        if (_enemyDamageReceiver == null) return EnemyHitResult.None;

        return _enemyDamageReceiver.ReceiveHit(attackType, damage);
    }
}
