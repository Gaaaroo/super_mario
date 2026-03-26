using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementTran : MonoBehaviour // Đổi tên class để không đụng hàng
{
    private new Camera camera;
    private new Rigidbody2D rigidbody;
    private Vector2 defaultSpawnPosition;

    private Vector2 velocity;
    private float inputAxis;

    [Header("Input responsiveness")]
    public float jumpBufferTime = 0.12f;
    public float coyoteTime = 0.1f;

    private float jumpBufferTimer;
    private float coyoteTimer;

    public float moveSpeed = 8f;
    public float maxJumpHeight = 5f;
    public float maxJumpTime = 1f;

    [Header("Âm thanh")]
    public AudioClip jumpSound;
    private AudioSource audioSource;

    private SpriteRenderer spriteRenderer;

    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow((maxJumpTime / 2f), 2);

    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);

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
        inputAxis = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;

        grounded = rigidbody.Raycast(Vector2.down);
        if (grounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        jumpBufferTimer -= Time.deltaTime;

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

        velocity.x = inputAxis * moveSpeed;

        bool falling = velocity.y < 0f || !Input.GetButton("Jump");
        float multiplier = falling ? 2f : 1f;
        velocity.y += gravity * multiplier * Time.fixedDeltaTime;
        velocity.y = Mathf.Max(velocity.y, gravity / 2f);

        Vector2 leftEdge = camera.ScreenToWorldPoint(Vector2.zero);
        Vector2 rightEdge = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        if (rigidbody.position.x <= leftEdge.x + 0.5f && velocity.x < 0f)
            velocity.x = 0f;
        if (rigidbody.position.x >= rightEdge.x - 0.5f && velocity.x > 0f)
            velocity.x = 0f;

        rigidbody.linearVelocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsStomped)
        {
            if (transform.DotTest(collision.transform, Vector2.down) && velocity.y <= 0f)
            {
                velocity.y = jumpForce / 2f;
                enemy.Stomp();
            }
            else
            {
                HitByEnemy();
            }
            return;
        }

        Koopa koopa = collision.gameObject.GetComponent<Koopa>();
        if (koopa != null)
        {
            if (collision.contacts[0].normal.y > 0.5f)
            {
                koopa.Stomp(transform);
                GetComponent<Rigidbody2D>().linearVelocity = new Vector2(rigidbody.linearVelocity.x, 10f);
            }
            else
            {
                if (koopa.IsShell && !koopa.IsPushed)
                {
                    Debug.Log("Mario đang đá cái mai, không chết.");
                }
                else
                {
                    HitByEnemy();
                }
            }
        }

        if (collision.gameObject.layer != LayerMask.NameToLayer("PowerUp"))
            if (transform.DotTest(collision.transform, Vector2.up))
                velocity.y = 0f;
    }

    // --- HÀM XỬ LÝ TRÚNG ĐÒN (CUSTOM CỦA BẠN) ---
    public void HitByEnemy()
    {
        // Trừ mạng theo hệ thống chung của team
        GameData.lives--;
        GameData.coins = GameData.coinsAtLevelStart;
        LifeManager.RefreshAllLifeUI();

        // Nếu HẾT MẠNG -> Game Over, reset về màn đầu
        if (GameData.lives <= 0)
        {
            GameData.lives = 5;
            GameData.coins = 0;
            GameData.coinsAtLevelStart = 0;
            CheckpointManager.ClearCheckpoint();
            LifeManager.RefreshAllLifeUI();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        // Vẫn còn mạng: nhấp nháy (chỉ hiệu ứng, không bất tử)
        StartCoroutine(FlashAfterHit());
    }

    private System.Collections.IEnumerator FlashAfterHit()
    {
        float blinkInterval = 0.1f;
        float duration = 5f;

        for (float t = 0; t < duration; t += blinkInterval)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }

        spriteRenderer.enabled = true;
    }

    public void DieFromCrushOrHazard() => HitByEnemy();

    public void RespawnAt(Vector2 position)
    {
        velocity = Vector2.zero;
        rigidbody.position = position;
        rigidbody.linearVelocity = Vector2.zero;
    }

    public void TeleportTo(Vector2 worldPosition, bool preserveVelocity = true)
    {
        Vector2 v = preserveVelocity ? rigidbody.linearVelocity : Vector2.zero;
        rigidbody.position = worldPosition;
        transform.position = worldPosition;
        rigidbody.linearVelocity = v;
        velocity = v;
    }
}