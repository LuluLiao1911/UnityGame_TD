using UnityEngine;
using System.Collections;

public class SkyManager : MonoBehaviour
{
    [Header("生成設定")]
    public GameObject liverPrefab; 
    public float minTime = 3.1f; // 最快幾秒掉一個
    public float maxTime = 7f; // 最慢幾秒掉一個

    [Header("高度控制 (請看 Scene 的線)")]
    public float startY = 7f;   // 綠線：生成高度 (要大於 5.4 才會在螢幕外)
    public float stopY = -3f;   // 紅線：停止高度 (肝會掉到這裡停住)

    [Header("左右範圍")]
    public float xRangeLeft = -8f;
    public float xRangeRight = 8f;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minTime, maxTime);
            yield return new WaitForSeconds(waitTime);

            // 1. 決定生成位置 (高度用 startY)
            float randomX = Random.Range(xRangeLeft, xRangeRight);
            Vector3 spawnPos = new Vector3(randomX, startY, 0);

            // 2. 生成肝
            GameObject newLiver = Instantiate(liverPrefab, spawnPos, Quaternion.identity);
            
            // 3. 設定這個肝的數值 
            // [修改] 這裡要改成抓取 "LiverItem" 而不是 "Liver"
            LiverItem liverScript = newLiver.GetComponent<LiverItem>();
            
            if (liverScript != null)
            {
                // 告訴它：你是從天上掉下來的，要往下跑！
                liverScript.isFallingFromSky = true;
                
                // 設定停止高度 (加上一點隨機浮動，讓地上的肝看起來比較自然)
                liverScript.stopY = stopY + Random.Range(-0.5f, 0.5f); 
            }
        }
    }

    // 視覺輔助線
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(xRangeLeft, startY, 0), new Vector3(xRangeRight, startY, 0));
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(xRangeLeft, stopY, 0), new Vector3(xRangeRight, stopY, 0));

        Gizmos.color = Color.white;
        Gizmos.DrawLine(new Vector3(xRangeLeft, startY, 0), new Vector3(xRangeLeft, stopY, 0));
        Gizmos.DrawLine(new Vector3(xRangeRight, startY, 0), new Vector3(xRangeRight, stopY, 0));
    }
}