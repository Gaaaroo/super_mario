using UnityEngine;
using System.Collections;

public class PlayerPowerUpManager : MonoBehaviour
{
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void CollectPowerUp(PowerUpType type, float duration)
    {
        Debug.Log("Mario đã ăn: " + type);

        switch (type)
        {
            case PowerUpType.Invisibility:
                StartCoroutine(InvisibilityRoutine(duration));
                break;

            //case PowerUpType.Gun:
            //    ActivateGun();
            //    break;

            case PowerUpType.ExtraLife:
                AddLife();
                break;
        }
    }

    // --- LOGIC CHI TIẾT TỪNG LOẠI ---
    IEnumerator InvisibilityRoutine(float duration)
    {
        GameData.isInvincible = true;

        // BẬT NHẠC
        if (UIManager.Instance != null) UIManager.Instance.StartStarmanMusic();

        // 1. Lấy ID của 2 lớp
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        // 2. BẬT XUYÊN THẤU: Mario và bất cứ thứ gì ở Layer Enemy sẽ đi xuyên qua nhau
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();

        float elapsed = 0;
        while (elapsed < duration)
        {
            // Hiệu ứng nhấp nháy mờ
            foreach (var s in allRenderers) if (s != null) s.color = new Color(1, 1, 1, 0.3f);
            yield return new WaitForSeconds(0.1f);
            foreach (var s in allRenderers) if (s != null) s.color = new Color(1, 1, 1, 0.8f);
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }

        // 3. TẮT XUYÊN THẤU: Trở lại bình thường, đụng quái/gai là chết
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

        foreach (var s in allRenderers) if (s != null) s.color = Color.white;

        if (UIManager.Instance != null) UIManager.Instance.StopStarmanMusic();

        GameData.isInvincible = false;
    }
    //IEnumerator InvisibilityRoutine(float duration)
    //{
    //    GameData.isInvincible = true;

    //    // Lấy TẤT CẢ SpriteRenderer ở các con (Small, Big, v.v.)
    //    SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();

    //    float elapsed = 0;
    //    while (elapsed < duration)
    //    {
    //        // Nhấp nháy mờ + đổi sang màu vàng nhạt cho dễ thấy
    //        foreach (var s in allRenderers)
    //        {
    //            if (s != null) s.color = new Color(1f, 1f, 0.5f, 0.3f);
    //        }
    //        yield return new WaitForSeconds(0.1f);

    //        // Hiện rõ lại một chút
    //        foreach (var s in allRenderers)
    //        {
    //            if (s != null) s.color = new Color(1f, 1f, 1f, 0.8f);
    //        }
    //        yield return new WaitForSeconds(0.1f);

    //        elapsed += 0.2f;
    //    }

    //    // KẾT THÚC: Trả lại màu trắng chuẩn (Alpha = 1) cho tất cả
    //    foreach (var s in allRenderers)
    //    {
    //        if (s != null) s.color = Color.white;
    //    }

    //    GameData.isInvincible = false;
    //    Debug.Log("Hết tàng hình!");
    //}

    //private void ActivateGun()
    //{
    //    GameData.hasGun = true;
    //    sr.color = Color.red; // Đổi sang Mario đỏ chẳng hạn
    //    // Kích hoạt vật thể cây súng trên tay Mario nếu có
    //}

    private void AddLife()
    {
        GameData.lives++;
        if (UIManager.Instance != null) UIManager.Instance.UpdateUI();
    }
}