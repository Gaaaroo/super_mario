using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody2D rb;
    private Collider2D capsuleCollider;

    private Vector2 velocity;
    private float inputAxis;

    public float moveSpeed = 8f;
    public float maxJumpHeight = 5f;
    public float maxJumpTime = 1f;

    [Header("Âm thanh")]
    public AudioClip jumpSound;
    private AudioSource audioSource;

    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow(maxJumpTime / 2f, 2f);

    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);
    public bool falling => velocity.y < 0f && !grounded;

    private void Awake()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        rb.bodyType = RigidbodyType2D.Dynamic; // ???
        capsuleCollider.enabled = true;
        velocity = Vector2.zero;
        jumping = false;
    }

    private void OnDisable()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        capsuleCollider.enabled = false;
        velocity = Vector2.zero;
        jumping = false;
        rigidbody = GetComponent<Rigidbody2D>();
        camera = Camera.main;

        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        HorizontalMovement();

        grounded = rb.Raycast(Vector2.down);

        if (grounded)
        {
            GroundedMovement();
        }

        ApplyGravity();
    }

    private void FixedUpdate()
    {
        // Move mario based on his velocity
        Vector2 position = rb.position;
        position += velocity * Time.fixedDeltaTime;

        // Clamp within the screen bounds
        Vector2 leftEdge = mainCamera.ScreenToWorldPoint(Vector2.zero);
        Vector2 rightEdge = mainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        position.x = Mathf.Clamp(position.x, leftEdge.x + 0.5f, rightEdge.x - 0.5f);

        rb.MovePosition(position);
    }

    private void HorizontalMovement()
    {
        // Accelerate / decelerate
        inputAxis = Input.GetAxis("Horizontal");
        velocity.x = Mathf.MoveTowards(velocity.x, inputAxis * moveSpeed, moveSpeed);

        // Check if running into a wall
        if (rb.Raycast(Vector2.right * velocity.x))
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

            if (jumpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(jumpSound);
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
            {
                velocity.y = 0f;
            }
        }
    }

    }


    public Transform respawnWhenHitByEnemy;

    //private void HitByEnemy()
    //{
    //    if (respawnWhenHitByEnemy != null)
    //        RespawnAt(respawnWhenHitByEnemy.position);
    //    else
    //        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    //}

    private void HitByEnemy()
    {
        // Tìm script Death trên người Mario và gọi hàm Die()
        DPlayerDeath deathScript = GetComponent<DPlayerDeath>();
        if (deathScript != null)
        {
            deathScript.Die();
        }
        else
        {
            // Nếu không có script death thì mới load scene thẳng
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    public void RespawnAt(Vector2 position)
    {
        velocity = Vector2.zero;
        rigidbody.position = position;
        rigidbody.linearVelocity = Vector2.zero;
    }
}