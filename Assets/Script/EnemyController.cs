using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("數值設定")]
    public float moveSpeed = 1.0f;
    public int maxHealth = 5;

    [Header("冰凍圖片設定")]
    public Sprite frozenSprite;
    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;

    public bool isFrozen = false;
    private Vector3 frozenPosition;
    private Coroutine currentFreezeCoroutine;

    [Header("受傷特效設定")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;
    private Coroutine currentFlashRoutine;
    private int currentHealth;

    private Animator anim;
    private Rigidbody2D rb;

    // ★ 新增：原本的 Body Type，解凍時要設回來
    private RigidbodyType2D originalBodyType;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalSprite = spriteRenderer.sprite;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // 記住一開始的物理狀態 (通常是 Dynamic 或 Kinematic)
        if (rb != null) originalBodyType = rb.bodyType;
    }

    void Update()
    {
        // 冰凍時強制鎖定位置
        if (isFrozen)
        {
            transform.position = frozenPosition;
            return;
        }

        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Student"))
        {
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // 受傷時，不管有沒有被冰凍，都要閃紅光
        FlashEffect();

        if (currentHealth <= 0) Die();
    }

    void Die() { Destroy(gameObject); }

    void FlashEffect()
    {
        if (currentFlashRoutine != null) StopCoroutine(currentFlashRoutine);
        currentFlashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        // 1. 變紅
        if (spriteRenderer != null) spriteRenderer.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        // 2. 變回來 (如果是冰凍狀態，就變回白色，但圖片還是 frozenSprite)
        // 這樣可以確保受傷特效結束後，還是看得出是冰凍狀態
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    public void Freeze(float duration)
    {
        if (currentFreezeCoroutine != null) StopCoroutine(currentFreezeCoroutine);
        currentFreezeCoroutine = StartCoroutine(FreezeRoutine(duration));
    }

    IEnumerator FreezeRoutine(float duration)
    {
        frozenPosition = transform.position;
        isFrozen = true;

        // 1. 暫停動畫
        if (anim != null) anim.enabled = false;

        // 2. ★★★ 關鍵修改：不要關閉 simulated，改為 Kinematic ★★★
        // 這樣碰撞箱還在，可以被打到
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // 速度歸零
            rb.bodyType = RigidbodyType2D.Kinematic; // 變成不受力狀態，但還有碰撞
            rb.constraints = RigidbodyConstraints2D.FreezeAll; // 鎖死旋轉和位移
        }

        // 3. 換冰凍圖
        if (spriteRenderer != null && frozenSprite != null)
        {
            spriteRenderer.sprite = frozenSprite;
        }

        // === 等待 ===
        yield return new WaitForSeconds(duration);

        // === 解凍 ===
        isFrozen = false;

        // 1. 恢復動畫
        if (anim != null) anim.enabled = true;

        // 2. ★★★ 關鍵修改：恢復物理狀態 ★★★
        if (rb != null)
        {
            rb.bodyType = originalBodyType; // 變回原本的 (Dynamic)
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 通常只鎖旋轉
            rb.linearVelocity = Vector2.zero;
        }

        // 3. 換回原圖
        if (spriteRenderer != null && originalSprite != null)
        {
            spriteRenderer.sprite = originalSprite;
        }

        currentFreezeCoroutine = null;
    }
}