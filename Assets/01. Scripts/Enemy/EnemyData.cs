using UnityEngine;

public enum EnemyKind
{
    Grounded,
    Flyer,
    Turret
}

public enum WeightType
{
    Static,
    Light,
    Heavy
}

public enum AttackReactionType
{
    Normal,
    Armored,
    WeakPointOnly
}

public enum ChaseType
{
    None,
    Always,
    RangeBased
}

public enum AttackType
{
    Contact,
    Ranged,
    Explosive
}

public class EnemyData : MonoBehaviour
{
    [Header("Enemy Type")]
    [SerializeField] private EnemyKind _enemyKind = EnemyKind.Grounded;
    [SerializeField] private WeightType _enemyWeight = WeightType.Static;
    [SerializeField] private AttackReactionType _attackReactionType = AttackReactionType.Normal;
    [SerializeField] private ChaseType _chaseType = ChaseType.None;
    [SerializeField] private AttackType _attackType = AttackType.Contact;

    [Header("Physics")]
    [SerializeField] private float _groundGravityScale = 2.5f;

    [Header("Hook Settings")]
    [Range(0f,1f)]
    [SerializeField] private float _lightPullRate = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float _heavyPullRate = 0.1f;

    public EnemyKind EnemyKind => _enemyKind;
    public WeightType WeightType => _enemyWeight;
    public AttackReactionType AttackReaction => _attackReactionType;
    public ChaseType ChaseType => _chaseType;
    public AttackType AttackType => _attackType;

    public float PullRate 
    {
        get
        {
            switch (_enemyWeight)
            {
                case WeightType.Light:
                    return _lightPullRate;
                
                case WeightType.Heavy:
                    return _heavyPullRate;

                case WeightType.Static:
                default:
                    return 0f;
            }
        }
    }
    public float DefaultGravityScale
    {
        get
        {
            switch (_enemyKind)
            {
                case EnemyKind.Flyer:
                case EnemyKind.Turret:
                    return 0f;

                case EnemyKind.Grounded:
                default:
                    return _groundGravityScale;
            }
        }
    }
}