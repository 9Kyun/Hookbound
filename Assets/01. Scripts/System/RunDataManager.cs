using System;
using UnityEngine;

public class RunDataManager : MonoBehaviour
{
    public static RunDataManager Instance { get; private set; }

    public int CurrentStage { get; private set; }
    public int KillCount { get; private set; }
    public int EarnedShopCurrency { get; private set; }
    public int EarnedPerkCurrency { get; private set; }
    public float ReachedHeight { get; private set; }
    public int Score { get; private set; }
    public bool IsCleared { get; private set; }
    public bool IsRunActive { get; private set; }

    public event Action OnRunDataChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartNewRun()
    {
        CurrentStage = 1;
        KillCount = 0;
        EarnedShopCurrency = 0;
        EarnedPerkCurrency = 0;
        ReachedHeight = 0f;
        Score = 0;
        IsCleared = false;
        IsRunActive = true;

        OnRunDataChanged?.Invoke();
    }

    public void AddKillCount(int amount = 1)
    {
        if (!IsRunActive) return;
        if (amount <= 0) return;

        KillCount += amount;
        Score += amount * 100;

        OnRunDataChanged?.Invoke();
    }

    public void AddCurrency(CurrencyType currencyType, int amount)
    {
        if (!IsRunActive) return;
        if (amount <= 0) return;

        switch (currencyType)
        {
            case CurrencyType.ShopCurrency:
                EarnedShopCurrency += amount;
                break;

            case CurrencyType.PerkCurrency:
                EarnedPerkCurrency += amount;
                break;
        }

        Score += amount * 10;

        OnRunDataChanged?.Invoke();
    }

    public void SetReachedHeight(float height)
    {
        if (!IsRunActive) return;

        ReachedHeight = Mathf.Max(ReachedHeight, height);
        Score = Mathf.Max(Score, Mathf.RoundToInt(ReachedHeight * 10f));

        OnRunDataChanged?.Invoke();
    }

    public void AdvanceStage()
    {
        if (!IsRunActive) return;

        CurrentStage++;
        Score += 500;

        OnRunDataChanged?.Invoke();
    }

    public void FinishRun(bool isCleared)
    {
        if (!IsRunActive) return;

        IsCleared = isCleared;
        IsRunActive = false;

        if (isCleared)
        {
            Score += 1000;
        }

        OnRunDataChanged?.Invoke();
    }

    public void ClearRunData()
    {
        CurrentStage = 0;
        KillCount = 0;
        EarnedShopCurrency = 0;
        EarnedPerkCurrency = 0;
        ReachedHeight = 0f;
        Score = 0;
        IsCleared = false;
        IsRunActive = false;

        OnRunDataChanged?.Invoke();
    }
}