using System.Collections;
using UnityEngine;

public class StudentShooter : MonoBehaviour
{
    public Transform firePoint;
    [Header("武器設定")]
    public GameObject bulletPrefab; // 記得把子彈 Prefab 拖進來
    public float fireInterval = 2.0f; // 幾秒開一次槍

    [Header("攻擊模式 (需求二)")]
    public int bulletsPerShot = 1; // 一次發幾顆？ (1 或 2)
    public float burstDelay = 0.2f; // 如果發兩顆，中間間隔幾秒

    // 射線偵測層 (只偵測 Enemy Layer)
    public LayerMask enemyLayer;

    void Start()
    {
        // 開始週期性攻擊
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            // 1. 偵測前方有沒有敵人 (懶人法：直接射，或者用 Raycast)
            // 這裡簡單做：直接定時發射

            // 2. 執行發射邏輯
            for (int i = 0; i < bulletsPerShot; i++)
            {
                Shoot();
                // 如果要連發多顆，中間暫停一下
                if (bulletsPerShot > 1)
                    yield return new WaitForSeconds(burstDelay);
            }

            // 3. 等待冷卻時間
            yield return new WaitForSeconds(fireInterval);
        }
    }

    void Shoot()
    {
        // 2. 判斷邏輯：如果有設定 FirePoint，就用 FirePoint 的位置
        // 如果忘記設，就還是用腳底 (transform.position) 避免報錯
        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position;

        // 3. 生成子彈
        Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
    }
}