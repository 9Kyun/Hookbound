using UnityEngine;

public class ShopDialogueProvider : MonoBehaviour
{
    [Header("Dialogues")]
    [TextArea]
    [SerializeField]
    private string[] _dialogues =
    {
        "어서 와. 오늘도 살아 돌아왔군.",
        "좋은 물건이 들어왔어.",
        "위로 올라가려면 준비가 필요하지.",
        "신중하게 골라.",
        "강화는 배신하지 않아."
    };

    public string GetRandomDialogue()
    {
        if (_dialogues == null || _dialogues.Length == 0)
        {
            return string.Empty;
        }

        int index = Random.Range(0, _dialogues.Length);
        return _dialogues[index];
    }
}