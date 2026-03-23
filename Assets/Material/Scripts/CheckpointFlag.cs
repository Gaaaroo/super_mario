using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class CheckpointFlag : MonoBehaviour
{
    [Tooltip("Điểm respawn. Để trống sẽ dùng chính vị trí lá cờ.")]
    public Transform spawnPoint;

    [Tooltip("Màu cờ khi chưa kích hoạt.")]
    public Color inactiveColor = Color.white;

    [Tooltip("Màu cờ khi đã kích hoạt.")]
    public Color activeColor = Color.green;

    private SpriteRenderer spriteRenderer;
    private bool activated;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        spriteRenderer.color = inactiveColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated)
            return;

        if (!IsPlayerCollider(other))
            return;

        activated = true;
        spriteRenderer.color = activeColor;

        Vector2 targetPosition = spawnPoint != null ? (Vector2)spawnPoint.position : (Vector2)transform.position;
        CheckpointManager.SetCheckpoint(targetPosition);
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

