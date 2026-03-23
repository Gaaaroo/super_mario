using UnityEngine;
using System.Collections;

public class PipeWarpSide : MonoBehaviour
{
    public Transform destination;      // Kéo cái ống đích (Pipe B) vào đây
    public float warpSpeed = 2f;      // Tốc độ di chuyển lúc chui
    public float distanceToMove = 1.5f; // Quãng đường chui vào trong ống

    private bool isWarping = false;

    private void OnTriggerStay2D(Collider2D other)
    {
        // 1. Kiểm tra nếu là Mario và không đang trong quá trình chui
        if (other.CompareTag("Player") && !isWarping)
        {
            // 2. Nếu người chơi nhấn phím D (đi tới)
            if (Input.GetKey(KeyCode.D))
            {
                StartCoroutine(WarpSequence(other.transform));
            }
        }
    }

    private IEnumerator WarpSequence(Transform player)
    {
        isWarping = true;

        // 3. Khóa điều khiển và vật lý
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (pm != null) pm.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // --- GIAI ĐOẠN 1: CHUI VÀO NGANG (SANG PHẢI) ---
        Vector3 startPos = player.position;
        Vector3 enterTarget = startPos + Vector3.right * distanceToMove;

        float elapsed = 0;
        while (elapsed < 1f)
        {
            player.position = Vector3.Lerp(startPos, enterTarget, elapsed);
            elapsed += Time.deltaTime * warpSpeed;
            yield return null;
        }

        // --- GIAI ĐOẠN 2: DỊCH CHUYỂN TỚI ỐNG ĐÍCH ---
        // Đặt Mario ở dưới đáy của ống đích (ẩn dưới đất)
        player.position = destination.position + Vector3.down * distanceToMove;

        // --- GIAI ĐOẠN 3: CHUI TỪ DƯỚI LÊN ---
        Vector3 exitStartPos = player.position;
        Vector3 exitTarget = destination.position + Vector3.up * 0.5f; // Nhô lên khỏi miệng ống 0.5m

        elapsed = 0;
        while (elapsed < 1f)
        {
            player.position = Vector3.Lerp(exitStartPos, exitTarget, elapsed);
            elapsed += Time.deltaTime * warpSpeed;
            yield return null;
        }

        // 4. Trả lại quyền điều khiển
        rb.bodyType = RigidbodyType2D.Dynamic;
        if (pm != null) pm.enabled = true;

        isWarping = false;
        Debug.Log("Chui ống thành công!");
    }
}