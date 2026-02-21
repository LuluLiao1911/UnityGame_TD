using UnityEngine;
using TMPro;
using System.Collections; // 必須引用此命名空間以使用協程

public class MessageBoxController : MonoBehaviour
{
    public GameObject messageBox;
    public TextMeshProUGUI messageText;

    void Awake()
    {
        if (messageBox != null) messageBox.SetActive(false);
    }

    public void ShowMessage(string content)
    {
        if (messageBox != null)
        {
            if (messageText != null)
            {
                messageText.text = content;
            }

            messageBox.SetActive(true);

            // 停止原本可能正在運作的自動消失計時，避免衝突
            StopAllCoroutines();

            // 開始自動消失的協程
            StartCoroutine(AutoCloseRoutine());
        }
    }

    // 新增：自動消失的協程
    private IEnumerator AutoCloseRoutine()
    {
        // 1. 等待 1 秒
        // 注意：如果你在顯示 MessageBox 時 Time.timeScale = 0 (暫停狀態)
        // 必須使用 WaitForSecondsRealtime，否則計時會跟著停止
        yield return new WaitForSecondsRealtime(1f);

        // 2. 執行關閉
        CloseMessage();
    }

    public void CloseMessage()
    {
        if (messageBox != null)
        {
            messageBox.SetActive(false);

            // 如果你有暫停遊戲，記得在這裡恢復
            // Time.timeScale = 1f; 
        }
    }
}
