using UnityEngine;

/// <summary>
/// Phát hiện ép tường / block chủ yếu qua <see cref="Rigidbody2D.GetContacts"/> (đúng khi đang kẹt vật lý).
/// Cast chỉ dùng dự phòng. Crush Layers: collider hai bên phải thuộc mask (mặc định Default).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCrushDeath : MonoBehaviour
{
    [Tooltip("Chỉ tính va chạm với collider thuộc các layer này (mặc định Default).")]
    [SerializeField] private LayerMask crushLayers;

    [Tooltip("Độ lớn thành phần normal theo trục để coi là đang bị ép từ phía đó (0–1).")]
    [SerializeField] private float normalThreshold = 0.22f;

    [Tooltip("Khoảng cách cast (world) mỗi hướng — nhánh dự phòng Cast.")]
    [SerializeField] private float castDistance = 0.6f;

    [Tooltip("Hai phía Cast đều trong ngưỡng này — nhánh dự phòng.")]
    [SerializeField] private float crushDepth = 0.28f;

    [Tooltip("Cùng collider + Cast: Dot(normalA, normalB) <= giá trị này.")]
    [SerializeField] private float sameColliderNormalDotMax = -0.35f;

    [Tooltip("Cùng collider: khoảng cách giữa hai điểm (world) dọc trục ép.")]
    [SerializeField] private float minHitPointSeparation = 0.12f;

    [SerializeField] private bool detectHorizontalCrush = true;

    [SerializeField] private bool detectVerticalCrush = true;

    [Tooltip("Bật kiểm tra Contact (nên bật — phát hiện kẹt đáng tin hơn Cast).")]
    [SerializeField] private bool useContactDetection = true;

    [Tooltip("Dùng Cast nếu Contact không thấy ép (dự phòng).")]
    [SerializeField] private bool useCastFallback = true;

    [Tooltip("Số frame liên tiếp phải thỏa ép mới chết.")]
    [SerializeField] private int framesToConfirm = 2;

    private static readonly RaycastHit2D[] HitBuffer = new RaycastHit2D[16];
    private static readonly ContactPoint2D[] ContactBuffer = new ContactPoint2D[24];

    private Collider2D _col;
    private Rigidbody2D _rb;
    private ContactFilter2D _filter;
    private int _crushFrames;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        if (_col == null)
            _col = GetComponentInChildren<Collider2D>();

        if (crushLayers.value == 0)
            crushLayers = LayerMask.GetMask("Default");

        _filter = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = true,
            layerMask = crushLayers
        };
    }

    private void FixedUpdate()
    {
        if (_col == null || _rb == null)
            return;

        bool crushed = false;

        if (useContactDetection)
        {
            if (detectHorizontalCrush && IsSandwichedByContacts(true))
                crushed = true;
            if (!crushed && detectVerticalCrush && IsSandwichedByContacts(false))
                crushed = true;
        }

        if (!crushed && useCastFallback)
        {
            if (detectHorizontalCrush && IsSqueezedByCast(Vector2.left, Vector2.right))
                crushed = true;
            if (!crushed && detectVerticalCrush && IsSqueezedByCast(Vector2.up, Vector2.down))
                crushed = true;
        }

        if (crushed)
        {
            _crushFrames++;
            if (_crushFrames >= framesToConfirm)
            {
                _crushFrames = 0;
                var pm = GetComponent<PlayerMovement>();
                if (pm != null)
                    pm.DieFromCrushOrHazard();
            }
        }
        else
        {
            _crushFrames = 0;
        }
    }

    /// <summary>
    /// Ép ngang: có contact pháp tuyến +X (tường trái đẩy vào) và -X (tường phải).
    /// Ép dọc: +Y (sàn) và -Y (trần).
    /// </summary>
    private bool IsSandwichedByContacts(bool horizontal)
    {
        int n = _rb.GetContacts(_filter, ContactBuffer);
        if (n <= 0)
            return false;

        bool posAxis = false, negAxis = false;
        Collider2D colPos = null, colNeg = null;
        Vector2 ptPos = default, ptNeg = default;
        float t = normalThreshold;

        for (int i = 0; i < n; i++)
        {
            var cp = ContactBuffer[i];
            var c = cp.collider;
            if (c == null || c.attachedRigidbody == _rb)
                continue;
            if (!ColliderOnCrushLayers(c))
                continue;

            Vector2 nrm = cp.normal;
            if (horizontal)
            {
                if (nrm.x >= t)
                {
                    posAxis = true;
                    colPos = c;
                    ptPos = cp.point;
                }

                if (nrm.x <= -t)
                {
                    negAxis = true;
                    colNeg = c;
                    ptNeg = cp.point;
                }
            }
            else
            {
                if (nrm.y >= t)
                {
                    posAxis = true;
                    colPos = c;
                    ptPos = cp.point;
                }

                if (nrm.y <= -t)
                {
                    negAxis = true;
                    colNeg = c;
                    ptNeg = cp.point;
                }
            }
        }

        if (!posAxis || !negAxis)
            return false;

        if (colPos != colNeg)
            return true;

        if (horizontal)
            return Mathf.Abs(ptPos.x - ptNeg.x) >= minHitPointSeparation;
        return Mathf.Abs(ptPos.y - ptNeg.y) >= minHitPointSeparation;
    }

    private bool IsSqueezedByCast(Vector2 dirA, Vector2 dirB)
    {
        if (!TryClosestHit(dirA, out RaycastHit2D hitA))
            return false;
        if (!TryClosestHit(dirB, out RaycastHit2D hitB))
            return false;

        if (hitA.distance >= crushDepth || hitB.distance >= crushDepth)
            return false;

        if (!ColliderOnCrushLayers(hitA.collider) || !ColliderOnCrushLayers(hitB.collider))
            return false;

        if (hitA.collider != hitB.collider)
            return true;

        float normalDot = Vector2.Dot(hitA.normal, hitB.normal);
        if (normalDot > sameColliderNormalDotMax)
            return false;

        float separationAlongAxis = Mathf.Abs(Vector2.Dot(hitA.point - hitB.point, dirB));
        return separationAlongAxis >= minHitPointSeparation;
    }

    private bool ColliderOnCrushLayers(Collider2D c)
    {
        if (c == null)
            return false;
        return (crushLayers.value & (1 << c.gameObject.layer)) != 0;
    }

    private bool TryClosestHit(Vector2 direction, out RaycastHit2D best)
    {
        best = default;
        int count = _col.Cast(direction, _filter, HitBuffer, castDistance);
        float minD = float.MaxValue;
        bool any = false;
        for (int i = 0; i < count; i++)
        {
            var h = HitBuffer[i];
            if (h.collider == null)
                continue;
            if (h.collider.attachedRigidbody == _col.attachedRigidbody)
                continue;
            if (!ColliderOnCrushLayers(h.collider))
                continue;
            if (h.distance < minD)
            {
                minD = h.distance;
                best = h;
                any = true;
            }
        }

        return any;
    }
}
