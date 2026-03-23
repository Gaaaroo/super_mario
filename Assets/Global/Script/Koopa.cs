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
    public float rayVerticalOffset = 0.4f; 
    public float rayForwardOffset = 0.3f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private AnimatedSprite anim;
    private BoxCollider2D col;

    private Vector2 direction = Vector2.left;
    private bool isShell = false;
    private bool isPushed = false;
    private float lastFlipTime;
    private float flipCooldown = 0.2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimatedSprite>();
        col = GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        // TRẠNG THÁI 1: Cái mai đứng yên
        if (isShell && !isPushed)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return; // Đứng im, không chạy Raycast hay di chuyển
        }

        // TRẠNG THÁI 2 & 3: Đang đi bộ HOẶC cái mai đang bị đá bay
        // 1. Raycast quay đầu
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * rayVerticalOffset + (direction * rayForwardOffset);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, rayLength, wallLayer);
        Debug.DrawRay(rayOrigin, direction * rayLength, Color.red);

        if (hit.collider != null && Time.time > lastFlipTime + flipCooldown)
        {
            Flip();
        }

        // 2. Thiết lập vận tốc
        float currentSpeed = isPushed ? shellSpeed : moveSpeed;
        rb.linearVelocity = new Vector2(direction.x * currentSpeed, rb.linearVelocity.y);
    }

    // Mario nhảy lên đầu (Sự kiện tác động)
    public void Stomp(Transform stomper = null)
    {
        if (!isShell)
        {
            // TRƯỜNG HỢP 1: Đang đi bộ -> Biến thành mai đứng yên
            isShell = true;
            isPushed = false;
            if (anim != null) anim.enabled = false;
            sr.sprite = shellSprite;

            col.size = new Vector2(col.size.x, 0.5f);
            col.offset = new Vector2(col.offset.x, -0.25f);

            ResetWakeUpTimer();
            Debug.Log("Rùa thụt đầu!");
        }
        else
        {
            // TRƯỜNG HỢP 2: Đang là mai (dù đứng yên hay đang bay)
            if (isPushed)
            {
                // Nếu đang bay -> DỪNG LẠI
                isPushed = false;
                Debug.Log("Dừng cái mai đang bay!");
            }

            // Cả hai trường hợp (đang đứng im sẵn hoặc vừa bị dừng) 
            // đều phải reset lại 5 giây để chờ tỉnh dậy
            ResetWakeUpTimer();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // --- 1. XỬ LÝ KHI ĐỤNG QUÁI (TAG: ENEMY) ---
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Nếu tôi là cái mai ĐANG BAY
            if (isShell && isPushed)
            {
                // Tìm các script quái để gọi hàm chết của tụi nó
                Goomba goomba = collision.gameObject.GetComponent<Goomba>();
                Koopa otherKoopa = collision.gameObject.GetComponent<Koopa>();

                //if (goomba != null) goomba.Stomp();
                //else if (otherKoopa != null && otherKoopa != this) otherKoopa.Stomp(transform);
                //else
                //{
                //    // Nếu là loại quái lạ khác chưa có script riêng, cứ cho nó biến mất
                //    Destroy(collision.gameObject);
                //}

                Debug.Log("Cái mai đã tiêu diệt một Enemy!");
                return; // Đụng quái thì lướt xuyên qua luôn, không quay đầu
            }
        }

        // --- 2. XỬ LÝ KHI ĐỤNG VẬT THỂ KHÔNG PHẢI PLAYER (TƯỜNG, ỐNG NƯỚC) ---
        if (!collision.gameObject.CompareTag("Player"))
        {
            // Mai đứng yên chờ tỉnh thì không quay đầu khi đụng tường
            if (isShell && !isPushed) return;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.5f && Time.time > lastFlipTime + flipCooldown)
                {
                    Flip();
                    break;
                }
            }
            return;
        }

        // --- 3. XỬ LÝ VA CHẠM VỚI PLAYER (MARIO) ---
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // A. Mario nhảy lên đầu
            // 1. Kiểm tra xem hướng va chạm có phải từ TRÊN XUỐNG không
            // (Vector2.up là hướng lên, nếu kết quả > 0.5 là Mario đang ở trên)
            if (contact.normal.y > 0.5f)
            {
                Stomp(collision.transform);

                // 2. LỰC NẨY: Phải gán TRỰC TIẾP vận tốc cho Mario
                Rigidbody2D rbPlayer = collision.gameObject.GetComponent<Rigidbody2D>();
                if (rbPlayer != null)
                {
                    // Đưa vận tốc Y về 0 trước khi nẩy để lực luôn đều
                    rbPlayer.linearVelocity = new Vector2(rbPlayer.linearVelocity.x, 0f);
                    rbPlayer.linearVelocity = new Vector2(rbPlayer.linearVelocity.x, 10f); // 10 là lực nhún
                }
                return;
            }

            // B. Mario đụng bên hông
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                if (isShell && !isPushed)
                {
                    // Mai đứng im -> Bị đá
                    float pushDir = (transform.position.x - collision.transform.position.x) > 0 ? 1f : -1f;
                    PushShell(new Vector2(pushDir, 0f));
                }
                else if (!isShell || isPushed)
                {
                    // Rùa đang đi bộ HOẶC mai đang bay -> Mario chết
                    Debug.Log("Mario đụng trúng quái và chết!");
                    // Gọi hàm Mario chết ở đây
                }
                return;
            }
        }
    }

    public bool IsShell => isShell;
    public bool IsPushed => isPushed;

    private void ResetWakeUpTimer()
    {
        CancelInvoke("WakeUp"); // Hủy hẹn giờ cũ
        Invoke("WakeUp", 5f);    // Hẹn giờ mới
    }

    private void PushShell(Vector2 pushDirection)
    {
        isPushed = true;
        CancelInvoke("WakeUp"); // Đang chạy thì KHÔNG tỉnh dậy
        direction = pushDirection.normalized;
        Debug.Log("Cái mai bị đá văng! Hủy đếm ngược.");
    }

    //private void WakeUp()
    //{
    //    // Fix: Convert transform.position (Vector3) to Vector2 before adding Vector2
    //    Vector2 boxCenter = (Vector2)transform.position + (Vector2.up * 0.5f);
    //    Collider2D hit = Physics2D.OverlapBox(boxCenter, new Vector2(0.5f, 0.5f), 0, LayerMask.GetMask("Player"));

    //    if (hit != null)
    //    {
    //        Debug.Log("Có người đang đứng trên mai, đợi thêm 5s...");
    //        ResetWakeUpTimer(); // Có người đang đứng trên đầu, đợi tiếp 5s đi
    //        return;
    //    }

    //    if (isShell && !isPushed) 
    //    {
    //        isShell = false;
    //        if (anim != null) anim.enabled = true; 

    //        // Trả lại kích thước ban đầu (nhớ chỉnh số này cho khớp với Koopa của bạn)
    //        col.size = new Vector2(col.size.x, 1.5f);
    //        col.offset = new Vector2(col.offset.x, 0.25f);

    //        Debug.Log("Hết 5s không ai chạm, rùa tỉnh dậy!");
    //    }
    //}

    //// Hàm này chạy liên tục mỗi khi có vật thể chạm vào rùa
    //private void OnCollisionStay2D(Collision2D collision)
    //{

    //    if (collision.gameObject.CompareTag("Enemy"))
    //    {
    //        // Nếu tôi là cái mai ĐANG BAY
    //        if (isShell && isPushed)
    //        {
    //            // Tìm các script quái để gọi hàm chết của tụi nó
    //            Goomba goomba = collision.gameObject.GetComponent<Goomba>();
    //            Koopa otherKoopa = collision.gameObject.GetComponent<Koopa>();

    //            //if (goomba != null) goomba.Stomp();
    //            //else if (otherKoopa != null && otherKoopa != this) otherKoopa.Stomp(transform);
    //            //else
    //            //{
    //            //    // Nếu là loại quái lạ khác chưa có script riêng, cứ cho nó biến mất
    //            //    Destroy(collision.gameObject);
    //            //}

    //            Debug.Log("Cái mai đã tiêu diệt một Enemy!");
    //            return; // Đụng quái thì lướt xuyên qua luôn, không quay đầu
    //        }
    //    }

    //    if (isShell && !isPushed && collision.gameObject.CompareTag("Player"))
    //    {
    //        foreach (ContactPoint2D contact in collision.contacts)
    //        {
    //            // Nếu Mario đang đứng trên đỉnh mai rùa (hướng Y từ trên xuống)
    //            if (contact.normal.y > 0.5f)
    //            {
    //                ResetWakeUpTimer(); // Reset liên tục, rùa sẽ không bao giờ tỉnh khi bạn còn đứng đó
    //                                    // Debug.Log("Mario đang đứng trên mai, chờ thêm 5s...");
    //            }
    //        }
    //    }
    //}

    private void WakeUp()
    {
        // 1. Kiểm tra xem có Mario đang đè lên không (Dùng vị trí thay vì Layer cho chắc)
        // Quét một vùng nhỏ trên đầu con rùa
        Collider2D hit = Physics2D.OverlapBox((Vector2)transform.position + Vector2.up * 0.5f, new Vector2(0.5f, 0.5f), 0);

        if (hit != null && hit.CompareTag("Player"))
        {
            Debug.Log("<color=orange>[WAKEUP]</color> Phát hiện Mario đang đứng trên đầu, chờ thêm 5s...");
            ResetWakeUpTimer();
            return;
        }

        if (isShell && !isPushed)
        {
            isShell = false;
            if (anim != null) anim.enabled = true;

            // Reset Collider (số này bạn chỉnh cho khớp con rùa của bạn nhé)
            col.size = new Vector2(col.size.x, 1.5f);
            col.offset = new Vector2(col.offset.x, 0.25f);

            Debug.Log("<color=green>[WAKEUP]</color> Rùa đã tỉnh dậy!");
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Kiểm tra Player chạm vào
        if (isShell && !isPushed && collision.gameObject.CompareTag("Player"))
        {
            // Dùng so sánh vị trí Y: Nếu chân Mario (collision) cao hơn tâm rùa (transform)
            if (collision.transform.position.y > transform.position.y + 0.2f)
            {
                ResetWakeUpTimer();
                // In log liên tục để bạn thấy nó đang chạy
                Debug.Log("<color=yellow>[STAY]</color> Mario đang đứng trên mai rùa, Reset 5s liên tục...");
            }
        }
    }


    private void Flip()
    {
        direction.x = -direction.x;
        sr.flipX = (direction.x > 0);
        lastFlipTime = Time.time;
    }
}