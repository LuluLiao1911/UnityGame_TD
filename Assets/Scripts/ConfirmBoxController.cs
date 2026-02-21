using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConfirmBoxController : MonoBehaviour
{
    [Header("UI 元件設定 (ConfirmBox 1)")]
    public GameObject confirmPanel;
    public TextMeshProUGUI messageText;
    public Button yesButton;
    public Button cancelButton;

    [Header("UI 元件設定 (ConfirmBox 2 - 不足時)")]
    public GameObject confirmPanel2;
    public TextMeshProUGUI messageText2;
    public Button okButton2;

    [Header("引用轉盤")]
    public TurnTableManager turnTable;

    [Header("玩家數值")]
    public int playerCoins = 8;

    [Header("外部 UI 控制")]
    // ★★★ 1. 改成陣列 (Array)，可以放無限多個按鈕
    public Button[] buttonsToControl;

    void Start()
    {
        if (okButton2 != null)
        {
            okButton2.onClick.AddListener(CloseConfirm2);
        }
    }

    // ★★★ 新增一個輔助函式：一次開關所有按鈕
    // 傳入 true = 解鎖(可按), 傳入 false = 鎖住(變灰)
    private void SetButtonsInteractable(bool canInteract)
    {
        if (buttonsToControl != null)
        {
            foreach (Button btn in buttonsToControl)
            {
                if (btn != null)
                {
                    btn.interactable = canInteract;
                }
            }
        }
    }

    public void OpenConfirmWindow()
    {
        Time.timeScale = 0f;

        // ★★★ 2. 開啟視窗時 -> 全部鎖住 (False)
        SetButtonsInteractable(false);

        if (GameManager.Instance.CheckRouletteLiver(8))
        {
            confirmPanel.SetActive(true);
            confirmPanel2.SetActive(false);
            messageText.text = "Do you want to spend 8 Livers to spin?";
        }
        else
        {
            confirmPanel.SetActive(false);
            confirmPanel2.SetActive(true);
            messageText2.text = "You don't have enough livers.";
        }
    }

    public void ConfirmAndSpin()
    {
        if (GameManager.Instance.CheckRouletteLiver(8))
        {
            GameManager.Instance.liverCount -= 8;
            GameManager.Instance.UpdateLiverUI();
            confirmPanel.SetActive(false);

            if (turnTable != null)
            {
                turnTable.CreateWheel();
            }
            // 轉盤還在轉，先不恢復按鈕
        }
    }

    public void ShowResultInConfirm2(string rewardMessage)
    {
        confirmPanel.SetActive(false);
        confirmPanel2.SetActive(true);

        TextMeshProUGUI c2Text = confirmPanel2.GetComponentInChildren<TextMeshProUGUI>();
        if (c2Text != null)
        {
            c2Text.text = rewardMessage;
        }
    }

    public void CloseWindow()
    {
        confirmPanel.SetActive(false);
        Time.timeScale = 1f;

        // ★★★ 3. 取消時 -> 全部解鎖 (True)
        SetButtonsInteractable(true);
    }

    public void CloseConfirm2()
    {
        confirmPanel2.SetActive(false);
        Time.timeScale = 1f;

        // ★★★ 4. 結束時 -> 全部解鎖 (True)
        SetButtonsInteractable(true);

        Debug.Log("玩家已確認結果，遊戲恢復運行");
    }
}