using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc để load lại màn chơi

public class DeathAnimation : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;
    public Sprite DeadSprite;

    private void Reset()
    {
        // Tự động tìm SpriteRenderer nếu ông quên kéo vào
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        // 1. Logic của Duic: Xử lý dữ liệu và UI trước khi diễn hoạt ảnh
        HandleLifeAndUI();

        // 2. Logic của Huy: Cập nhật hình ảnh và tắt vật lý
        UpdateSprite();
        DisablePhysics();

        // 3. Chạy hoạt ảnh văng lên trời
        StartCoroutine(Animate());
    }

    private void HandleLifeAndUI()
    {
        // Trừ 1 mạng trong kho dữ liệu static
        GameData.lives--;

        GameData.coins = GameData.coinsAtLevelStart;

        LifeManager.RefreshAllLifeUI();

        Debug.Log("Mario đã chết. Mạng còn lại: " + GameData.lives);
    }

    private void UpdateSprite()
    {
        if (SpriteRenderer == null) SpriteRenderer = GetComponent<SpriteRenderer>();

        SpriteRenderer.enabled = true;
        SpriteRenderer.sortingOrder = 10; // Đảm bảo xác Mario hiện lên trên cùng

        if (DeadSprite != null)
            SpriteRenderer.sprite = DeadSprite;
    }

    private void DisablePhysics()
    {
        // Tắt TẤT CẢ collider trên người Mario và các object con (như Small/Big)
        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in allColliders)
        {
            col.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Tắt script di chuyển
        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        // Tránh PlayerSpriteRender / AnimatedSprite (con Small/Big) ghi đè sprite chết mỗi LateUpdate
        foreach (PlayerSpriteRender psr in GetComponentsInChildren<PlayerSpriteRender>(true))
            psr.enabled = false;
        foreach (PlayerSpriteRenderCustom psrc in GetComponentsInChildren<PlayerSpriteRenderCustom>(true))
            psrc.enabled = false;
        foreach (AnimatedSprite anim in GetComponentsInChildren<AnimatedSprite>(true))
            anim.enabled = false;
    }

    private IEnumerator Animate()
    {
        // Đảm bảo vị trí không đổi khi bắt đầu diễn
        Vector3 deathPosition = transform.position;
        float elapsed = 0f;
        float duration = 2f;

        while (elapsed < duration)
        {
            // Ép vị trí Mario đứng yên tại chỗ chết, không cho phép bay đi đâu cả
            transform.position = deathPosition;

            elapsed += Time.deltaTime;
            yield return null;
        }

        ReloadLevel();
    }

    private void ReloadLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (GameData.lives > 0)
        {
            GameData.coins = GameData.coinsAtLevelStart;
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("GAME OVER - Load lại cùng map, reset mạng / tiền.");

            GameData.lives = 5;
            GameData.coins = 0;
            GameData.coinsAtLevelStart = 0;

            // Hết mạng = chơi lại từ đầu màn, không spawn tại checkpoint cũ
            CheckpointManager.ClearCheckpoint();

            SceneManager.LoadScene(sceneName);
        }
    }
}