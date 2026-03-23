using UnityEngine;

/// <summary>
/// Gắn cùng GameObject có <see cref="Enemy"/>: Goomba tự bước ngang khi Mario ở trên (kể cả thẳng đầu) và đang rơi.
/// Chạy sau <see cref="PlayerMovement"/> (execution order cao) và dùng cùng velocity.y như stomp.
/// </summary>
[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(Rigidbody2D))]
[DefaultExecutionOrder(100)]
public class EnemyStompEvade : MonoBehaviour
{
    [Header("Phát hiện Mario phía trên")]
    [Tooltip("Dùng bounds: chân Mario (min Y) so với đỉnh collider Goomba (max Y). Âm nhỏ = bắt sớm khi sắp chạm.")]
    [SerializeField] private bool useColliderBounds = true;

    [Tooltip("Khoảng chân Mario so với đỉnh đầu Goomba (world Y). -0.15 = cho phép hơi chồng lên để bắt sớm.")]
    [SerializeField] private float minFeetAboveGoombaTop = -0.14f;

    [Tooltip("Quá cao so với đỉnh đầu thì không né (0 = tắt trần).")]
    [SerializeField] private float maxFeetAboveGoombaTop = 2.85f;

    [Tooltip("Fallback khi thiếu collider: tâm Mario cao hơn tâm Goomba (world Y).")]
    [SerializeField] private float minHeightAboveCenter = 0.05f;

    [SerializeField] private float maxHeightAboveCenter = 2.9f;

    [Tooltip("Mario trong đoạn ngang [-left, +right] (theo tâm bounds hoặc transform).")]
    [SerializeField] private float horizontalDetectLeft = 2.05f;

    [SerializeField] private float horizontalDetectRight = 2.05f;

    [Tooltip("Khớp stomp PlayerMovement: velocity.y <= giá trị này là coi như có thể giẫm (mặc định ~0 giống stomp).")]
    [SerializeField] private float stompVelocityYThreshold = 0.06f;

    [Tooltip("Nếu Mario bay lên rất nhanh (vẫn trong vùng ngang), bỏ qua để tránh né nhầm khi nhảy qua đầu.")]
    [SerializeField] private float ignoreIfRisingFasterThan = 4.5f;

    [Header("Né (quãng cố định + tốc độ)")]
    [Tooltip("Tốc độ ngang khi né. Tăng speed chỉ rút ngắn thời gian, không tăng quãng đường.")]
    [SerializeField] private float evadeSpeed = 36f;

    [Tooltip("Quãng đường ngang (world) cần trượt hết thì hết né — độc lập với Evade Speed.")]
    [SerializeField] private float evadeDistance = 24.48f;

    [Tooltip("Nếu kẹt (không dịch chuyển X), cưỡng bức kết thúc né sau (distance/speed)×hệ số giây. 0 = tắt.")]
    [SerializeField] private float stuckEscapeTimeMultiplier = 2.75f;

    [Tooltip("Hồi chiêu trước khi né lại — chỉ áp dụng khi Mario không còn trong vùng né; còn trên đầu thì né liên tục (bỏ qua).")]
    [SerializeField] private float cooldown = 0.5f;

    [Header("Né: đổi hướng khi chạm tường")]
    [Tooltip("Layer chặn né (Default/Map/Ground...). Để trống = dùng Default+Map.")]
    [SerializeField] private LayerMask wallBlockingMask;

    [SerializeField] private float wallProbeDistance = 0.42f;

    [SerializeField] private float wallFlipCooldown = 0.07f;

    [Tooltip("Sau khi đổi hướng vì tường, chỉnh luôn hướng patrol của Enemy.")]
    [SerializeField] private bool syncEnemyDirectionOnWallFlip = true;

    [Tooltip("Quãng né (world) sau khi đập tường — đổi hướng và còn phải trượt bấy nhiêu theo hướng mới. Khác với Evade Distance lúc bắt đầu né.")]
    [SerializeField] private float wallBounceDistance = 5f;

