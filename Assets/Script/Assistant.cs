using UnityEngine;

public class Assistant : MonoBehaviour
{
    [Header("凍結持續時間")]
    public float freezeDuration = 3.0f; 

    //碰到碰撞框
    void OnTriggerEnter2D(Collider2D other)
    {
        // Tag==Enemy
        if (other.CompareTag("Enemy"))
        {
            
            EnemyController enemy = other.GetComponent<EnemyController>();

            if (enemy != null)
            {
                // 呼叫怪獸的凍結功能
                enemy.Freeze(freezeDuration);


                // A. 立刻關閉碰撞框 to avoid freeze over one enemy
                GetComponent<Collider2D>().enabled = false;

                // B. 銷毀assistant物件 
                Destroy(gameObject);
            }
        }
    }
}
