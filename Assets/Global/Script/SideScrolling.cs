using UnityEngine;

public class SideScrolling : MonoBehaviour
{
    private Transform player;

    private void Awake()
    {
        // Tìm Mario thông qua Tag "Player"
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 cameraPosition = transform.position;

        // Lấy giá trị lớn nhất giữa vị trí hiện tại của Camera và vị trí của Mario.
        // vidu - Nếu Camera đang ở vị trí X = 0 và Mario di chuyển đến X = 5, thì cameraPosition.x sẽ được cập nhật thành 5.
        // Điều này đảm bảo Camera chỉ có thể tăng X (đi tới) chứ không bao giờ giảm X (đi lùi).
        cameraPosition.x = Mathf.Max(cameraPosition.x, player.position.x);

        transform.position = cameraPosition;
    }
}
