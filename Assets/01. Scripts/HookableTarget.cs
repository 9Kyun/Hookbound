using System;
using UnityEngine;

public enum HookTargetType
{
    Static,
    Pullable,
    Switch,
    Airborne,
    Projectile
}

public class HookableTarget : MonoBehaviour
{
    [SerializeField] private HookTargetType _targetType = HookTargetType.Static;
    [SerializeField] private bool _canBeHooked = true;

    public HookTargetType TargetType => _targetType;
    public bool CanBeHooked => _canBeHooked;

    public void SetCanBeHooked(bool canBeHooked)
    {
        _canBeHooked = canBeHooked;
    }
}
