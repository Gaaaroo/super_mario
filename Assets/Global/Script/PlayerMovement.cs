using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private new Camera camera;
    private new Rigidbody2D rigidbody;
    private Vector2 defaultSpawnPosition;

    private Vector2 velocity;
    private float inputAxis;

    public float moveSpeed = 8f;
    public float maxJumpHeight = 5f;
    public float maxJumpTime = 1f;
    public float jumpBufferTimer = 0f;

    [Header("Âm thanh")]
    public AudioSource jumpAudioSource; // Loa phát tiếng nhảy
    public AudioSource runAudioSource;  // Loa phát tiếng chạy (Loop)
    public AudioClip jumpSound;
    public AudioClip runSound; // Nhớ kéo file tiếng bước chân vào đây



    [Header("Bất Tử")]
    public bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow(maxJumpTime / 2f, 2f);

    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);
    public bool falling => velocity.y < 0f && !grounded;

    /// <summary>velocity.y hiện tại — trùng logic stomp enemy (<see cref="OnCollisionEnter2D"/> dùng velocity.y &lt;= 0).</summary>
    public float VerticalMoveSpeed => velocity.y;

    private void Awake()
    {

        rigidbody = GetComponent<Rigidbody2D>();
        camera = Camera.main;
    }

    private void Update()
    {
        HorizontalMovement();

        grounded = rigidbody.Raycast(Vector2.down);

        if (grounded)
        {
            GroundedMovement();
        }

        jumpBufferTimer -= Time.deltaTime;

        // Xoay sprite theo hướng di chuyển
        if (inputAxis > 0f)
            transform.eulerAngles = Vector3.zero;
        else if (inputAxis < 0f)
            transform.eulerAngles = new Vector3(0f, 180f, 0f);

        HandleRunSound();
    }

    private void FixedUpdate()
    {
        // Move mario based on his velocity
        Vector2 position = rigidbody.position;
        position += velocity * Time.fixedDeltaTime;

        // Clamp within the screen bounds
        Vector2 leftEdge = camera.ScreenToWorldPoint(Vector2.zero);
        Vector2 rightEdge = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        position.x = Mathf.Clamp(position.x, leftEdge.x + 0.5f, rightEdge.x - 0.5f);

        rigidbody.MovePosition(position);
    }

    private void HorizontalMovement()
    {
        // Accelerate / decelerate
        inputAxis = Input.GetAxis("Horizontal");
        velocity.x = Mathf.MoveTowards(velocity.x, inputAxis * moveSpeed, moveSpeed);

        // Check if running into a wall
        if (rigidbody.Raycast(Vector2.right * velocity.x))
        {
            velocity.x = 0f;
        }

        // Flip sprite to face direction
        if (velocity.x > 0f)
        {
            transform.eulerAngles = Vector3.zero;
        }
        else if (velocity.x < 0f)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
    }

    private void GroundedMovement()
    {
        // Prevent gravity from infinitly building up
        velocity.y = Mathf.Max(velocity.y, 0f);
        jumping = velocity.y > 0f;

        // Perform jump
        if (Input.GetButtonDown("Jump"))
        {
            velocity.y = jumpForce;
            jumping = true;
            jumpBufferTimer = 0f;

            if (jumpSound != null && jumpAudioSource != null)
            {
                jumpAudioSource.PlayOneShot(jumpSound);
            }
        }
    }

    private void ApplyGravity()
    {
        // Check if falling
        bool falling = velocity.y < 0f || !Input.GetButton("Jump");
        float multiplier = falling ? 2f : 1f;

        // Apply gravity and terminal velocity
        velocity.y += gravity * multiplier * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, gravity / 2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // Bounce off enemy head
            if (transform.DotTest(collision.transform, Vector2.down))
            {
                velocity.y = jumpForce / 2f;
                jumping = true;
            }
        }
        else if (collision.gameObject.layer != LayerMask.NameToLayer("PowerUp"))
        {
            // Stop vertical movement if mario bonks his head

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
            {
                velocity.y = 0f;
            }
        }
    }

    public void HitByEnemy()
    {

        // Nếu đang tàng hình thì bỏ qua, coi như không có chuyện gì xảy ra
        if (GameData.isInvincible)
        {
            Debug.Log("Mario đang tàng hình, quái tuổi gì!");
            return;
        }

        // Tìm script DeathAnimation của Huy trên người Mario và BẬT nó lên
        // Khi bật lên, hàm OnEnable trong đó sẽ lo hết việc trừ mạng và reset tiền
        DeathAnimation deathScript = GetComponent<DeathAnimation>();

        // 2. Bật hiệu ứng nhấp nháy 2 giây
        StartCoroutine(FlashAndInvincible());

        // 3. Gọi trừ mạng
        LifeManager lifeManager = FindAnyObjectByType<LifeManager>();
        if (lifeManager != null)
        {
            lifeManager.LoseLife();
        }

        // =========================================================
        // CODE CỦA TEAM BẠN ĐÃ ĐƯỢC TẠM TẮT (COMMENT) Ở DƯỚI ĐÂY
        // Lý do: Nếu bật, Mario sẽ chết/dịch chuyển ngay lập tức, 
        // làm mất tác dụng nhấp nháy chạy tiếp của bạn.
        // =========================================================
        /*
        if (CheckpointManager.HasCheckpoint)
        {
            RespawnAt(CheckpointManager.GetSpawnPosition(defaultSpawnPosition));
            return;
        }

        DeathAnimation deathAnimation = GetComponent<DeathAnimation>();
        if (deathAnimation != null)
        {
            deathAnimation.enabled = true;
            return;
        }

        DPlayerDeath dPlayerDeath = GetComponent<DPlayerDeath>();
        if (dPlayerDeath != null)
        {
            dPlayerDeath.Die();
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        */
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

    void HandleRunSound()
    {
        // ĐIỀU KIỆN: Đang đứng trên đất VÀ đang chạy (running là biến ông đã có)
        if (grounded && running)
        {
            // Nếu cái loa chưa hát thì bảo nó hát
            if (!runAudioSource.isPlaying)
            {
                runAudioSource.Play();
            }
        }
        else
        {
            // Nếu đang nhảy hoặc đứng yên thì bắt nó im lặng
            if (runAudioSource.isPlaying)
            {
                runAudioSource.Stop();
            }
        }
    }
}