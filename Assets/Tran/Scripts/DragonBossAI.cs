using UnityEngine;
using UnityEngine.Serialization;

public class DragonBossAI : MonoBehaviour
{
    public Transform player;
    public GameObject fireballPrefab;
    public Transform firePoint;

    public Health health;

    [Tooltip("Cooldown phase 1 và 3 (chậm).")]
    [FormerlySerializedAs("attackCooldown")]
    public float attackCooldownSlow = 2f;

    [Tooltip("Cooldown phase 2 và 4 (nhanh).")]
    public float attackCooldownFast = 1f;

    [FormerlySerializedAs("moveSpeed")]
    public float moveSpeedSlow = 2f;

    public float moveSpeedFast = 4f;

    [Tooltip("Khoảng cách theo trục X: khi đã gần Mario hơn giá trị này thì không tiến thêm (tránh dính sát).")]
    public float stopDistance = 4f;

    [Header("Phase 2+ — bay")]
    [Tooltip("Độ cao tối đa so với Y lúc spawn.")]
    [SerializeField] private float flyMaxHeightAboveSpawn = 8f;

    [Tooltip("Mỗi bao nhiêu giây chọn lại độ cao Y ngẫu nhiên trong vùng bay.")]
    [SerializeField] private float randomFlyRetargetInterval = 2f;

    [Header("Phase 3 & 4 — 3 tia")]
    [SerializeField] private float tripleShotSpreadDegrees = 28f;

    private float timer;
    private float spawnY;
    private int _phase = 1;
    private float randomFlyTargetY;
    private float nextRandomFlyRetargetTime;

    private void Start()
    {
        spawnY = transform.position.y;
        randomFlyTargetY = spawnY;
        nextRandomFlyRetargetTime = 0f;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        RefreshPhaseFromHealth();
    }

    private void Update()
    {
        RefreshPhaseFromHealth();
        FacePlayer();
        UpdateRandomFlyTarget();
        Move();
        AttackTimer();
    }

    private void RefreshPhaseFromHealth()
    {
        if (health == null) return;
        int max = health.maxHealth;
        if (max <= 0) return;
        float ratio = (float)health.currentHealth / max;
        int newPhase = ratio > 0.75f ? 1 : ratio > 0.50f ? 2 : ratio > 0.25f ? 3 : 4;
        if (newPhase != _phase)
        {
            int oldPhase = _phase;
            _phase = newPhase;
            if (newPhase >= 2 && oldPhase < 2)
                nextRandomFlyRetargetTime = 0f;
        }
    }

    private void UpdateRandomFlyTarget()
    {
        if (_phase < 2) return;
        if (Time.time < nextRandomFlyRetargetTime) return;
        float yMax = spawnY + flyMaxHeightAboveSpawn;
        randomFlyTargetY = Random.Range(spawnY, yMax);
        nextRandomFlyRetargetTime = Time.time + randomFlyRetargetInterval;
    }

    private float CurrentAttackCooldown() => _phase is 2 or 4 ? attackCooldownFast : attackCooldownSlow;

    private float CurrentMoveSpeed() => _phase == 1 ? moveSpeedSlow : moveSpeedFast;

    private void Move()
    {
        if (player == null) return;

        float speed = CurrentMoveSpeed();
        float moveX = 0f;
        float dx = Mathf.Abs(player.position.x - transform.position.x);
        if (dx > stopDistance)
        {
            float direction = player.position.x > transform.position.x ? 1 : -1;
            moveX = direction * speed * Time.deltaTime;
        }

        float newY = transform.position.y;
        if (_phase >= 2)
            newY = Mathf.MoveTowards(transform.position.y, randomFlyTargetY, speed * Time.deltaTime);

        transform.position += new Vector3(moveX, newY - transform.position.y, 0f);
    }

    private void AttackTimer()
    {
        timer += Time.deltaTime;

        if (timer > CurrentAttackCooldown())
        {
            Attack();
            timer = 0;
        }
    }

    private void Attack()
    {
        if (player == null) return;

        Vector2 baseDir = (Vector2)(player.position - firePoint.position);
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
        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, rotation);
        fireball.GetComponent<Fireball>().speed = 12f;
    }

    private static Vector2 RotateVector2(Vector2 v, float radians)
    {
        float c = Mathf.Cos(radians);
        float s = Mathf.Sin(radians);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }

    private void FacePlayer()
    {
        if (player == null) return;

        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }
}
