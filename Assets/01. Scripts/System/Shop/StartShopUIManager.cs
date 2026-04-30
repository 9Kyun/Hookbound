using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartShopUIManager : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject _shopUIRoot;
    [SerializeField] private GameObject _darkOverlay;
    [SerializeField] private GameObject _dialogueRoot;

    [Header("Text")]
    [SerializeField] private TMP_Text _dialogueText;

    [Header("Button")]
    [SerializeField] private Button _closeButton;

    [Header("Player")]
    [SerializeField] private PlayerControlLocker _playerControlLocker;

    [Header("Open Settings")]
    [SerializeField] private float _dialogueDurationBeforeOpen = 0f;

    public bool IsShopOpen { get; private set; }
    public bool IsShopOpening { get; private set; }
    public bool IsShopActive => IsShopOpen || IsShopOpening;

    private Coroutine _openShopCoroutine;

    private void Awake()
    {
        ResolveReferences();

        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(CloseShop);
            _closeButton.onClick.AddListener(CloseShop);
        }

        SetShopVisible(false);
        SetDialogueVisible(false);
    }

    private void ResolveReferences()
    {
        if (_shopUIRoot == null)
        {
            Transform found = FindInactiveChildByName("ShopUIRoot");
            if (found != null)
            {
                _shopUIRoot = found.gameObject;
            }
        }

        if (_darkOverlay == null && _shopUIRoot != null)
        {
            Transform found = _shopUIRoot.transform.Find("DarkOverlay");
            if (found != null)
            {
                _darkOverlay = found.gameObject;
            }
        }

        if (_dialogueRoot == null)
        {
            Transform found = FindInactiveChildByName("DialogueTextUI");
            if (found == null)
            {
                found = FindInactiveChildByName("DialogueUI");
            }

            if (found != null)
            {
                _dialogueRoot = found.gameObject;
            }
        }

        if (_dialogueText == null && _dialogueRoot != null)
        {
            _dialogueText = _dialogueRoot.GetComponentInChildren<TMP_Text>(true);
        }

        if (_closeButton == null && _shopUIRoot != null)
        {
            Transform found = _shopUIRoot.transform.Find("CloseButton");
            if (found != null)
            {
                _closeButton = found.GetComponent<Button>();
            }
        }

        if (_playerControlLocker == null)
        {
            _playerControlLocker = FindAnyObjectByType<PlayerControlLocker>();
        }
    }

    private Transform FindInactiveChildByName(string objectName)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform target = allTransforms[i];

            if (target.name != objectName) continue;
            if (!target.gameObject.scene.IsValid()) continue;

            return target;
        }

        return null;
    }

    public void OpenShop(string dialogue)
    {
        Debug.Log("StartShopUIManager OpenShop 호출됨");

        ResolveReferences();

        if (_shopUIRoot == null)
        {
            Debug.LogError("ShopUIRoot를 찾지 못했습니다. Canvas 아래 오브젝트 이름이 ShopUIRoot인지 확인하세요.");
            return;
        }

        if (IsShopActive) return;

        IsShopOpening = true;

        if (_playerControlLocker != null)
        {
            _playerControlLocker.LockControls();
        }

        ShowDialogue(dialogue);

        if (_dialogueDurationBeforeOpen <= 0f)
        {
            IsShopOpening = false;
            IsShopOpen = true;
            SetShopVisible(true);
            return;
        }

        _openShopCoroutine = StartCoroutine(OpenShopRoutine());
    }

    private IEnumerator OpenShopRoutine()
    {
        yield return new WaitForSecondsRealtime(_dialogueDurationBeforeOpen);

        IsShopOpening = false;
        IsShopOpen = true;

        SetShopVisible(true);

        _openShopCoroutine = null;
    }

    public void CloseShop()
    {
        if (!IsShopActive) return;

        if (_openShopCoroutine != null)
        {
            StopCoroutine(_openShopCoroutine);
            _openShopCoroutine = null;
        }

        IsShopOpening = false;
        IsShopOpen = false;

        SetShopVisible(false);
        SetDialogueVisible(false);

        if (_playerControlLocker != null)
        {
            _playerControlLocker.UnlockControls();
        }
    }

    private void SetShopVisible(bool visible)
    {
        if (_shopUIRoot != null)
        {
            _shopUIRoot.SetActive(visible);
            Debug.Log($"ShopUIRoot Active: {_shopUIRoot.activeSelf}");
        }

        if (_darkOverlay != null)
        {
            _darkOverlay.SetActive(visible);
        }
    }

    private void ShowDialogue(string dialogue)
    {
        SetDialogueVisible(true);

        if (_dialogueText != null)
        {
            _dialogueText.text = dialogue;
        }
    }

    private void SetDialogueVisible(bool visible)
    {
        if (_dialogueRoot != null)
        {
            _dialogueRoot.SetActive(visible);
        }
    }
}