using UnityEngine;
using System.Collections;

public class Spinner : MonoBehaviour
{
    public float spinSpeed = 600f;
    private bool isSpinning = false;

    public IEnumerator SpinAndStop(Transform wheel)
    {
        if (isSpinning) yield break;
        isSpinning = true;

        Debug.Log("1. 轉盤開始旋轉");
        float timer = 0f;
        float duration = 3f;

        while (timer < duration)
        {
            wheel.Rotate(0, 0, spinSpeed * Time.unscaledDeltaTime);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        isSpinning = false;
        Debug.Log("2. 旋轉結束，保留轉盤畫面讓玩家看結果");

        // 這裡先儲存角度，但不立刻執行功能
        float finalAngle = wheel.eulerAngles.z;

        // 停留 1 秒讓玩家看指針指在哪裡
        yield return new WaitForSecondsRealtime(1f);

        Debug.Log("3. 轉盤消失並恢復遊戲時間");

        // 先讓轉盤消失
        this.gameObject.SetActive(false);
        // 恢復遊戲速度
        Time.timeScale = 1f;

        // --- 關鍵修改：轉盤消失後，才根據剛才紀錄的角度執行功能 ---
        try
        {
            DetermineResult(finalAngle);
        }
        catch (System.Exception e)
        {
            Debug.LogError("執行功能出錯: " + e.Message);
        }
    }

    private void DetermineResult(float angle)
    {
        float normalizedAngle = angle % 360;
        Debug.Log("最終判定角度：" + normalizedAngle);

        if (normalizedAngle >= 0 && normalizedAngle < 120)
        {
            Debug.Log("轉盤已消失 -> 執行金幣翻倍");
            ApplyDoubleCoins();
        }
        else if (normalizedAngle >= 120 && normalizedAngle < 240)
        {
            Debug.Log("轉盤已消失 -> 執行清空怪物");
            ClearAllMonsters();
        }
        else
        {
            Debug.Log("轉盤已消失 -> 執行怪物靜止");
            FreezeMonsters();
        }
    }

    // --- 功能實作區 (保持你的強力清除邏輯) ---

    void ApplyDoubleCoins()
    {
        ConfirmBoxController controller = FindFirstObjectByType<ConfirmBoxController>();
        if (controller != null)
        {
            controller.playerCoins *= 2;
            Debug.Log("金幣已翻倍！");
        }
    }

    void ClearAllMonsters()
    {
        // 使用類型尋找，確保轉盤消失後怪物才被刪除
        ExamPaper[] monsters = Object.FindObjectsByType<ExamPaper>(FindObjectsSortMode.None);
        Debug.Log("轉盤消失後，偵測到 " + monsters.Length + " 隻怪物並清除");

        foreach (ExamPaper m in monsters)
        {
            if (m != null) Destroy(m.gameObject);
        }
    }

    void FreezeMonsters()
    {
        StartCoroutine(FreezeRoutine());
    }

    IEnumerator FreezeRoutine()
    {
        Debug.Log("轉盤消失，怪物開始靜止...");

        // 因為 Tag 可能有問題，這裡也同步改用 ExamPaper 類型尋找
        ExamPaper[] monsters = Object.FindObjectsByType<ExamPaper>(FindObjectsSortMode.None);
        foreach (ExamPaper m in monsters)
        {
            m.enabled = false;
        }

        ExamSpawner spawner = FindFirstObjectByType<ExamSpawner>();
        if (spawner != null) spawner.enabled = false;

        yield return new WaitForSeconds(3f);

        if (spawner != null) spawner.enabled = true;
        foreach (ExamPaper m in monsters)
        {
            if (m != null) m.enabled = true;
        }
        Debug.Log("怪物靜止結束，恢復行動！");
    }
}
