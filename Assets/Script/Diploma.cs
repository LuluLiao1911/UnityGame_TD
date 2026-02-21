using UnityEngine;

public class Diploma : MonoBehaviour
{
    // 碰到碰撞框
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("畢業證書被吃掉了！");

            GameManager.Instance.GameOver();
        }
    }
}