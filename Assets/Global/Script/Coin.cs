using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Settings")]
    public int coinValue = 1; // Mỗi đồng xu đáng giá bao nhiêu (mặc định là 1)

    private bool isCollected = false; // Chốt an toàn

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem vật chạm vào có phải là Mario (Tag: Player) không
        if (other.CompareTag("Player"))
        {
            isCollected = true; // Đánh dấu đã ăn, không cho ăn nữa

            EatCoin();
        }
    }

    private void EatCoin()
    {
        // 1. Cộng tiền vào kho dữ liệu chung
        GameData.coins += coinValue;

        // 2. Bảo UIManager cập nhật con số mới lên màn hình
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
        }

        // 3. Tùy chọn: Thêm hiệu ứng âm thanh hoặc hạt bụi ở đây (nếu có)
        Debug.Log("Đã ăn xu! Tổng tiền: " + GameData.coins);

        // 4. Biến mất khỏi màn hình
        Destroy(gameObject);
    }
}