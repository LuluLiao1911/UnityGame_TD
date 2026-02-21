using UnityEngine;
using System.Collections;

public class TurnTableManager : MonoBehaviour
{
    [Header("UI 物件設定")]
    public GameObject turnTableObject;
    public GameObject arrowObject;

    [Header("旋轉參數設定")]
    public float minSpeed = 1000f;
    public float maxSpeed = 1500f;
    public float minFriction = 250f;
    public float maxFriction = 400f;
    [Header("凍結秒數")]
    public float duration = 3.0f;

    private bool isSpinning = false;

    public void CreateWheel()
    {
        if (isSpinning) return;
        Time.timeScale = 0f;

        turnTableObject.SetActive(true);
        arrowObject.SetActive(true);
        arrowObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        StartCoroutine(SpinAndStop(turnTableObject.transform));
    }

    public IEnumerator SpinAndStop(Transform wheelTransform)
    {
        isSpinning = true;
        float currentSpeed = Random.Range(minSpeed, maxSpeed);
        float friction = Random.Range(minFriction, maxFriction);

        while (currentSpeed > 0.1f)
        {
            wheelTransform.Rotate(0, 0, currentSpeed * Time.unscaledDeltaTime);
            currentSpeed -= friction * Time.unscaledDeltaTime;
            yield return null;
        }

        float finalAngle = wheelTransform.eulerAngles.z;
        yield return new WaitForSecondsRealtime(2f);

        turnTableObject.SetActive(false);
        arrowObject.SetActive(false);
        isSpinning = false;

        

        DetermineReward(finalAngle);
    }

    private void DetermineReward(float angle)
    {
        float normalizedAngle = (angle % 360);
        if (normalizedAngle > 315) normalizedAngle -= 360;

        ConfirmBoxController controller = Object.FindFirstObjectByType<ConfirmBoxController>();
        string resultText = "";

        if (normalizedAngle >= -45 && normalizedAngle < 45)
        {
            resultText = "You get: Double Livers!";
            GameManager.Instance.DoubleLiver();
        }
        else if (normalizedAngle >= 45 && normalizedAngle < 135)
        {
            resultText = "You get: Clear All Monsters!";
            ClearAllMonsters();
        }
        else if (normalizedAngle >= 135 && normalizedAngle < 225)
        {
            resultText = "You get: Try Again!";
        }
        else
        {
            resultText = "You get: Monsters Paused for 3s!";
            if (controller != null) controller.ShowResultInConfirm2(resultText);
            StartCoroutine(FreezeRoutine());
            return; // 綠色區域由 FreezeRoutine 處理完畢後結束
        }


        // 紅、藍、咖啡色區域統一顯示結果視窗
        if (controller != null)
        {
            controller.ShowResultInConfirm2(resultText);
        }
    }

    private void ClearAllMonsters()
    {
        ExamPaper[] monsters = Object.FindObjectsByType<ExamPaper>(FindObjectsSortMode.None);
        foreach (ExamPaper m in monsters) if (m != null) Destroy(m.gameObject);
    }

    private IEnumerator FreezeRoutine()
    {
        Debug.Log("準備凍結...等待玩家關閉視窗");

        // 1. 等待直到遊戲時間恢復流動 (代表玩家點了 ConfirmBox 的 Yes/OK)
        // 這裡一定要用 WaitUntil，因為 ConfirmBox 彈出時通常會把 Time.timeScale 設為 0
        yield return new WaitUntil(() => Time.timeScale > 0);

        Debug.Log("視窗已關閉，開始執行凍結！");

        // 2. 搜尋所有 Tag 為 "Enemy" 的物件
        // 請確保你的怪獸 Prefab 的 Tag 已經設為 "Enemy"
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        //  
        // 確保你的 Inspector 中 Tag 下拉選單選的是 Enemy

        // 3. 遍歷每一個怪獸
        foreach (GameObject enemyObj in allEnemies)
        {
            // 4. 抓取 EnemyController 腳本
            EnemyController controller = enemyObj.GetComponent<EnemyController>();

            // 5. 如果抓到了，就呼叫它的 Freeze 功能
            if (controller != null)
            {
                controller.Freeze(duration);
            }
        }

        Debug.Log("全體凍結發動！凍住了 " + allEnemies.Length + " 隻怪獸");
    }



}