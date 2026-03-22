using UnityEngine;

public class Hover : MonoBehaviour
{
    public float amplitude = 0.2f; // Độ cao bay lên xuống
    public float speed = 2f;       // Tốc độ bay
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Dùng hàm Sin để tạo chuyển động nhịp nhàng như sóng biển
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}