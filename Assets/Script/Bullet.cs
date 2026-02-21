using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;  // 子彈飛行速度
    public int damage = 1;     // 傷害值

    [Header("邊界設定")]
    [Tooltip("子彈飛超過這個 X 座標就會消失")]
    public float destroyXPosition = 15f; // 預設 15，你可以在 Inspector 隨意改

    void Update()
    {
        // 1. 往右飛
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        // 2. 檢查是否超過你設定的那條線
        //    這裡直接使用你設定的參數 destroyXPosition
        if (transform.position.x > destroyXPosition)
        {
            Destroy(gameObject);
        }
    }

    // 當子彈撞到東西 
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // 扣血
            }
            Destroy(gameObject);
        }
    }

    // ===================================================
    // ★★★ 這是編輯器輔助功能 ★★★
    // 只有在 Unity 編輯器裡會執行，遊戲發布後不會影響效能
    // 當你選取這個物件時，會在場景視窗畫出輔助線
    // ===================================================
    void OnDrawGizmosSelected()
    {
        // 設定輔助線的顏色 (例如紅色，比較顯眼)
        Gizmos.color = Color.red;

        // 畫一條垂直線代表消失的位置
        // 起點：(你的X座標, -10, 0)
        // 終點：(你的X座標,  10, 0)
        // Y 軸從 -10 到 10 只是為了讓線夠長，你可以自己調整
        Vector3 startPoint = new Vector3(destroyXPosition, -10f, 0f);
        Vector3 endPoint = new Vector3(destroyXPosition, 10f, 0f);

        Gizmos.DrawLine(startPoint, endPoint);
    }
}