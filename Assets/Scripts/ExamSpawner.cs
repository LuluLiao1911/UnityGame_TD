using UnityEngine;
using System.Collections;

public class ExamSpawner : MonoBehaviour
{
    [Header("怪物設定")]
    public GameObject[] examPrefabs;
    public float minInterval = 1.0f;
    public float maxInterval = 2.0f;
    public Transform[] spawnPoints;

    [Header("生成延遲設定")]
    [Tooltip("遊戲正式開始後，要再等幾秒才生第一隻怪？")]
    public float startDelay = 5.0f; // ★ 新增：開場緩衝時間

    [Header("UI 引用")]
    public MessageBoxController messageBoxController;

    [HideInInspector] public bool isPaused = false;

    // ★ 新增：內部開關，只有初始化完成後才變成 true
    private bool isSpawningStarted = false;

    private float timer;
    private float currentInterval;
    private int waveCount = 0;
    private bool isGameOver = false;

    // ★★★ 修改重點 1：把 Start 改成 IEnumerator (協程) ★★★
    // 這樣我們才能在裡面使用「等待」指令
    IEnumerator Start()
    {
        // 1. 等待 GameManager 正式開始遊戲 (等待 3-2-1 倒數結束)
        // 這行會一直卡住，直到 isGameActive 變成 true
        if (GameManager.Instance != null)
        {
            yield return new WaitUntil(() => GameManager.Instance.isGameActive);
        }

        // 2. ★★★ 這裡就是你要的「暫停 3 秒」 ★★★
        // 倒數結束後，再多等 startDelay 秒，讓玩家準備一下
        yield return new WaitForSeconds(startDelay);

        // 3. 正式啟動生成器
        isSpawningStarted = true;

        // 4. 設定第一次生成的間隔
        currentInterval = Random.Range(minInterval, maxInterval);

        // 5. 啟動後續的定時事件 (Midterm, GameEnd)
        // 注意：我們改在這裡 Invoke，這樣計時才會準確
        Invoke("StartTheWave", 37f);
        Invoke("EndGame", 90f);
    }

    void Update()
    {
        // ★★★ 修改重點 2：多加一個檢查 isSpawningStarted ★★★
        // 如果遊戲結束、暫停中、或是「還沒初始化完成」，就不準計時
        if (isGameOver || isPaused || !isSpawningStarted) return;

        timer += Time.deltaTime;

        if (timer >= currentInterval)
        {
            Spawn();
            timer = 0;
            currentInterval = Random.Range(minInterval, maxInterval);
        }
    }

    void StartTheWave()
    {
        if (isGameOver) return;

        waveCount++;
        if (messageBoxController != null)
        {
            if (waveCount == 1) messageBoxController.ShowMessage("Midterm Exam is coming!");
            else if (waveCount == 2) messageBoxController.ShowMessage("Final Exam is coming!");
        }

        StartCoroutine(SpawnWave(15));

        if (waveCount == 1)
        {
            Invoke("StartTheWave", 30f);
        }
    }

    void EndGame()
    {
        isGameOver = true;
        StopAllCoroutines();

        ExamPaper[] allMonsters = Object.FindObjectsByType<ExamPaper>(FindObjectsSortMode.None);
        foreach (ExamPaper m in allMonsters)
        {
            if (m != null) Destroy(m.gameObject);
        }
    }

    void Spawn()
    {
        // 1. 雙重保險檢查
        if (isGameOver || isPaused || !isSpawningStarted) return;

        // 2. 確保陣列與生成點有資料
        if (examPrefabs != null && examPrefabs.Length > 0 && spawnPoints.Length > 0)
        {
            // A. 隨機選一個生成點
            int PointIndex = Random.Range(0, spawnPoints.Length);

            // B. 隨機選一種怪物 (依照你的機率邏輯)
            int selectedMonsterIndex = 0;
            int chance = Random.Range(1, 101);

            if (chance <= 50) selectedMonsterIndex = 0;      // 50% 機率
            else if (chance <= 85) selectedMonsterIndex = 1; // 35% 機率
            else selectedMonsterIndex = 2;                   // 15% 機率 (剩餘的)

            // 防呆：避免算出來的 index 超過你實際放的怪物數量 (防止報錯)
            if (selectedMonsterIndex >= examPrefabs.Length)
            {
                selectedMonsterIndex = 0;
            }

            // C. ★★★ 生成怪物，並用變數 newEnemy 接住它 ★★★
            GameObject newEnemy = Instantiate(examPrefabs[selectedMonsterIndex], spawnPoints[PointIndex].position, Quaternion.identity);

            // D. ★★★ 強制校正 Z 軸 (解決 Layer 覆蓋問題的關鍵) ★★★
            // 1. 取得它現在的位置
            Vector3 fixedPos = newEnemy.transform.position;
            // 2. 把 Z 軸強制設為 0
            fixedPos.z = 0f;
            // 3. 寫回物件
            newEnemy.transform.position = fixedPos;
        }
    }

    IEnumerator SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 暫停時等待
            while (isPaused || !isSpawningStarted)
            {
                yield return null;
            }

            Spawn();
            yield return new WaitForSeconds(0.15f);
        }
    }
}
