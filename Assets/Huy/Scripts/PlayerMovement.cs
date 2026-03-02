using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private new Camera camera;
    private new Rigidbody2D rigidbody;

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
    
    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow((maxJumpTime / 2f), 2);

    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        camera = Camera.main;
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

        if (collision.gameObject.layer != LayerMask.NameToLayer("PowerUp"))
            if (transform.DotTest(collision.transform, Vector2.up))
                velocity.y = 0f;
    }


    public Transform respawnWhenHitByEnemy;

    private void HitByEnemy()
    {
        if (respawnWhenHitByEnemy != null)
            RespawnAt(respawnWhenHitByEnemy.position);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void RespawnAt(Vector2 position)
    {
        velocity = Vector2.zero;
        rigidbody.position = position;
        rigidbody.linearVelocity = Vector2.zero;
    }
}
