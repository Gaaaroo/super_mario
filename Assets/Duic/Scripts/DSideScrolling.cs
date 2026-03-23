using UnityEngine;

public class DSideScrolling : MonoBehaviour
{
    private Transform player;
    private float furthestReachedX; // Lưu vị trí xa nhất mà camera từng đạt tới

    private void Awake()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            // Khởi tạo vị trí ban đầu
            furthestReachedX = transform.position.x;
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 cameraPosition = transform.position;

        // LOGIC CHÍNH: So sánh vị trí X hiện tại của Mario với vị trí xa nhất camera từng đạt được.
        // Chỉ cập nhật nếu Mario đã đi xa hơn vị trí đó.
        cameraPosition.x = Mathf.Max(cameraPosition.x, player.position.x);

        transform.position = cameraPosition;
    }
}
