using UnityEngine;
using UnityEngine.EventSystems; // ★ 1. 必須引用這個，才能偵測 UI 遮擋

public class LiverItem : MonoBehaviour
{
    [Header("基本數值")]
    public int liverValue = 1;
    public float fallSpeed = 5f;

    [Header("由 SkyManager 控制")]
    public bool isFallingFromSky = false;
    public float stopY = -3.5f;

    // ★ 新增：暫停開關 (配合你的 TurnTableManager 凍結功能)
    private bool isPaused = false;

    private bool isCollected = false;
    private SpriteRenderer sr;

    // ★ 新增：給外部呼叫暫停用 (例如 TurnTableManager)
    public void SetPaused(bool pauseStatus)
    {
        isPaused = pauseStatus;
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            // 強制設定 Sorting Layer 為 Gameplay
            sr.sortingLayerName = "Gameplay";
            // 強制設定 Order 為 999
            sr.sortingOrder = 999;
        }

        // 強制 Z 軸往攝影機靠近
        transform.position = new Vector3(transform.position.x, transform.position.y, -5f);
    }

    void Update()
    {
        // ★ 2. 如果被外部程式 (TurnTableManager) 暫停了，就不執行掉落
        if (isPaused) return;

        // 處理掉落邏輯
        if (isFallingFromSky)
        {
            // 注意：這裡建議維持 deltaTime，因為 Time.timeScale = 0 時 deltaTime 也會是 0，自然會停住
            // 但如果是有特殊的 isPaused 狀態，上面那行會攔截
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

            if (transform.position.y <= stopY)
            {
                isFallingFromSky = false;
                transform.position = new Vector3(transform.position.x, stopY, -5f);
            }
        }
    }

    void OnMouseDown()
    {
        // === ★★★ 關鍵修改開始 ★★★ ===

        // 1. 如果已經被撿過，不做事
        if (isCollected) return;

        // 2. 如果遊戲時間是暫停的 (TimeScale = 0)，代表現在有 PauseMenu 或 轉盤，禁止撿取！
        if (Time.timeScale == 0f) return;

        // 3. 如果這個肝臟被外部腳本標記為暫停 (例如凍結技能)，禁止撿取！
        if (isPaused) return;

        // 4. (選用) 防止穿透 UI 點擊
        // 如果滑鼠指標目前正停留在任何 UI 物件 (例如 PausePanel 的背景) 上，禁止撿取！
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // === ★★★ 關鍵修改結束 ★★★ ===

        isCollected = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddLiver(liverValue);
        }
        Destroy(gameObject);
    }
}