using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Cài đặt loại vùng chết")]
    [Tooltip("Tích vào nếu đây là Hố Sâu (Tàng hình vẫn chết). Bỏ tích nếu là Gai/Lửa (Tàng hình không chết).")]
    public bool isInstantDeathPit = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Chỉ xử lý nếu là Mario (Tag: Player)
        if (other.CompareTag("Player"))
        {
            // 2. Nếu là GAI (isInstantDeathPit = false) VÀ đang tàng hình -> THA
            if (!isInstantDeathPit && GameData.isInvincible)
            {
                Debug.Log("Mario đang tàng hình, đi xuyên qua vật cản!");
                return; // Thoát hàm, không chết
            }

            // 3. Nếu là HỐ SÂU, hoặc là GAI nhưng không tàng hình -> CHẾT
            DeathAnimation deathScript = other.GetComponent<DeathAnimation>();

            // Chỉ kích hoạt nếu script chết đang tắt (tránh trừ 2 lần mạng)
            if (deathScript != null && deathScript.enabled == false)
            {
                deathScript.enabled = true;
                Debug.Log("Mario đã chết tại: " + gameObject.name);
            }
        }
    }
}