using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    // === 單例模式 ===
    public static GameManager Instance;

    [Header("UI 設定 (輸掉的畫面)")]
    public GameObject gameOverPanel;
    public Text timeText;

    [Header("UI 設定 (勝利的畫面)")]
    public GameObject winPanel;
    public Text winTimeText;

    [Header("UI 設定 (用來檢查是否要保持暫停)")]
    [Tooltip("請拖入 PauseMenu 的 Panel")]
    public GameObject pausePanel;
    [Tooltip("請拖入 TurnTableCanvas 下的 TurnTable 物件")]
    public GameObject turnTableObject;
    [Tooltip("請拖入 ConfirmBox 的 Panel 1")]
    public GameObject confirmPanel;
    [Tooltip("請拖入 ConfirmBox 的 Panel 2")]
    public GameObject confirmPanel2;

    [Header("UI 設定 (時間進度條)")]
    public Slider timeSlider;
    public GameObject markerPrefab;
    public RectTransform markerContainer;
    // ★★★ 設定波次標記時間 (配合 ExamSpawner) ★★★
    public float[] waveTimes = new float[] { 37f, 67f };

    [Header("遊戲設定")]
    // ★★★ 設定遊戲總時長 ★★★
    public float targetTime = 90f;

    private bool isGameOver = false;

    // ★★★ 關鍵變數：遊戲是否正式開始 ★★★
    // 預設為 false，只有等倒數完畢才會變 true
    public bool isGameActive = false;

    private float gameTimer = 0f;

    [Header("肝臟經濟系統")]
    public int liverCount = 100;
    public TextMeshProUGUI liverText;

    [Header("音樂音效設定")]
    public AudioSource bgmAudioSource; // 請在 Inspector 拖入一個掛有 AudioSource 的物件
    public AudioClip bgmClip;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        UpdateLiverUI();
    }

    void Start()
    {
        // 1. 初始化數值
        gameTimer = 0f;
        isGameOver = false;

        // 2. 初始化 UI
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        UpdateLiverUI();
        if (timeSlider != null) timeSlider.value = 1f;
        SpawnTimeMarkers();

        // 3. ★★★ 關鍵邏輯：決定是否暫停遊戲等待倒數 ★★★

        // 檢查場景有沒有倒數管理器 (GameStartManager)
        if (Object.FindFirstObjectByType<GameStartManager>() != null)
        {
            // A. 有倒數：先鎖住遊戲，並暫停時間
            isGameActive = false;
            Time.timeScale = 0f; // 強制暫停時間
        }
        else
        {
            // B. 沒倒數：直接開始遊戲
            isGameActive = true;
            Time.timeScale = 1f;
        }

        // 4. 播放背景音樂
        PlayBGM();
    }

    void Update()
    {
        if (isGameOver) return;

        // 這裡原本的檢查可能擋不住 "半死不活" 的物件
        if (timeSlider == null || timeSlider.gameObject == null) return;

        if (isGameActive && Time.timeScale > 0f)
        {
            gameTimer += Time.deltaTime;

            // ★★★ 修改這裡：加上 try-catch 保護 ★★★
            try
            {
                if (timeSlider != null)
                {
                    float progress = 1f - (gameTimer / targetTime);
                    timeSlider.value = Mathf.Clamp01(progress);
                }
            }
            catch (MissingReferenceException)
            {
                // 如果抓到這個錯誤，代表 Slider 已經壞了，我們就放棄更新它，甚至可以把參照清空
                Debug.LogWarning("TimeSlider 遺失或被銷毀，停止更新 UI");
                timeSlider = null;
                return;
            }

            // 檢查勝利
            if (gameTimer >= targetTime)
            {
                WinGame();
            }
        }
    }

    // === ★★★ 核心邏輯：倒數結束後呼叫 ★★★ ===
    public void StartGameLogic()
    {
        // 1. 標記遊戲已激活 (這會讓 ShopButton 亮起來)
        isGameActive = true;

        // 2. 檢查現在是否有任何會擋住畫面的 UI 開著
        // 如果有，我們就不應該恢復時間，要讓它繼續暫停
        bool isAnyMenuOpen = false;

        if (pausePanel != null && pausePanel.activeSelf) isAnyMenuOpen = true;
        if (turnTableObject != null && turnTableObject.activeSelf) isAnyMenuOpen = true;
        if (confirmPanel != null && confirmPanel.activeSelf) isAnyMenuOpen = true;
        if (confirmPanel2 != null && confirmPanel2.activeSelf) isAnyMenuOpen = true;

        if (isAnyMenuOpen)
        {
            // 如果有視窗開著，保持時間暫停！
            Time.timeScale = 0f;
            Debug.Log("倒數結束，但因為有視窗 (暫停/轉盤) 開著，保持靜止。");
        }
        else
        {
            // 都沒視窗開著，才恢復時間
            Time.timeScale = 1f;
            Debug.Log("倒數結束，遊戲正式開始！");
        }
    }

    // === 音樂功能 ===
    public void PlayBGM()
    {
        if (bgmAudioSource != null && bgmClip != null)
        {
            bgmAudioSource.clip = bgmClip;
            bgmAudioSource.loop = true; // 設定循環
            bgmAudioSource.volume = 0.5f; // 音量 (0~1)
            bgmAudioSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
    }

    // === 肝臟經濟功能 ===
    public void AddLiver(int amount)
    {
        liverCount += amount;
        UpdateLiverUI();
    }

    public bool SpendLiver(int cost)
    {
        // 如果遊戲還沒開始 (isGameActive = false)，禁止購買
        if (!isGameActive)
        {
            Debug.Log("遊戲尚未開始，無法購買！");
            return false;
        }

        if (liverCount >= cost)
        {
            liverCount -= cost;
            UpdateLiverUI();
            return true;
        }
        return false;
    }

    public bool CheckRouletteLiver(int amount)
    {
        return liverCount >= amount;
    }

    public void DoubleLiver()
    {
        liverCount *= 2;
        UpdateLiverUI();
    }

    public void UpdateLiverUI()
    {
        if (liverText != null)
            liverText.text = liverCount.ToString();
    }

    // === 輔助功能 (畫進度條上的線) ===
    void SpawnTimeMarkers()
    {
        if (markerPrefab == null || markerContainer == null) return;

        // 清除舊的標記 (防止 PlayAgain 重複生成)
        foreach (Transform child in markerContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (float waveTime in waveTimes)
        {
            float ratio = 1f - (waveTime / targetTime);
            GameObject line = Instantiate(markerPrefab, markerContainer);
            RectTransform rect = line.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(ratio, 0);
            rect.anchorMax = new Vector2(ratio, 1);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(3f, 0);
        }
    }

    // === 勝負邏輯 ===
    public void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        isGameActive = false; // 鎖住遊戲狀態

        StopBGM();
        Debug.Log("You Win!");
        Time.timeScale = 0f;

        if (timeSlider != null) timeSlider.value = 0f;
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (winTimeText != null) winTimeText.text = "挑戰成功！堅持了 " + targetTime + " 秒";
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        isGameActive = false; // 鎖住遊戲狀態

        Debug.Log("Game Over!");
        Time.timeScale = 0f;
        StopBGM();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (timeText != null) timeText.text = "存活時間: " + gameTimer.ToString("F2") + " 秒";
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}