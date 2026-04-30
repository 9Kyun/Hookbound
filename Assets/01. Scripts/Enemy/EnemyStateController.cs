using UnityEngine;

public class EnemyStateController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private EnemyData _enemyData;

    public bool HasArmor { get; private set; }
    public bool IsWeakPointOpen { get; private set; }
    public bool IsStunned { get; private set; }
    public bool IsPulled { get; private set; }
    public bool IsDead { get; private set; }

    public bool IsNormal => !IsDead && !IsWeakPointOpen && !IsStunned && !IsPulled;

    private void Awake()
    {
        if (_enemyData == null)
        {
            _enemyData = GetComponent<EnemyData>();
        }

        ResetState();
    }

    public void ResetState()
    {
        IsDead = false;
        IsWeakPointOpen = false;
        IsStunned = false;
        IsPulled = false;

        HasArmor = _enemyData != null && _enemyData.AttackReaction == AttackReactionType.Armored;
    }

    public void OpenWeakPoint()
    {
        if (!IsNormal) return;

        IsWeakPointOpen = true;
    }

    public void CloseWeakPoint()
    {
        if (!IsWeakPointOpen) return;

        IsWeakPointOpen = false;
        IsStunned = false;
    }

    public void EnterStunned()
    {
        if (IsDead) return;
        if (!IsWeakPointOpen) return;

        IsStunned = true;
    }

    public void ExitStunned()
    {
        if (IsDead) return;
        if (!IsStunned) return;

        IsStunned = false;
    }

    public void StartPulled()
    {
        if (IsDead) return;
        if (IsPulled) return;

        IsPulled = true;
    }

    public void EndPulled()
    {
        if (IsDead) return;
        if (!IsPulled) return;

        IsPulled = false;
    }

    public void BreakArmor()
    {
        if (IsDead) return;

        HasArmor = false;
    }

    public void Die()
    {
        IsDead = true;
        HasArmor = false;
        IsWeakPointOpen = false;
        IsStunned = false;
        IsPulled = false;
    }
}
