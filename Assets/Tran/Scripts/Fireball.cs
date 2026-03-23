using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 8f;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            // Tìm script di chuyển của Mario
            PlayerMovement mario = hitInfo.GetComponent<PlayerMovement>();

            // Nếu tìm thấy, gọi hàm xử lý trúng đòn mà ta vừa viết
            if (mario != null)
            {
                mario.DieFromCrushOrHazard();
            }

            // Viên đạn tự nổ
            Destroy(gameObject);
        }
    }
}