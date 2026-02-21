using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("1. 網格設定")]
    public int rows = 4;    // 總共有幾排
    public int columns = 4; // 每一排有幾格

    [Header("2. 手動設定 Y (高度)")]
    [Tooltip("Element 0 是最下面，Element 3 是最上面")]
    public float[] rowPositionsY; // Size 必須 = rows (例如 4)

    [Header("3. 手動設定 X (寬度與偏移)")]
    [Tooltip("【寬度】：數字越小，格子靠越近 (Size 必須 = 4)")]
    public float[] rowWidths;

    [Tooltip("【偏移】：0=置中，負數=往左移，正數=往右移 (Size 必須 = 4)")]
    public float[] rowOffsetsX;

    // --- 內部變數 ---
    private GameObject currentGhost;
    private int currentCost;
    private ShopButton currentSourceButton;

    void Update()
    {
        // 抓取滑鼠世界座標
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // ★ 確保滑鼠座標 Z 為 0

        // 1. 找最近格子
        Vector2Int closestGrid = GetClosestGridIndex(mouseWorldPos);
        bool isValidGrid = (closestGrid.x != -1);

        // 2. 計算吸附座標 (GetGridWorldPosition 已確保 Z=0)
        Vector3 snapPos = GetGridWorldPosition(closestGrid.x, closestGrid.y);

        // 鬼魂邏輯
        if (currentGhost != null)
        {
            if (isValidGrid)
            {
                currentGhost.transform.position = snapPos;
                // 透視縮放：越上面 (Row 3) 越小，越下面 (Row 0) 越大
                float t = (float)closestGrid.y / Mathf.Max(1, rows - 1);
                currentGhost.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.85f, t);
            }
            else
            {
                // 超出格子時跟隨滑鼠 (使用 Z=0 的滑鼠座標)
                currentGhost.transform.position = mouseWorldPos;
                currentGhost.transform.localScale = Vector3.one;
            }

            // 左鍵放置
            if (Input.GetMouseButtonDown(0))
            {
                if (isValidGrid) TryPlaceCharacter(snapPos);
                else Debug.Log("無法放置：不在格子內");
            }
            // 右鍵取消
            if (Input.GetMouseButtonDown(1)) CancelPurchase();
        }
    }

    // --- [核心功能 1] 手動陣列計算座標 ---
    Vector3 GetGridWorldPosition(int col, int row)
    {
        // 防呆：確保陣列都有值，避免報錯
        if (rowPositionsY == null || rowPositionsY.Length == 0 ||
            rowWidths == null || rowWidths.Length == 0 ||
            rowOffsetsX == null || rowOffsetsX.Length == 0 ||
            row < 0 || row >= rowPositionsY.Length)
        {
            return Vector3.zero;
        }

        // 1. 讀取高度 Y
        float posY = rowPositionsY[row];

        // 2. 讀取寬度與偏移
        float currentTotalWidth = rowWidths[Mathf.Min(row, rowWidths.Length - 1)];
        float currentOffsetX = rowOffsetsX[Mathf.Min(row, rowOffsetsX.Length - 1)];

        // 3. 計算 X 座標
        float startX = currentOffsetX - (currentTotalWidth / 2f);
        float cellW = currentTotalWidth / columns;
        float posX = startX + (col * cellW) + (cellW * 0.5f);

        // ★ Z 軸回傳 0，這是正確的
        return new Vector3(posX, posY, 0);
    }

    // --- [核心功能 2] 判定與放置 ---
    void TryPlaceCharacter(Vector3 pos)
    {
        // 1. 範圍偵測
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, 0.3f);

        foreach (var hit in hits)
        {
            if (hit.gameObject == currentGhost) continue;

            // ★ 關鍵判定：檢查 Y 軸差距，避免誤判前排或後排
            if (Mathf.Abs(hit.transform.position.y - pos.y) > 0.5f)
            {
                continue;
            }

            Debug.Log("放置失敗：此位置已被佔用");
            return;
        }

        // 2. 扣款與生成
        if (GameManager.Instance.SpendLiver(currentCost))
        {
            SpriteRenderer sr = currentGhost.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color; c.a = 1f; sr.color = c;

                // ★★★ 修正點 1：移除手動計算 Order ★★★
                // 為了讓 Project Settings 的 Y 軸排序生效，
                // 我們不要把 Order 設成 5000 這麼大，改回預設值 (例如 0 或 1)
                // 讓它跟怪獸 (Order 0) 處於同一個比較基準，這樣 Y 軸排序才會正確
                sr.sortingOrder = 0;
            }

            // ★★★ 修正點 2：雙重確認 Z 軸歸零 ★★★
            // 防止因為碰撞或其他物理擠壓導致 Z 軸偏移
            Vector3 finalPos = currentGhost.transform.position;
            finalPos.z = 0f;
            currentGhost.transform.position = finalPos;

            // 啟用所有腳本與碰撞器
            foreach (var script in currentGhost.GetComponents<MonoBehaviour>()) script.enabled = true;
            if (currentGhost.GetComponent<Collider2D>()) currentGhost.GetComponent<Collider2D>().enabled = true;

            // 開始冷卻
            if (currentSourceButton != null) currentSourceButton.StartCooldownTimer();

            // 清空參考，完成放置
            currentGhost = null;
            currentSourceButton = null;
        }
    }

    // --- 找最近格子 ---
    Vector2Int GetClosestGridIndex(Vector3 mousePos)
    {
        if (rowPositionsY == null || rowPositionsY.Length == 0) return new Vector2Int(-1, -1);

        float minDist = 100f;
        Vector2Int bestIndex = new Vector2Int(-1, -1);
        int loopRows = Mathf.Min(rows, rowPositionsY.Length);

        for (int r = 0; r < loopRows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Vector3 gridPos = GetGridWorldPosition(c, r);
                // 忽略 Z 軸差異，只算平面距離
                float dist = Vector2.Distance(new Vector2(mousePos.x, mousePos.y), new Vector2(gridPos.x, gridPos.y));

                if (dist < 0.8f && dist < minDist)
                {
                    minDist = dist;
                    bestIndex = new Vector2Int(c, r);
                }
            }
        }
        return bestIndex;
    }

    public void SelectCharacterToBuy(GameObject prefab, int cost, ShopButton sourceBtn)
    {
        if (currentGhost != null) CancelPurchase();
        currentCost = cost;
        currentSourceButton = sourceBtn;

        // ★★★ 修正點 3：生成時直接指定 Z=0 ★★★
        // 先計算好滑鼠位置，再生成，確保從第一幀開始 Z 就是 0
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        currentGhost = Instantiate(prefab, mousePos, Quaternion.identity);

        var sr = currentGhost.GetComponent<SpriteRenderer>();
        if (sr)
        {
            var c = sr.color; c.a = 0.5f; sr.color = c;
            // 拖曳時可以設高一點，避免被遮住，確認放置時再改回來
            sr.sortingOrder = 100;
        }

        foreach (var s in currentGhost.GetComponents<MonoBehaviour>()) s.enabled = false;
        if (currentGhost.GetComponent<Collider2D>()) currentGhost.GetComponent<Collider2D>().enabled = false;
    }

    void CancelPurchase()
    {
        if (currentGhost != null) { Destroy(currentGhost); currentGhost = null; }
    }

    // --- 視覺化輔助線 ---
    void OnDrawGizmos()
    {
        if (rowPositionsY == null || rowPositionsY.Length == 0 ||
            rowWidths == null || rowWidths.Length == 0) return;

        int loopRows = Mathf.Min(rows, rowPositionsY.Length);

        Gizmos.color = Color.cyan;
        for (int r = 0; r < loopRows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Gizmos.DrawWireSphere(GetGridWorldPosition(c, r), 0.15f);
            }
        }

        Gizmos.color = Color.yellow;
        for (int r = 0; r < loopRows; r++)
        {
            Vector3 start = GetGridWorldPosition(0, r);
            Vector3 end = GetGridWorldPosition(columns - 1, r);
            float cellW = rowWidths[Mathf.Min(r, rowWidths.Length - 1)] / columns;
            Gizmos.DrawLine(start - Vector3.right * cellW * 0.5f, end + Vector3.right * cellW * 0.5f);
        }

        Gizmos.color = Color.green;
        for (int c = 0; c < columns; c++)
        {
            Vector3 bottom = GetGridWorldPosition(c, 0);
            Vector3 top = GetGridWorldPosition(c, loopRows - 1);
            Gizmos.DrawLine(bottom, top);
        }
    }
}