using UnityEngine;

/// <summary>
/// Cổng dịch chuyển 2D: vào trigger này → xuất hiện tại <see cref="exitPoint"/>.
/// Gán <see cref="linkedPortal"/> là cổng đích để tránh vừa teleport đã kích hoạt ngược lại.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Portal2D : MonoBehaviour
{
    [Tooltip("Vị trí xuất hiện (Transform của cổng B, thường là Empty con đặt chỗ đứng an toàn).")]
    [SerializeField] private Transform exitPoint;

    [Tooltip("Cổng đôi — script Portal2D trên cổng kia (để cả hai cùng bỏ qua trigger một lúc).")]
    [SerializeField] private Portal2D linkedPortal;

    [Tooltip("Thời gian (giây) không nhận trigger sau khi teleport — chống ping-pong A↔B.")]
    [SerializeField] private float exitCooldownSeconds = 0.45f;

    [Tooltip("Giữ vận tốc hiện tại khi qua cổng.")]
    [SerializeField] private bool preserveVelocity = true;

    [Tooltip("Cộng thêm vào vị trí exit (world-space offset).")]
    [SerializeField] private Vector2 exitPositionOffset;

    private float ignoreTriggersUntilUnscaled;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.unscaledTime < ignoreTriggersUntilUnscaled)
            return;
        if (exitPoint == null)
            return;

        if (!IsPlayerCollider(other))
            return;

        Rigidbody2D rb = other.attachedRigidbody != null
            ? other.attachedRigidbody
            : other.GetComponentInParent<Rigidbody2D>();
        if (rb == null)
            return;

        Vector2 dest = (Vector2)exitPoint.position + exitPositionOffset;
        PlayerMovement pm = rb.GetComponent<PlayerMovement>();
        if (pm != null)
            pm.TeleportTo(dest, preserveVelocity);
        else
        {
            rb.position = dest;
            rb.transform.position = dest;
            if (!preserveVelocity)
                rb.linearVelocity = Vector2.zero;
        }

        float until = Time.unscaledTime + exitCooldownSeconds;
        ignoreTriggersUntilUnscaled = until;
        if (linkedPortal != null)
            linkedPortal.ignoreTriggersUntilUnscaled = until;
    }

    private static bool IsPlayerCollider(Collider2D other)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0 && other.gameObject.layer == playerLayer)
            return true;

        if (other.GetComponent<PlayerMovement>() != null || other.GetComponentInParent<PlayerMovement>() != null)
            return true;

        return other.GetComponent<Player>() != null || other.GetComponentInParent<Player>() != null;
    }
}
