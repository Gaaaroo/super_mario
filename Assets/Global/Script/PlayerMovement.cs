//using UnityEngine;

//[RequireComponent(typeof(Rigidbody2D))]
//public class PlayerMovement : MonoBehaviour
//{
//    private new Camera camera;
//    private new Rigidbody2D rigidbody;

//    private Vector2 velocity;
//    private float inputAxis;

//    public float moveSpeed = 8f;
//    public float maxJumpHeight = 5f;
//    public float maxJumpTime = 1f;

//    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
//    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow((maxJumpTime / 2f), 2);

//    public bool grounded { get; private set; }
//    public bool jumping { get; private set; }
//    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
//    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);

//    private void Awake()
//    {
//        rigidbody = GetComponent<Rigidbody2D>();
//        camera = Camera.main;

//        // Đặt vị trí spawn theo checkpoint (nếu có), nếu không thì giữ vị trí ban đầu trong scene
//        Vector2 spawnPosition = CheckpointManager.GetSpawnPosition(rigidbody.position);
//        rigidbody.position = spawnPosition;
//        transform.position = spawnPosition;
//    }

//    // Update is called once per frame
//    private void Update()
//    {
//        HorizontalMovement();

//        grounded = rigidbody.Raycast(Vector2.down);

//        if (grounded)
//            GroundedMovement();

//        ApplyGravity();
//    }

//    private void HorizontalMovement()
//    {
//        inputAxis = Input.GetAxis("Horizontal");
//        velocity.x = inputAxis * moveSpeed;

//        if (rigidbody.Raycast(Vector2.right * velocity.x))
//            velocity.x = 0f;

//        if (velocity.x > 0f)
//            transform.eulerAngles = Vector3.zero;
//        else if (velocity.x < 0f)
//            transform.eulerAngles = new Vector3(0f, 180f, 0f);
//    }

//    private void GroundedMovement()
//    {
//        velocity.y = Mathf.Max(velocity.y, 0f);
//        jumping = velocity.y > 0f;

//        if (Input.GetButtonDown("Jump"))
//        {
//            velocity.y = jumpForce;
//            jumping = true;
//        }
//    }

//    private void ApplyGravity()
//    {
//        bool falling = velocity.y < 0f || !Input.GetButton("Jump");
//        float multiplier = falling ? 2f : 1f;

//        velocity.y += gravity * multiplier * Time.deltaTime;
//        velocity.y = Mathf.Max(velocity.y, gravity / 2f);
//    }

//    private void FixedUpdate()
//    {
//        Vector2 position = rigidbody.position;
//        position += velocity * Time.fixedDeltaTime;

//        Vector2 leftEdge = camera.ScreenToWorldPoint(Vector2.zero);
//        Vector2 rightEdge = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
//        position.x = Mathf.Clamp(position.x, leftEdge.x + 0.5f, rightEdge.x - 0.5f);

//        rigidbody.MovePosition(position);
//    }

//    private void OnCollisionEnter2D(Collision2D collision)
//    {
//        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
//            if (transform.DotTest(collision.transform, Vector2.down))
//            {
//                // Bounce the player up when they jump on an enemy
//                velocity.y = jumpForce / 2f;
//                jumping = true;
//            }
//        else if (collision.gameObject.layer != LayerMask.NameToLayer("PowerUp"))
//            if (transform.DotTest(collision.transform, Vector2.up))
//                velocity.y = 0f;
//    }


//    private void HitByEnemy()
//    {
//        // Chết bởi enemy: reload scene, Player sẽ spawn lại ở checkpoint (nếu có)
//        UnityEngine.SceneManagement.SceneManager.LoadScene(
//            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
//        );
//    }

//    public void RespawnAt(Vector2 position)
//    {
//        velocity = Vector2.zero;
//        rigidbody.position = position;
//        rigidbody.linearVelocity = Vector2.zero;
//    }
//}


