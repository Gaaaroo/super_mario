using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DPlayerDeath : MonoBehaviour
{
    public Sprite deathSprite; // Kéo cái hình Mario chết vào đây
    private bool isDead = false;

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. TRỪ MẠNG TRONG GAME DATA
        GameData.lives--;

        // 2. CẬP NHẬT GIAO DIỆN NGAY LẬP TỨC
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
        }

        // 3. KIỂM TRA GAME OVER
        if (GameData.lives <= 0)
        {
            Debug.Log("HẾT MẠNG RỒI!");
            GameData.lives = 5; // Reset lại để chơi tiếp hoặc xử lý hiện bảng Lose
        }

        // 4. Tắt điều khiển và chạy anim (giữ nguyên code cũ của ông)
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = deathSprite;
        if (GetComponent<AnimatedSprite>() != null) GetComponent<AnimatedSprite>().enabled = false;

        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // Vô hiệu hóa va chạm với mọi thứ để rơi xuyên sàn luôn
        GetComponent<Collider2D>().enabled = false;

        // Nhẩy nẩy lên một phát
        rb.linearVelocity = new Vector2(0, 12f);

        // Chờ 3 giây cho rơi khỏi màn hình rồi mới load lại cảnh
        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}