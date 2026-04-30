using UnityEngine;

public enum EnemyHitResult
{
    None,
    ArmorBroken,
    Stunned,
    Damaged,
    Killed
}

public class EnemyDamageReceiver : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private EnemyData _enemyData;
    [SerializeField] private EnemyStateController _enemyState;
    [SerializeField] private EnemyHealth _enemyHealth;
    [SerializeField] private EnemyCurrencyDropper _currencyDropper;

    private void Awake()
    {
        if (_enemyData == null)
        {
            _enemyData = GetComponent<EnemyData>();
        }

        if (_enemyState == null)
        {
            _enemyState = GetComponent<EnemyStateController>();
        }

        if (_enemyHealth == null)
        {
            _enemyHealth = GetComponent<EnemyHealth>();
        }

        if (_currencyDropper == null)
        {
            _currencyDropper = GetComponent<EnemyCurrencyDropper>();
        }
    }

    public EnemyHitResult ReceiveHit(PlayerAttackType attackType, int damage = 1)
    {
        if (damage <= 0) return EnemyHitResult.None;
        if (_enemyData == null || _enemyState == null || _enemyHealth == null) return EnemyHitResult.None;
        if (_enemyState.IsDead) return EnemyHitResult.None;

        EnemyHitResult result = EnemyHitResult.None;

        if (attackType == PlayerAttackType.PiercingMelee)
        {
            result = HandlePiercingHit(damage);
        }
        else
        {
            switch (_enemyData.AttackReaction)
            {
                case AttackReactionType.Normal:
                    result = HandleNormalHit(damage);
                    break;

                case AttackReactionType.Armored:
                    result = HandleArmoredHit(attackType, damage);
                    break;

                case AttackReactionType.WeakPointOnly:
                    result = HandleWeakPointOnlyHit(damage);
                    break;
            }
        }

        if (result == EnemyHitResult.Killed)
        {
            RunDataManager runDataManager = FindAnyObjectByType<RunDataManager>();

            if (runDataManager != null)
            {
                runDataManager.AddKillCount(1);
            }

            _currencyDropper?.DropCurrency();
        }

        return result;
    }

    private EnemyHitResult HandleNormalHit(int damage)
    {
        return _enemyHealth.ApplyDamage(damage);
    }

    private EnemyHitResult HandleArmoredHit(PlayerAttackType attackType, int damage)
    {
        if (_enemyState.HasArmor)
        {
            bool canBreakArmor =
                attackType == PlayerAttackType.Melee ||
                attackType == PlayerAttackType.ParryProjectile;

            if (!canBreakArmor)
            {
                return EnemyHitResult.None;
            }

            _enemyState.BreakArmor();
            return EnemyHitResult.ArmorBroken;
        }

        return _enemyHealth.ApplyDamage(damage);
    }

    private EnemyHitResult HandleWeakPointOnlyHit(int damage)
    {
        if (_enemyState.IsStunned)
        {
            return _enemyHealth.ApplyDamage(damage);
        }

        if (!_enemyState.IsWeakPointOpen)
        {
            return EnemyHitResult.None;
        }

        _enemyState.EnterStunned();
        return EnemyHitResult.Stunned;
    }

    private EnemyHitResult HandlePiercingHit(int damage)
    {
        if (_enemyState.HasArmor)
        {
            _enemyState.BreakArmor();
        }

        return _enemyHealth.ApplyDamage(damage);
    }
}