using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private new Camera camera;
    private new Rigidbody2D rigidbody;
    private Vector2 defaultSpawnPosition;

    private Vector2 velocity;
    private float inputAxis;

    [Header("Input responsiveness")]
    [Tooltip("Thời gian giữ lệnh nhảy sau khi nhấn (để nhảy vẫn ăn khi vừa chạm đất).")]
    public float jumpBufferTime = 0.12f;
    [Tooltip("Cho phép nhảy trong thời gian ngắn sau khi rời mặt đất (coyote time).")]
    public float coyoteTime = 0.1f;

    private float jumpBufferTimer;
    private float coyoteTimer;

    public float moveSpeed = 8f;
    public float maxJumpHeight = 5f;
    public float maxJumpTime = 1f;

    [Header("Âm thanh")]
    public AudioClip jumpSound;
    private AudioSource audioSource;

    [Header("Bất Tử")]
    public bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow((maxJumpTime / 2f), 2);

    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);

    /// <summary>velocity.y hiện tại — trùng logic stomp enemy (<see cref="OnCollisionEnter2D"/> dùng velocity.y &lt;= 0).</summary>
    public float VerticalMoveSpeed => velocity.y;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        camera = Camera.main;
        defaultSpawnPosition = rigidbody.position;

        Vector2 spawnPosition = CheckpointManager.GetSpawnPosition(defaultSpawnPosition);
        rigidbody.position = spawnPosition;
        transform.position = spawnPosition;

        audioSource = GetComponent<AudioSource>();

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Đọc input mỗi frame để không bỏ lỡ (GetAxisRaw = phản hồi ngay, không làm mượt)
        inputAxis = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;

        grounded = rigidbody.Raycast(Vector2.down);
        if (grounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        jumpBufferTimer -= Time.deltaTime;

        // Xoay sprite theo hướng di chuyển
        if (inputAxis > 0f)
            transform.eulerAngles = Vector3.zero;
        else if (inputAxis < 0f)
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
    }

    private void FixedUpdate()
    {
        bool groundedThisStep = rigidbody.Raycast(Vector2.down);
        if (groundedThisStep)
            velocity.y = Mathf.Max(velocity.y, 0f);

        jumping = velocity.y > 0f;

        // Nhảy: dùng buffer + coyote để nhấn không bị mất
        bool canJump = (groundedThisStep || coyoteTimer > 0f) && jumpBufferTimer > 0f;
        if (canJump)
        {
            velocity.y = jumpForce;
            jumping = true;
            jumpBufferTimer = 0f;

            if (jumpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }

        // Ngang: áp dụng input ngay
        velocity.x = inputAxis * moveSpeed;

        // Gravity
        bool falling = velocity.y < 0f || !Input.GetButton("Jump");
        float multiplier = falling ? 2f : 1f;
        velocity.y += gravity * multiplier * Time.fixedDeltaTime;
        velocity.y = Mathf.Max(velocity.y, gravity / 2f);

        // Giới hạn trong camera (không đi ra ngoài màn hình)
        Vector2 leftEdge = camera.ScreenToWorldPoint(Vector2.zero);
        Vector2 rightEdge = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        if (rigidbody.position.x <= leftEdge.x + 0.5f && velocity.x < 0f)
            velocity.x = 0f;
        if (rigidbody.position.x >= rightEdge.x - 0.5f && velocity.x > 0f)
            velocity.x = 0f;

        // Dùng linearVelocity thay MovePosition: Dynamic body luôn phản hồi, không bị ngủ
        rigidbody.linearVelocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsStomped)
        {
            if (transform.DotTest(collision.transform, Vector2.down) && velocity.y <= 0f)
            {
                velocity.y = jumpForce / 2f; // bounce
                enemy.Stomp();
            }
            else
            {
                HitByEnemy();
            }
            return;
        }

        // Kiểm tra nếu đụng trúng con Rùa (Koopa)
        Koopa koopa = collision.gameObject.GetComponent<Koopa>();

        if (koopa != null)
        {
            // Kiểm tra hướng va chạm bằng "Normal"
            // Nếu normal.y > 0.5 nghĩa là Mario đang nằm TRÊN đầu con rùa
            if (collision.contacts[0].normal.y > 0.5f)
            {
                koopa.Stomp(transform); // Gọi hàm giẫm bẹp (thành cái mai)
                                        // Cho Mario nhảy nẩy lên một cái cho đúng kiểu
                                        //GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 10f);
                GetComponent<Rigidbody2D>().linearVelocity = new Vector2(rigidbody.linearVelocity.x, 10f);

            }
            else
            {
                // 2. Nếu đụng từ bên hông
                // KIỂM TRA: Nếu nó đã là cái mai VÀ nó đang ĐỨNG YÊN
                if (koopa.IsShell && !koopa.IsPushed)
                {
                    // Mario không chết! 
                    // Lúc này hàm OnCollisionEnter2D bên script Koopa sẽ lo việc đá cái mai đi.
                    Debug.Log("Mario đang đá cái mai, không chết.");
                }
                else
                {
                    // Nếu nó đang đi bộ HOẶC cái mai đang bay vèo vèo -> Mario mới chết
                    Debug.Log("Mario đụng quái và chết!");
                    HitByEnemy();
                }
            }
        }

        if (collision.gameObject.layer != LayerMask.NameToLayer("PowerUp"))
            if (transform.DotTest(collision.transform, Vector2.up))
                velocity.y = 0f;
    }


    public Transform respawnWhenHitByEnemy;

    public void HitByEnemy()
    {
        if (isInvincible) return;

        StartCoroutine(FlashAndInvincible());

        LifeManager lifeManager = FindAnyObjectByType<LifeManager>();
        if (lifeManager != null)
        {
            lifeManager.LoseLife();
        }

        if (CheckpointManager.HasCheckpoint)
        {
            RespawnAt(CheckpointManager.GetSpawnPosition(defaultSpawnPosition));
            return;
        }

        DPlayerDeath dPlayerDeath = GetComponent<DPlayerDeath>();
        if (dPlayerDeath != null)
        {
            dPlayerDeath.Die();
            return;
        }

        DeathAnimation deathAnimation = GetComponent<DeathAnimation>();
        if (deathAnimation != null)
        {
            deathAnimation.enabled = true;
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    // Hàm tạo hiệu ứng nhấp nháy và đếm ngược 2 giây
    private System.Collections.IEnumerator FlashAndInvincible()
    {
        isInvincible = true; // Bật lá chắn

        float blinkInterval = 0.1f; // Tốc độ nháy
        float duration = 2f; // Thời gian bất tử (2 giây)

        for (float t = 0; t < duration; t += blinkInterval)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // Tắt/Bật hình ảnh liên tục
            yield return new WaitForSeconds(blinkInterval); // Chờ 0.1s rồi lặp lại
        }

        spriteRenderer.enabled = true; // Đảm bảo hình ảnh được bật lại khi kết thúc
        isInvincible = false; // Tắt lá chắn, có thể bị trúng đòn lại
    }

    /// <summary>Ép tường / crush — cùng luồng với HitByEnemy (checkpoint → respawn → Die / reload).</summary>
    public void DieFromCrushOrHazard() => HitByEnemy();

    public void RespawnAt(Vector2 position)
    {
        velocity = Vector2.zero;
        rigidbody.position = position;
        rigidbody.linearVelocity = Vector2.zero;
    }

    /// <summary>Dịch chuyển Mario (cổng teleport). Đồng bộ vector vận tốc nội bộ với rigidbody.</summary>
    public void TeleportTo(Vector2 worldPosition, bool preserveVelocity = true)
    {
        Vector2 v = preserveVelocity ? rigidbody.linearVelocity : Vector2.zero;
        rigidbody.position = worldPosition;
        transform.position = worldPosition;
        rigidbody.linearVelocity = v;
        velocity = v;
    }
}