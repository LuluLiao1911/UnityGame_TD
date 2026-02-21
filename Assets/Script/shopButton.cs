using UnityEngine;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour
{
    [Header("商品設定")]
    public GameObject characterPrefab; // 對應的角色 Prefab
    public int cost = 50;              // 價格

    private Button btn;
    private Image btnImage;

    // ★★★ 1. 新增引用變數
    private ConfirmBoxController confirmBox;

    void Start()
    {
        btn = GetComponent<Button>();
        btnImage = GetComponent<Image>();

        // ★★★ 2. 自動在場景中找到 ConfirmBoxController
        // 這樣你就不需要一個一個手動拖曳
        confirmBox = Object.FindFirstObjectByType<ConfirmBoxController>();
    }

    void Update()
    {
        // 防呆
        if (GameManager.Instance == null) return;

        // --- 狀態檢查 ---

        // A. 遊戲是否開始
        bool isGameStarted = GameManager.Instance.isGameActive;

        // B. 錢夠不夠
        bool hasEnoughMoney = GameManager.Instance.liverCount >= cost;

        // C. ★★★ 關鍵修改：檢查是否有任何阻擋視窗開啟 ★★★
        // 判斷依據：
        // 1. 如果 ConfirmBox 存在，且它的任一面板是開著的 -> 視為忙碌中
        // 2. 或者檢查 Time.timeScale (因為你的 ConfirmBox 會暫停時間，這是雙重保險)
        bool isWindowOpen = false;

        if (confirmBox != null)
        {
            // 檢查確認窗1 或 確認窗2 是否開啟
            if (confirmBox.confirmPanel.activeSelf || confirmBox.confirmPanel2.activeSelf)
            {
                isWindowOpen = true;
            }
        }

        // 也可以加上暫停時間的檢查 (這樣連 PauseMenu 開啟時都會自動鎖住)
        if (Time.timeScale == 0f)
        {
            isWindowOpen = true;
        }

        // --- 最終決定按鈕狀態 ---

        // 邏輯：(遊戲已開始) AND (錢夠) AND (沒有視窗擋住)
        if (isGameStarted && hasEnoughMoney && !isWindowOpen)
        {
            // 亮起，可以點
            SetTransparency(1f);
            btn.interactable = true;
        }
        else
        {
            // 變暗，鎖住
            // 這裡會包含：遊戲未開始、錢不夠、或者有視窗開著
            SetTransparency(0.2f);
            btn.interactable = false;
        }
    }

    void SetTransparency(float alpha)
    {
        if (btnImage == null) return;
        Color color = btnImage.color;
        color.a = alpha;
        btnImage.color = color;
    }

    public void OnClick()
    {
        // 再次檢查：如果暫停中或有視窗開啟，不執行
        if (Time.timeScale == 0f) return;
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive) return;

        MapManager mapManager = Object.FindFirstObjectByType<MapManager>();
        if (mapManager != null)
        {
            mapManager.SelectCharacterToBuy(characterPrefab, cost, this);
        }
    }

    public void StartCooldownTimer()
    {
        // 無視冷卻
    }
}