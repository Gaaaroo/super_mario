using UnityEngine;

public class SpikePopUp : MonoBehaviour
{
    public Transform spikeTransform;
    public float targetY;
    public float speed = 10f;

    private bool isTriggered = false;
    private bool isFinished = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ chạy nếu là Player và chưa hoàn thành
        if (other.CompareTag("Player") && !isFinished)
        {
            isTriggered = true;
        }
    }

    private void Update()
    {
        if (isTriggered && !isFinished)
        {
            // 1. Di chuyển Y
            float newY = Mathf.MoveTowards(spikeTransform.localPosition.y, targetY, speed * Time.deltaTime);
            spikeTransform.localPosition = new Vector2(spikeTransform.localPosition.x, newY);

            // 2. Kiểm tra nếu đã tới đích (dùng Mathf.Approximately cho chính xác)
            if (Mathf.Approximately(newY, targetY))
            {
                // ÉP CHẶT tọa độ cuối cùng để không bao giờ bị lệch
                spikeTransform.localPosition = new Vector2(spikeTransform.localPosition.x, targetY);

                isTriggered = false;
                isFinished = true; // Đánh dấu đã xong hoàn toàn

                // 3. TẮT CẢI TRIGGER ĐỂ KHÔNG NHẬN VA CHẠM NỮA (Giúp mượt máy)
                Collider2D triggerCol = GetComponent<Collider2D>();
                if (triggerCol != null) triggerCol.enabled = false;

                Debug.Log("Gai đã khóa vị trí!");
                this.enabled = false; // Tắt luôn script này
            }
        }
    }
}