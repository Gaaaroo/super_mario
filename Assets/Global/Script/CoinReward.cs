using UnityEngine;
using System.Collections;

public class CoinReward : MonoBehaviour
{
    [Header("Physics Settings")]
    public float jumpVelocity = 12f;  // Lực văng lên ban đầu (càng cao văng càng nhanh)
    public float gravity = -35f;      // Trọng lực kéo xuống (càng âm rơi càng nặng)

    void Start()
    {
        // 1. Cộng tiền
        GameData.coins++;
        if (UIManager.Instance != null) UIManager.Instance.UpdateUI();

        // 2. Chạy hoạt ảnh vật lý
        StartCoroutine(AnimatePhysics());
    }

    private IEnumerator AnimatePhysics()
    {
        Vector3 pos = transform.position;
        float startY = pos.y; // Lưu lại vạch xuất phát
        float vY = jumpVelocity; // Vận tốc trục Y ban đầu

        // Chạy cho đến khi đồng xu rơi xuống qua khỏi vị trí ban đầu
        while (true)
        {
            // Áp dụng gia tốc trọng lực vào vận tốc
            vY += gravity * Time.deltaTime;

            // Cập nhật vị trí dựa trên vận tốc
            pos.y += vY * Time.deltaTime;
            transform.position = pos;

            // Nếu đang rơi xuống (vY < 0) và đã chạm hoặc vượt quá vị trí ban đầu
            if (vY < 0 && pos.y <= startY)
            {
                break;
            }

            yield return null;
        }

        // Đặt về đúng vị trí cũ trước khi biến mất cho đẹp
        transform.position = new Vector3(pos.x, startY, pos.z);

        // 3. Biến mất
        Destroy(gameObject);
    }
}