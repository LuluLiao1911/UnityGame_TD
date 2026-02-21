using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    [Header("UI 面板")]
    public GameObject pauseMenuPanel; // 只保留暫停選單

    private bool isPaused = false;

    void Start()
    {
        // 1. 確保遊戲時間是流動的
        Time.timeScale = 1f;

        // 2. 確保暫停選單一開始是關閉的
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Debug.Log("遊戲直接開始！");
    }

    void Update()
    {
        // 監聽 ESC 鍵來暫停/繼續
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    // --- 暫停功能 ---
    public void PauseGame()
    {
        isPaused = true;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // 暫停時間
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // 恢復時間
    }

    // --- 場景切換功能 ---
    public void RestartGame()
    {
        Time.timeScale = 1f; // 重來前一定要把時間恢復
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // [修改] 原本是回到主選單，現在改成直接關閉遊戲程式
    public void EndGame()
    {
        Debug.Log("正在關閉遊戲程式..."); // 在編輯器裡看Log確認有沒有執行

        // 這是真正關閉應用程式的指令 (只在打包後的 .exe / .app 有效)
        Application.Quit();

        // 如果你在 Unity 編輯器裡面測試，這行指令可以讓你停止播放模式
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}