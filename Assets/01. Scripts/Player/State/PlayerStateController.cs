using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    public PlayerState CurrentState { get; private set; } = PlayerState.Normal;

    public bool IsNormal => CurrentState == PlayerState.Normal;
    public bool IsDashing => CurrentState == PlayerState.Dash;
    public bool IsGrappling => CurrentState == PlayerState.Grapple;
    public bool IsAttacking => CurrentState == PlayerState.Attack;
    public bool IsParrying => CurrentState == PlayerState.Parry;
    public bool IsEvading => CurrentState == PlayerState.Evade;
    public bool IsCharging => CurrentState == PlayerState.Charging;
    public bool IsSlamming => CurrentState == PlayerState.Slam;
    public bool IsWallClimbing => CurrentState == PlayerState.WallClimb;
    public bool IsHit => CurrentState == PlayerState.Hit;
    public bool IsDead => CurrentState == PlayerState.Dead;


    public bool TryChangeState(PlayerState newState)
    {
        if (!CanChangeState(newState))
        {
            return false;
        }

        CurrentState = newState;
        return true;
    }

    public bool CanChangeState(PlayerState newState)
    {
        if (CurrentState == PlayerState.Dead)
        {
            return false;
        }

        switch (CurrentState)
        {
            case PlayerState.Normal:
                return newState == PlayerState.Dash
                    || newState == PlayerState.Grapple
                    || newState == PlayerState.Attack
                    || newState == PlayerState.Parry
                    || newState == PlayerState.Evade
                    || newState == PlayerState.Slam
                    || newState == PlayerState.WallClimb
                    || newState == PlayerState.Hit
                    || newState == PlayerState.Dead;

            case PlayerState.Dash:
                return newState == PlayerState.Normal
                    || newState == PlayerState.Evade
                    || newState == PlayerState.Hit
                    || newState == PlayerState.Dead;

            case PlayerState.Grapple:
                return newState == PlayerState.Normal
                    || newState == PlayerState.Hit
                    || newState == PlayerState.Dead;

            case PlayerState.Attack:
                return newState == PlayerState.Normal
                    || newState == PlayerState.Hit
                    || newState == PlayerState.Dead;

            case PlayerState.Parry:
                return newState == PlayerState.Normal
                    || newState == PlayerState.Grapple
                    || newState == PlayerState.Attack
                    || newState == PlayerState.Hit
                    || newState == PlayerState.Dead;

            case PlayerState.Evade:
                return newState == PlayerState.Normal
                    || newState == PlayerState.Charging;

            case PlayerState.Charging:
                return newState == PlayerState.Normal;

            case PlayerState.Slam:
                return newState == PlayerState.Normal
                    || newState == PlayerState.Hit
                    || newState == PlayerState.Dead;

            case PlayerState.WallClimb:
                return newState == PlayerState.Normal
                    || newState == PlayerState.Dash
                    || newState == PlayerState.Grapple
                    || newState == PlayerState.Hit
                    || newState == PlayerState.Dead;

            case PlayerState.Hit:
                return newState == PlayerState.Normal
                    || newState == PlayerState.Dead;
        }

        return false;
    }

    public bool CanMove()
    {
        return CurrentState == PlayerState.Normal;
    }

    public bool CanDash()
    {
        return CurrentState == PlayerState.Normal;
    }

    public bool CanGrapple()
    {
        return CurrentState == PlayerState.Normal;
    }

    public bool CanAttack()
    {
        return CurrentState == PlayerState.Normal;
    }

    public bool CanParry()
    {
        return CurrentState == PlayerState.Normal;
    }

    public bool CanEvade()
    {
        return CurrentState == PlayerState.Dash;
    }

    public bool CanCharge()
    {
        return CurrentState == PlayerState.Evade;
    }

    public bool CanSlam()
    {
        return CurrentState == PlayerState.Normal;
    }
}
