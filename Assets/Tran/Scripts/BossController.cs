using UnityEngine;
using UnityEngine.UI;

public class BossController : MonoBehaviour
{
    [Header("Health & UI")]
    public int maxHealth = 100;

    private int currentHealth;
    public Slider healthBar;

    [Header("Attack Settings")]
    public Transform fightPoint;

    public GameObject fireballPrefab;
    [Tooltip("Tốc độ bắn phase 1 và phase 3 (chậm).")]
    public float fireRate = 2f;

    private float nextFireTime = 0f;

    [Header("Movement & AI")]
    public Transform player;

    public float moveSpeed = 2f;
    public float chaseDistance = 1000f;
    public float stopDistance = 4f;

    [Header("Phase 2 & 4 — nhanh")]
    [Tooltip("Tốc độ bắn phase 2 và phase 4 (nhanh).")]
    public float rageFireRate = 1f;
    public float rageMoveSpeed = 4f;

    [Tooltip("Độ cao Y tối đa so với vị trí lúc spawn (phase 2+).")]
    [SerializeField] private float rageMaxHeightAboveGround = 8f;

    [Tooltip("Mỗi bao nhiêu giây chọn lại độ cao Y ngẫu nhiên trong vùng bay.")]
    [SerializeField] private float randomFlyRetargetInterval = 2f;

    [Header("Phase 3 & 4 — 3 tia")]
    [SerializeField] private float tripleShotSpreadDegrees = 28f;

    private bool isFacingRight = true;
    private float groundY;
    private Color initialSpriteColor;
    private int _phase = 1;
    private float randomFlyTargetY;
    private float nextRandomFlyRetargetTime;

    private void Start()
    {
        groundY = transform.position.y;
        randomFlyTargetY = groundY;
        nextRandomFlyRetargetTime = 0f;
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            initialSpriteColor = sr.color;

        RefreshPhase();
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        LookAtPlayer();

        bool inChaseRange = distanceToPlayer < chaseDistance && distanceToPlayer > stopDistance;
        UpdateRandomFlyTarget();
        ChaseOrFly(inChaseRange);

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + GetFireRateForPhase(_phase);
        }
    }

    /// <summary>Phase 1 và 3: chậm. Phase 2 và 4: nhanh.</summary>
    private float GetFireRateForPhase(int phase)
    {
        return phase is 2 or 4 ? rageFireRate : fireRate;
    }

    private void LookAtPlayer()
    {
        if (player.position.x > transform.position.x && !isFacingRight)
            Flip();
        else if (player.position.x < transform.position.x && isFacingRight)
            Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    /// <summary>Phase 1: chỉ ngang. Phase 2–4: bay theo độ cao Y ngẫu nhiên trong vùng cho phép.</summary>
    private void ChaseOrFly(bool inChaseRange)
    {
        float targetX = transform.position.x;
        float targetY = transform.position.y;

        if (inChaseRange)
            targetX = player.position.x;

        bool flyPhase = _phase >= 2;
        if (flyPhase)
            targetY = randomFlyTargetY;

        float currentSpeed = flyPhase ? rageMoveSpeed : moveSpeed;
        if (!inChaseRange && !flyPhase)
            return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            new Vector2(targetX, targetY),
            currentSpeed * Time.deltaTime);
    }

    private void UpdateRandomFlyTarget()
    {
        if (_phase < 2) return;
        if (Time.time < nextRandomFlyRetargetTime) return;
        float yMax = groundY + rageMaxHeightAboveGround;
        randomFlyTargetY = Random.Range(groundY, yMax);
        nextRandomFlyRetargetTime = Time.time + randomFlyRetargetInterval;
    }

    private void Shoot()
    {
        if (fightPoint == null || fireballPrefab == null || player == null) return;

        Vector3 target = GetAimTarget();
        Vector2 baseDir = (Vector2)(target - fightPoint.position);
        if (baseDir.sqrMagnitude < 0.0001f)
            baseDir = player.position.x >= transform.position.x ? Vector2.right : Vector2.left;
        else
            baseDir.Normalize();

        if (_phase <= 2)
            SpawnFireballInDirection(baseDir);
        else
        {
            float spreadRad = tripleShotSpreadDegrees * Mathf.Deg2Rad;
            SpawnFireballInDirection(baseDir);
            SpawnFireballInDirection(RotateVector2(baseDir, spreadRad));
            SpawnFireballInDirection(RotateVector2(baseDir, -spreadRad));
        }
    }

    private void SpawnFireballInDirection(Vector2 dir)
    {
        dir.Normalize();
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject fireball = Instantiate(fireballPrefab, fightPoint.position, rotation);
        Fireball fb = fireball.GetComponent<Fireball>();
        if (fb != null)
            fb.speed = 12f;
    }

    private static Vector2 RotateVector2(Vector2 v, float radians)
    {
        float c = Mathf.Cos(radians);
        float s = Mathf.Sin(radians);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }

    private Vector3 GetAimTarget()
    {
        Collider2D[] cols = player.GetComponentsInChildren<Collider2D>();
        bool found = false;
        Bounds b = default;
        foreach (Collider2D c in cols)
        {
            if (c.isTrigger) continue;
            if (!found)
            {
                b = c.bounds;
                found = true;
            }
            else
                b.Encapsulate(c.bounds);
        }

        if (found) return b.center;
        return player.position + Vector3.up * 0.5f;
    }

    private void RefreshPhase()
    {
        if (maxHealth <= 0) return;
        float ratio = (float)currentHealth / maxHealth;
        int newPhase = ratio > 0.75f ? 1 : ratio > 0.50f ? 2 : ratio > 0.25f ? 3 : 4;

        if (newPhase != _phase)
        {
            int oldPhase = _phase;
            _phase = newPhase;
            if (newPhase >= 2 && oldPhase < 2)
                nextRandomFlyRetargetTime = 0f;
            ApplyPhaseVisuals();
        }
    }

    private void ApplyPhaseVisuals()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        switch (_phase)
        {
            case 1:
                sr.color = initialSpriteColor;
                break;
            case 2:
                sr.color = new Color(1f, 0.5f, 0.5f);
                break;
            case 3:
                sr.color = new Color(1f, 0.72f, 0.42f);
                break;
            case 4:
                sr.color = new Color(1f, 0.35f, 0.35f);
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        if (healthBar != null) healthBar.value = currentHealth;

        RefreshPhase();

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("Boss Defeated!");
        LevelLoader loader = FindAnyObjectByType<LevelLoader>();

        if (loader != null)
            loader.LoadNextLevel();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("WIN");

        Destroy(gameObject);
    }
}