    private static readonly RaycastHit2D[] WallCastHits = new RaycastHit2D[6];

    private Enemy enemy;
    private Rigidbody2D rb;
    private Transform player;
    private Rigidbody2D playerRb;
    private PlayerMovement playerMovement;
    private Collider2D enemyCol;

    private float evadeRemainingDistance;
    private float lastEvadePosX;
    private float evadeDirX;
    private float lastEvadeEndTime = -999f;
    private float evadeStuckTimer;
    private float lastWallFlipFixedTime = -999f;

    /// <summary>Lần né vừa xong mà Mario vẫn trong vùng giẫm → lần sau bỏ qua cooldown.</summary>
    private bool lastEvadeEndedWhileThreatened;

    private ContactFilter2D wallContactFilter;
    private bool wallFilterBuilt;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
        enemyCol = GetComponent<Collider2D>();
        BuildWallFilterIfNeeded();
    }

    private void BuildWallFilterIfNeeded()
    {
        if (wallFilterBuilt)
            return;
        wallFilterBuilt = true;
        wallContactFilter = new ContactFilter2D();
        wallContactFilter.useTriggers = false;
        if (wallBlockingMask.value == 0)
            wallBlockingMask = LayerMask.GetMask("Default", "Map");
        wallContactFilter.SetLayerMask(wallBlockingMask);
        wallContactFilter.useLayerMask = true;
    }

    private bool TryFlipEvadeDirectionForWall(Vector2 hitNormal)
    {
        if (Mathf.Abs(hitNormal.x) < 0.25f)
            return false;

        if (evadeDirX * hitNormal.x >= 0f)
            return false;

        if (Time.fixedTime - lastWallFlipFixedTime < wallFlipCooldown)
            return false;

        lastWallFlipFixedTime = Time.fixedTime;
        evadeDirX = -evadeDirX;

        if (syncEnemyDirectionOnWallFlip && enemy != null && !enemy.IsStomped)
            enemy.direction = new Vector2(evadeDirX, 0f);

        if (evadeRemainingDistance > 0f && wallBounceDistance > 0f)
        {
            evadeRemainingDistance = wallBounceDistance;
            evadeStuckTimer = 0f;
            lastEvadePosX = rb.position.x;
        }

        return true;
    }

    private void ProbeWallAheadAndFlip()
    {
        if (enemyCol == null || Mathf.Abs(evadeDirX) < 0.01f)
            return;

        BuildWallFilterIfNeeded();
        Vector2 dir = new Vector2(evadeDirX, 0f);
        int count = enemyCol.Cast(dir, wallContactFilter, WallCastHits, wallProbeDistance, true);

        for (int i = 0; i < count; i++)
        {
            RaycastHit2D h = WallCastHits[i];
            if (h.collider == null)
                continue;
            if (h.collider.CompareTag("Player"))
                continue;
            if (h.rigidbody == rb)
                continue;

            if (TryFlipEvadeDirectionForWall(h.normal))
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (enemy == null || enemy.IsStomped || evadeRemainingDistance <= 0f)
            return;
        if (collision.gameObject.CompareTag("Player"))
            return;
        if (collision.contactCount == 0)
            return;

        Vector2 n = collision.GetContact(0).normal;
        TryFlipEvadeDirectionForWall(n);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (enemy == null || enemy.IsStomped || evadeRemainingDistance <= 0f)
            return;
        if (collision.gameObject.CompareTag("Player"))
            return;
        if (collision.contactCount == 0)
            return;

        Vector2 n = collision.GetContact(0).normal;
        TryFlipEvadeDirectionForWall(n);
    }

    private void CachePlayerRefs()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null)
            return;
        player = p.transform;
        playerRb = p.GetComponent<Rigidbody2D>();
        playerMovement = p.GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        CachePlayerRefs();
    }

    private void Update()
    {
        if (enemy == null || enemy.IsStomped || evadeRemainingDistance > 0f)
            return;

        if (player == null)
            CachePlayerRefs();
        if (player == null)
            return;

        if (!EvaluateThreat(out float dx))
            return;

        if (Time.time < lastEvadeEndTime + cooldown && !lastEvadeEndedWhileThreatened)
            return;

        const float centerEps = 0.1f;
        if (Mathf.Abs(dx) <= centerEps)
            evadeDirX = Random.value > 0.5f ? 1f : -1f;
        else
            evadeDirX = dx > 0f ? -1f : 1f;

        evadeRemainingDistance = Mathf.Max(0f, evadeDistance);
        lastEvadePosX = rb.position.x;
        evadeStuckTimer = 0f;
        lastEvadeEndedWhileThreatened = false;
        rb.linearVelocity = new Vector2(evadeDirX * evadeSpeed, rb.linearVelocity.y);
    }

    private void FixedUpdate()
    {
        if (enemy == null || enemy.IsStomped)
            return;

        if (evadeRemainingDistance <= 0f)
            return;

        float px = rb.position.x;
        float movedX = Mathf.Abs(px - lastEvadePosX);
        lastEvadePosX = px;
        evadeRemainingDistance -= movedX;

        float dt = Time.fixedDeltaTime;
        float nominalTime = evadeDistance / Mathf.Max(evadeSpeed, 0.01f);
        if (stuckEscapeTimeMultiplier > 0f && nominalTime > 0f)
        {
            if (movedX < 0.0005f)
                evadeStuckTimer += dt;
            else
                evadeStuckTimer = 0f;

            if (evadeStuckTimer >= nominalTime * stuckEscapeTimeMultiplier)
                evadeRemainingDistance = 0f;
        }

        if (evadeRemainingDistance <= 0f)
        {
            evadeRemainingDistance = 0f;
            if (player == null)
                CachePlayerRefs();
            lastEvadeEndedWhileThreatened = player != null && EvaluateThreat(out _);
            lastEvadeEndTime = Time.time;
            return;
        }

        ProbeWallAheadAndFlip();

        rb.linearVelocity = new Vector2(evadeDirX * evadeSpeed, rb.linearVelocity.y);
    }

    private bool EvaluateThreat(out float dxWorld)
    {
        dxWorld = 0f;

        if (playerRb == null)
            playerRb = player.GetComponent<Rigidbody2D>();
        if (playerMovement == null)
            playerMovement = player.GetComponent<PlayerMovement>();

        float vy = playerMovement != null
            ? playerMovement.VerticalMoveSpeed
            : (playerRb != null ? playerRb.linearVelocity.y : 0f);

        if (vy > ignoreIfRisingFasterThan)
            return false;

        if (vy > stompVelocityYThreshold)
            return false;

        Collider2D playerCol = player.GetComponent<Collider2D>();

        bool inVertical;
        if (useColliderBounds && enemyCol != null && playerCol != null)
        {
            float goombaTop = enemyCol.bounds.max.y;
            float playerFeetY = playerCol.bounds.min.y;
            float gapFeet = playerFeetY - goombaTop;
            inVertical = gapFeet >= minFeetAboveGoombaTop
                && (maxFeetAboveGoombaTop <= 0f || gapFeet <= maxFeetAboveGoombaTop);

            dxWorld = playerCol.bounds.center.x - enemyCol.bounds.center.x;
        }
        else
        {
            Vector2 pp = player.position;
            Vector2 gp = transform.position;
            float dy = pp.y - gp.y;
            inVertical = dy >= minHeightAboveCenter
                && (maxHeightAboveCenter <= 0f || dy <= maxHeightAboveCenter);
            dxWorld = pp.x - gp.x;
        }

        if (!inVertical)
            return false;

        return dxWorld >= -horizontalDetectLeft && dxWorld <= horizontalDetectRight;
    }
}
