using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultSceneUIManager : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text _resultTitleText;
    [SerializeField] private TMP_Text _stageText;
    [SerializeField] private TMP_Text _heightText;
    [SerializeField] private TMP_Text _killCountText;
    [SerializeField] private TMP_Text _shopCurrencyText;
    [SerializeField] private TMP_Text _perkCurrencyText;

    [Header("Button")]
    [SerializeField] private Button _returnButton;

    private RunDataManager _runDataManager;
    private CurrencyManager _currencyManager;

    private bool _isRewardClaimed;

    private void Awake()
    {
        _runDataManager = FindAnyObjectByType<RunDataManager>();
        _currencyManager = FindAnyObjectByType<CurrencyManager>();

        if (_returnButton != null)
        {
            _returnButton.onClick.AddListener(OnReturnButtonClicked);
        }
    }

    private void Start()
    {
        RefreshResultUI();
        ClaimReward();
    }

    private void OnDestroy()
    {
        if (_returnButton != null)
        {
            _returnButton.onClick.RemoveListener(OnReturnButtonClicked);
        }
    }

    private void RefreshResultUI()
    {
        if (_runDataManager == null)
        {
            SetText(_resultTitleText, "결과 없음");
            return;
        }

        SetText(_resultTitleText, _runDataManager.IsCleared ? "CLEAR" : "GAME OVER");
        SetText(_stageText, $"도달 스테이지 : {_runDataManager.CurrentStage}");
        SetText(_heightText, $"도달 높이 : {_runDataManager.ReachedHeight:0.0}");
        SetText(_killCountText, $"처치 수 : {_runDataManager.KillCount}");
        SetText(_shopCurrencyText, $"획득 상점 재화 : {_runDataManager.EarnedShopCurrency}");
        SetText(_perkCurrencyText, $"획득 퍽 재화 : {_runDataManager.EarnedPerkCurrency}");
    }

    private void ClaimReward()
    {
        if (_isRewardClaimed) return;
        if (_runDataManager == null) return;
        if (_currencyManager == null) return;

        _currencyManager.AddCurrency(
            CurrencyType.ShopCurrency,
            _runDataManager.EarnedShopCurrency
        );

        _currencyManager.AddCurrency(
            CurrencyType.PerkCurrency,
            _runDataManager.EarnedPerkCurrency
        );

        _isRewardClaimed = true;
    }

    private void OnReturnButtonClicked()
    {
        if (_runDataManager != null)
        {
            _runDataManager.ClearRunData();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }

    private void SetText(TMP_Text targetText, string value)
    {
        if (targetText == null) return;

        targetText.text = value;
    }
}