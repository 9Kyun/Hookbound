using TMPro;
using UnityEngine;

public class StartShopStatsUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerPassiveStats _passiveStats;

    [Header("UI")]
    [SerializeField] private TMP_Text _statText;

    private void Awake()
    {
        if (_passiveStats == null)
        {
            _passiveStats = FindAnyObjectByType<PlayerPassiveStats>();
        }
    }

    private void OnEnable()
    {
        if (_passiveStats != null)
        {
            _passiveStats.OnStatsChanged += RefreshStats;
        }

        RefreshStats();
    }

    private void OnDisable()
    {
        if (_passiveStats != null)
        {
            _passiveStats.OnStatsChanged -= RefreshStats;
        }
    }

    private void RefreshStats()
    {
        if (_statText == null || _passiveStats == null) return;

        _statText.text =
            $"스탯\n" +
            $"최대 체력 : {_passiveStats.MaxHp}\n" +
            $"이동 속도 : {_passiveStats.MoveSpeed:0.0}\n" +
            $"훅 사거리 : {_passiveStats.HookRange:0.0}\n" +
            $"훅 속도 보너스 : {_passiveStats.HookSpeedBonusPercent:0}%\n" +
            $"퍽 슬롯 : {_passiveStats.PerkSlotCount}";
    }
}