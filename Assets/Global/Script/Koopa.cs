using UnityEngine;

public class Koopa : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float shellSpeed = 12f;
    public Sprite shellSprite;
    public LayerMask wallLayer;

    [Header("Raycast Settings")]
    public float rayLength = 0.5f;
    public float rayVerticalOffset = 0.5f; // NÂNG CAO LÊN để không đụng sàn
    public float rayForwardOffset = 0.3f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private AnimatedSprite anim;
    private BoxCollider2D col;

    private Vector2 direction = Vector2.left;
    private bool isShell = false;
    private bool isPushed = false;
    
    // BÍ KÍP: Ngăn rùa quay đầu quá nhanh
    private float lastFlipTime; 
    private float flipCooldown = 0.2f; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimatedSprite>();
        col = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (isShell && !isPushed)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // 1. RAYCAST (MẮT THẦN)
        // Bắn tia từ "ngực" rùa thay vì từ "chân"
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * rayVerticalOffset + (direction * rayForwardOffset);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, rayLength, wallLayer);
        
        Debug.DrawRay(rayOrigin, direction * rayLength, Color.red);

        // Chỉ quay đầu nếu thấy tường VÀ đã hết thời gian chờ (cooldown)
        if (hit.collider != null && Time.time > lastFlipTime + flipCooldown)
        {
            Debug.Log("<color=red>[MẮT THẦN]</color> Thấy: " + hit.collider.name);
            Flip();
        }

        // 2. DI CHUYỂN
        float speed = isPushed ? shellSpeed : moveSpeed;
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
    }

    private void Flip()
    {
        direction.x = -direction.x;
        sr.flipX = (direction.x > 0);
        lastFlipTime = Time.time; // Lưu lại thời gian vừa quay đầu
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;

        // Nếu đụng tường thật sự bằng thân mình
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f && Time.time > lastFlipTime + flipCooldown)
            {
                Flip();
                break;
            }
        }
    }

    // Các hàm Stomp, EnterShell, PushShell giữ nguyên của bạn...
    public void Stomp(Transform stomper = null)
    {
        if (!isShell) {
            isShell = true;
            if (anim != null) anim.enabled = false;
            sr.sprite = shellSprite;
            // Thu nhỏ collider
            col.size = new Vector2(col.size.x, 0.5f);
            col.offset = new Vector2(col.offset.x, -0.25f);
        } else if (!isPushed) {
            isPushed = true;
            // Xác định hướng đẩy dựa vào Mario
            if (stomper != null)
                direction.x = (transform.position.x - stomper.position.x) > 0 ? 1 : -1;
        }
    }
}