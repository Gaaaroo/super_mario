using UnityEngine;

public class MimicEnemy : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;

    [Header("Colliders")]
    public Collider2D chestTrigger;   // Trigger để detect Mario chạm rương
    public Collider2D bodyCollider;   // Collider để chiến đấu (IsTrigger = false)

    [Header("Player")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float patrolDistance = 2f;
    public float chaseRange = 6f;

    [Header("Timing")]
    public float activationDelay = 5f;   // Thời gian animation mở rương
    public float combatDelay = 1f;     // Thêm delay trước khi được đánh nhau

    private bool isActivated = false;      // Đã mở rương?
    private bool isCombatReady = false;    // Đã được phép đánh nhau?
    private bool isDead = false;

    private float direction = 1f;
    private Vector2 startPos;

    private AudioSource audioSource;

    [Header("Sound Effects")]
    public AudioClip wakeUpSound;
    public AudioClip walkSound;
    public AudioClip dieSound;

    void Start()
    {
        startPos = transform.position;

        if (!animator) animator = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        // BAN ĐẦU — chỉ trigger hoạt động, body collision tắt
        bodyCollider.enabled = false;
        chestTrigger.enabled = true;
    }


    // ─────────────────────────────────────────────
    // PLAYER CHẠM RƯƠNG (TRIGGER)
    // ─────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isActivated) return;

        ActivateMimic();
    }


    void ActivateMimic()
    {
        MusicManager.Instance.SetMusic(false);   // Tắt nhạc nền

        PlaySound(wakeUpSound);

        PlaySoundDelayed(walkSound, activationDelay);   // Delay âm bước chân cho khớp với animation

        isActivated = true;

        animator.SetTrigger("Transform");   // Animation mở rương

        // Sau thời gian biến hình → bắt đầu di chuyển
        Invoke(nameof(StartMovement), activationDelay);

        // Sau biến hình + chút delay → cho phép gây sát thương
        Invoke(nameof(EnableCombat), activationDelay + combatDelay);
    }


    void StartMovement()
    {
        bodyCollider.enabled = true;  // Bật collider chiến đấu
    }

    void EnableCombat()
    {
        isCombatReady = true;
    }


    // ─────────────────────────────────────────────
    // UPDATE DI CHUYỂN
    // ─────────────────────────────────────────────
    void Update()
    {
        if (!isActivated || isDead)
            return;

        MusicManager.Instance.SetMusic(false);   // Đảm bảo nhạc nền luôn tắt khi mimic hoạt động

        // Auto tìm player
        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // Nếu player gần → chase
        if (player && Vector2.Distance(transform.position, player.position) < chaseRange)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            transform.position += (Vector3)dir * moveSpeed * Time.deltaTime;

            spriteRenderer.flipX = dir.x < 0;
            return;
        }

        // Patrol qua lại
        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;

        if (Mathf.Abs(transform.position.x - startPos.x) > patrolDistance)
        {
            direction *= -1f;
            spriteRenderer.flipX = direction < 0;
        }
    }


    // ─────────────────────────────────────────────
    // COMBAT — CHỈ HOẠT ĐỘNG KHI COLLIDER KHÔNG TRIGGER
    // ─────────────────────────────────────────────
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (!isActivated) return;
        if (!isCombatReady) return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        Player playerScript = collision.gameObject.GetComponent<Player>();

        // Mario đè từ trên xuống
        if (collision.transform.DotTest(transform, Vector2.down))
        {
            Die();
            return;
        }

        // Mario chạm ngang → bị hit
        playerScript.Hit();
    }



    // ─────────────────────────────────────────────
    // DIE
    // ─────────────────────────────────────────────
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        animator.SetTrigger("Die");
        rb.simulated = false;

        StopSound(walkSound);   
        PlaySound(dieSound);

        MusicManager.Instance.SetMusic(true);    // Bật lại nhạc nền

        Destroy(gameObject, 2f);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    void StopSound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.Stop();
    }

    AudioClip pendingClip;

    void PlaySoundDelayed(AudioClip clip, float delay)
    {
        if (clip == null) return;
        pendingClip = clip;
        Invoke(nameof(_Play), delay);
    }

    void _Play()
    {
        audioSource.PlayOneShot(pendingClip);
    }
}