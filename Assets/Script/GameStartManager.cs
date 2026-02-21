using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameStartManager : MonoBehaviour
{
    [Header("UI 設定 (倒數畫面)")]
    [Tooltip("用來顯示 3, 2, 1 圖片的 Image")]
    public Image countdownDisplay;

    [Tooltip("倒數圖片上的 Canvas Group (用來做淡出效果)")]
    public CanvasGroup countdownCanvasGroup;

    [Header("UI 設定 (遊戲介面總開關)")]
    // ★★★ 關鍵功能：倒數時鎖住所有按鈕 ★★★
    [Tooltip("請把包住 商店按鈕、轉盤按鈕 的那個 Panel (需掛有 CanvasGroup) 拖進來")]
    public CanvasGroup gameplayUIGroup;

    [Header("暫停選單 (防偷跑用)")]
    [Tooltip("請拖入 PauseMenu 的 Panel 物件")]
    public GameObject pauseMenuPanel;

    [Header("倒數素材")]
    [Tooltip("請依照 3 -> 2 -> 1 -> Start 的順序拖入 Sprite")]
    public Sprite[] countdownSprites;

    [Header("參數設定")]
    public float startDelay = 0.5f;
    public float numberDuration = 1.0f;

    void Start()
    {
        // 1. 遊戲一開始，先把所有遊戲按鈕鎖起來 (變灰、不能按)
        // 這樣玩家就不能在倒數時偷買東西或轉轉盤
        if (gameplayUIGroup != null)
        {
            gameplayUIGroup.interactable = false;
        }

        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        // 2. 初始暫停 (確保時間是停止的)
        Time.timeScale = 0f;

        // 先隱藏圖片，避免畫面一開始就卡著一個數字
        countdownDisplay.gameObject.SetActive(false);

        // 3. 【緩衝時間】(使用支援暫停的手動計時)
        yield return StartCoroutine(WaitWhilePausedOrCounting(startDelay));

        countdownDisplay.gameObject.SetActive(true);

        // 4. 【倒數迴圈】
        foreach (Sprite s in countdownSprites)
        {
            // A. 換圖
            countdownDisplay.sprite = s;
            countdownDisplay.SetNativeSize();

            // B. 播放動畫 (動畫內部也支援暫停檢測)
            // 這裡會等待動畫播完，才會進入下一張圖
            yield return StartCoroutine(PlayPopAnimation(numberDuration));
        }

        // 5. 【倒數結束】
        countdownDisplay.gameObject.SetActive(false);

        // ★★★ 安全檢查：防止玩家在倒數最後一刻按下 ESC ★★★
        // 如果倒數剛好結束的瞬間，玩家按下了 ESC，這裡要卡住，直到玩家關閉暫停選單
        while (IsPaused())
        {
            yield return null;
        }

        // 6. 【解鎖按鈕】
        // 倒數結束了，允許玩家點擊商店和轉盤
        if (gameplayUIGroup != null)
        {
            gameplayUIGroup.interactable = true;
        }

        // 7. 【正式啟動遊戲】
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGameLogic();
        }
        else
        {
            // 如果沒有 GameManager (測試用)，直接恢復時間
            Time.timeScale = 1f;
        }
    }

    // --- 輔助函式區 ---

    // 判斷是否暫停 (看 PausePanel 有沒有開)
    bool IsPaused()
    {
        return pauseMenuPanel != null && pauseMenuPanel.activeSelf;
    }

    // 支援暫停的等待函式 (取代 WaitForSecondsRealtime)
    IEnumerator WaitWhilePausedOrCounting(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            // 如果暫停選單開著，就空轉，不增加 timer
            if (IsPaused())
            {
                yield return null;
            }
            else
            {
                // 只有沒暫停的時候，才增加計時
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }

    // 支援暫停的動畫協程
    IEnumerator PlayPopAnimation(float duration)
    {
        float timer = 0;

        Vector3 startScale = Vector3.one * 0.5f; // 從 0.5倍大
        Vector3 midScale = Vector3.one * 1.2f;   // 彈到 1.2倍大
        Vector3 endScale = Vector3.one;          // 回到 1倍大

        // 確保一開始是不透明的
        if (countdownCanvasGroup != null) countdownCanvasGroup.alpha = 1f;

        while (timer < duration)
        {
            // ★ 關鍵：如果是暫停狀態，就卡在這裡，畫面定格
            if (IsPaused())
            {
                yield return null;
                continue;
            }

            timer += Time.unscaledDeltaTime;
            float progress = timer / duration;

            // --- 縮放效果 ---
            if (progress < 0.2f)
                countdownDisplay.transform.localScale = Vector3.Lerp(startScale, midScale, progress * 5);
            else
                countdownDisplay.transform.localScale = Vector3.Lerp(midScale, endScale, (progress - 0.2f) * 1.25f);

            // --- 淡出效果 (最後 30% 時間) ---
            if (countdownCanvasGroup != null && progress > 0.7f)
            {
                countdownCanvasGroup.alpha = 1 - ((progress - 0.7f) / 0.3f);
            }

            yield return null;
        }

        // 動畫結束，重置數值
        countdownDisplay.transform.localScale = endScale;
        if (countdownCanvasGroup != null) countdownCanvasGroup.alpha = 0f;
    }
}