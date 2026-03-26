//using System.Collections;
//using UnityEngine;
//using UnityEngine.SceneManagement; // Bắt buộc để load lại màn chơi

//public class DeathAnimation : MonoBehaviour
//{
//    public SpriteRenderer SpriteRenderer;
//    public Sprite DeadSprite;

//    private void Reset()
//    {
//        // Tự động tìm SpriteRenderer nếu ông quên kéo vào
//        SpriteRenderer = GetComponent<SpriteRenderer>();
//    }

//    private void OnEnable()
//    {
//        // 1. Logic của Duic: Xử lý dữ liệu và UI trước khi diễn hoạt ảnh
//        HandleLifeAndUI();

//        // 2. Logic của Huy: Cập nhật hình ảnh và tắt vật lý
//        UpdateSprite();
//        DisablePhysics();

//        // 3. Chạy hoạt ảnh văng lên trời
//        StartCoroutine(Animate());
//    }

//    private void HandleLifeAndUI()
//    {
//        // Trừ 1 mạng trong kho dữ liệu static
//        GameData.lives--;

//        GameData.coins = GameData.coinsAtLevelStart;

//        // Cập nhật lên thanh máu (UI Toolkit)
//        if (UIManager.Instance != null)
//        {
//            UIManager.Instance.UpdateUI();
//        }

//        Debug.Log("Mario đã chết. Mạng còn lại: " + GameData.lives);
//    }

//    private void UpdateSprite()
//    {
//        if (SpriteRenderer == null) SpriteRenderer = GetComponent<SpriteRenderer>();

//        SpriteRenderer.enabled = true;
//        SpriteRenderer.sortingOrder = 10; // Đảm bảo xác Mario hiện lên trên cùng

//        if (DeadSprite != null)
//            SpriteRenderer.sprite = DeadSprite;
//    }

//    private void DisablePhysics()
//    {
//        // Tắt TẤT CẢ collider trên người Mario và các object con (như Small/Big)
//        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
//        foreach (Collider2D col in allColliders)
//        {
//            col.enabled = false;
//        }

//        Rigidbody2D rb = GetComponent<Rigidbody2D>();
//        if (rb != null)
//        {
//            rb.linearVelocity = Vector2.zero;
//            rb.bodyType = RigidbodyType2D.Kinematic;
//        }

//        // Tắt script di chuyển
//        PlayerMovement pm = GetComponent<PlayerMovement>();
//        if (pm != null) pm.enabled = false;
//    }

//    private IEnumerator Animate()
//    {
//        // Đảm bảo vị trí không đổi khi bắt đầu diễn
//        Vector3 deathPosition = transform.position;
//        float elapsed = 0f;
//        float duration = 2f;

//        while (elapsed < duration)
//        {
//            // Ép vị trí Mario đứng yên tại chỗ chết, không cho phép bay đi đâu cả
//            transform.position = deathPosition;

//            elapsed += Time.deltaTime;
//            yield return null;
//        }

//        ReloadLevel();
//    }

//    private void ReloadLevel()
//    {
//        if (GameData.lives > 0)
//        {
//            // CÒN MẠNG: Cho chơi lại màn hiện tại để gỡ gạc
//            GameData.coins = GameData.coinsAtLevelStart; // Reset tiền của màn này
//            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//        }
//        else
//        {
//            // HẾT MẠNG: Game Over thực sự!
//            Debug.Log("GAME OVER - Quay về vạch xuất phát!");

//            // Reset toàn bộ dữ liệu như mới
//            GameData.lives = 5;
//            GameData.coins = 0;
//            GameData.coinsAtLevelStart = 0;

//            // Đá người chơi về Map 1 (Giả sử tên scene Map 1 của ông là "Map1")
//            SceneManager.LoadScene("Map1");
//        }
//    }
//}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathAnimation : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;
    public Sprite DeadSprite;

    [Header("Cung bay khi chết (giống DeathAnimationCustom)")]
    [SerializeField] private float deathArcDuration = 3f;
    [SerializeField] private float jumpVelocity = 10f;
    [SerializeField] private float gravity = -36f;

    [Header("Tùy chọn Animator")]
    [Tooltip("Nếu có, gọi SetTrigger khi bắt đầu chết (sprite vẫn có thể dùng DeadSprite).")]
    [SerializeField] private Animator deathAnimator;
    [SerializeField] private string deathAnimatorTrigger = "";

    private bool isAlreadyDead = false; // CHỐT AN TOÀN

    private void Reset()
    {
        if (SpriteRenderer == null)
            SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        // Nếu đã đang trong trạng thái chết thì không chạy lại nữa
        if (isAlreadyDead) return;
        isAlreadyDead = true;

        HandleLifeAndUI();
        UpdateSprite();
        DisablePhysics();
        // PlayerSpriteRender.OnDisable() sets spriteRenderer.enabled = false — show dead sprite again.
        UpdateSprite();
        PlayDeathAnimatorIfAny();
        StartCoroutine(Animate());
    }

    private void PlayDeathAnimatorIfAny()
    {
        if (deathAnimator == null) return;
        if (string.IsNullOrEmpty(deathAnimatorTrigger)) return;
        deathAnimator.SetTrigger(deathAnimatorTrigger);
    }

    private void HandleLifeAndUI()
    {
        GameData.lives--;
        GameData.coins = GameData.coinsAtLevelStart;

        LifeManager.RefreshAllLifeUI();
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateUI();

        Debug.Log("Mario đã chết. Mạng còn lại: " + GameData.lives);
    }

    private void UpdateSprite()
    {
        if (SpriteRenderer == null) SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (SpriteRenderer != null)
        {
            SpriteRenderer.gameObject.SetActive(true);
            SpriteRenderer.enabled = true;
            SpriteRenderer.sortingOrder = 100;
            if (DeadSprite != null) SpriteRenderer.sprite = DeadSprite;
        }
    }

    private void DisablePhysics()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        PlayerMovementTran pmt = GetComponent<PlayerMovementTran>();
        if (pmt != null) pmt.enabled = false;

        MarioMovement marioMovement = GetComponent<MarioMovement>();
        if (marioMovement != null) marioMovement.enabled = false;

        EntityMovement entityMovement = GetComponent<EntityMovement>();
        if (entityMovement != null) entityMovement.enabled = false;

        foreach (PlayerSpriteRender psr in GetComponentsInChildren<PlayerSpriteRender>(true))
            psr.enabled = false;
        foreach (AnimatedSprite anim in GetComponentsInChildren<AnimatedSprite>(true))
            anim.enabled = false;
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.1f, deathArcDuration);
        Vector3 velocity = Vector3.up * jumpVelocity;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        while (elapsed < duration)
        {
            yield return new WaitForFixedUpdate();
            float dt = Time.fixedDeltaTime;
            Vector2 delta = (Vector2)(velocity * dt);
            if (rb != null)
                rb.MovePosition(rb.position + delta);
            else
                transform.position += (Vector3)delta;
            velocity.y += gravity * dt;
            elapsed += dt;
        }

        isAlreadyDead = false;
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

            CheckpointManager.ClearCheckpoint();

            SceneManager.LoadScene(sceneName);
        }
    }
}