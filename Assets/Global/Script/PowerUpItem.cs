using UnityEngine;

public class PowerUpItem : MonoBehaviour
{
    public PowerUpType type; // Chọn loại sức mạnh trong bảng Inspector
    public float duration = 10f; // Thời gian tác dụng (nếu có)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tìm bộ não quản lý sức mạnh trên Mario
            PlayerPowerUpManager manager = other.GetComponent<PlayerPowerUpManager>();
            if (manager != null)
            {
                manager.CollectPowerUp(type, duration);
            }
            Destroy(gameObject); // Ăn xong biến mất
        }
    }
}