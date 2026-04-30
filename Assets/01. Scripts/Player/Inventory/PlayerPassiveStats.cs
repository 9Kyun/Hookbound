using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPassiveStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private int _baseMaxHp = 3;
    [SerializeField] private float _baseMoveSpeed = 10f;
    [SerializeField] private float _baseHookRange = 20f;
    [SerializeField] private float _baseHookSpeedBonusPercent = 0f;
    [SerializeField] private int _basePerkSlotCount = 3;

    private readonly Dictionary<PassiveType, int> _passiveLevels = new Dictionary<PassiveType, int>();

    public event Action OnStatsChanged;

    public int MaxHp => _baseMaxHp + GetLevel(PassiveType.MaxHp);
    public float MoveSpeed => _baseMoveSpeed + GetLevel(PassiveType.MoveSpeed) * 0.5f;
    public float HookRange => _baseHookRange + GetLevel(PassiveType.HookRange) * 2f;
    public float HookSpeedBonusPercent => _baseHookSpeedBonusPercent + GetLevel(PassiveType.HookSpeed) * 10f;
    public int PerkSlotCount => _basePerkSlotCount + GetLevel(PassiveType.PerkSlot);

    private const string PassiveSaveKeyPrefix = "PassiveLevel_";

    private void Awake()
    {
        LoadPassiveLevels();
    }

    public int GetLevel(PassiveType passiveType)
    {
        if (_passiveLevels.TryGetValue(passiveType, out int level))
        {
            return level;
        }

        return 0;
    }

    public bool CanUpgrade(PassiveType passiveType, int maxLevel)
    {
        return GetLevel(passiveType) < maxLevel;
    }

    public bool TryUpgrade(PassiveType passiveType, int maxLevel)
    {
        if (!CanUpgrade(passiveType, maxLevel))
        {
            return false;
        }

        int currentLevel = GetLevel(passiveType);
        _passiveLevels[passiveType] = currentLevel + 1;

        SavePassiveLevels();

        OnStatsChanged?.Invoke();
        return true;
    }

    private void LoadPassiveLevels()
    {
        _passiveLevels.Clear();

        PassiveType[] passiveTypes = (PassiveType[])Enum.GetValues(typeof(PassiveType));

        for (int i = 0; i < passiveTypes.Length; i++)
        {
            PassiveType passiveType = passiveTypes[i];
            string key = GetPassiveSaveKey(passiveType);

            int level = PlayerPrefs.GetInt(key, 0);

            if (level > 0)
            {
                _passiveLevels[passiveType] = level;
            }
        }

        OnStatsChanged?.Invoke();
    }

    private void SavePassiveLevels()
    {
        PassiveType[] passiveTypes = (PassiveType[])Enum.GetValues(typeof(PassiveType));

        for (int i = 0; i < passiveTypes.Length; i++)
        {
            PassiveType passiveType = passiveTypes[i];
            string key = GetPassiveSaveKey(passiveType);

            PlayerPrefs.SetInt(key, GetLevel(passiveType));
        }

        PlayerPrefs.Save();
    }

    private string GetPassiveSaveKey(PassiveType passiveType)
    {
        return PassiveSaveKeyPrefix + passiveType;
    }

    public void ClearSaveData()
    {
        PassiveType[] passiveTypes = (PassiveType[])Enum.GetValues(typeof(PassiveType));

        for (int i = 0; i < passiveTypes.Length; i++)
        {
            PlayerPrefs.DeleteKey(GetPassiveSaveKey(passiveTypes[i]));
        }

        _passiveLevels.Clear();
        PlayerPrefs.Save();

        OnStatsChanged?.Invoke();
    }
}