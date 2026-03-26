using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Hiển thị / đồng bộ mạng với <see cref="GameData.lives"/> (cùng nguồn với PlayerMovement, DeathAnimation, v.v.).
/// </summary>
public class LifeManager : MonoBehaviour
{
    public TextMeshProUGUI livesText;

    private static bool s_warnedMissingLivesText;

    private void OnEnable()
    {
        BindTextIfNeeded();
        RefreshLivesText();
    }

    private void Start()
    {
        BindTextIfNeeded();
        RefreshLivesText();
    }

    private void BindTextIfNeeded()
    {
        if (livesText != null)
            return;

        GameObject textObject = GameObject.Find("LivesText");
        if (textObject != null)
            livesText = textObject.GetComponent<TextMeshProUGUI>();
        else if (UIManager.Instance == null && !s_warnedMissingLivesText)
        {
            // Có UIManager thì mạng đã hiện bằng tim (UI Toolkit); chữ TMP "LivesText" là tùy chọn → không cảnh báo.
            s_warnedMissingLivesText = true;
            Debug.LogWarning("LifeManager: Không tìm thấy GameObject tên 'LivesText'. Gán trường livesText trên LifeManager hoặc thêm TMP đặt tên đúng.");
        }
    }

    public void RefreshLivesText()
    {
        BindTextIfNeeded();
        if (livesText != null)
            livesText.text = "LIVES: " + GameData.lives;
    }

    /// <summary>Gọi sau mỗi lần đổi <see cref="GameData.lives"/> (ví dụ từ <see cref="UIManager.UpdateUI"/>).</summary>
    public static void SyncAllLivesLabels()
    {
        foreach (LifeManager lm in Object.FindObjectsByType<LifeManager>(FindObjectsSortMode.None))
            lm.RefreshLivesText();
    }

    /// <summary>Cập nhật xu + tim (UI Toolkit) + chữ LIVES (TMP) sau khi đổi <see cref="GameData"/>.</summary>
    public static void RefreshAllLifeUI()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateUI();
        else
            SyncAllLivesLabels();
    }

    /// <summary>UnityEvent / trigger — trừ 1 mạng trong GameData, cập nhật mọi UI.</summary>
    public void LoseLife()
    {
        if (GameData.lives <= 0)
            return;

        GameData.lives--;
        GameData.coins = GameData.coinsAtLevelStart;

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateUI();
        else
            SyncAllLivesLabels();

        if (GameData.lives <= 0)
            ApplyGameOverThenReload();
    }

    private static void ApplyGameOverThenReload()
    {
        Debug.Log("GAME OVER! Reset mạng / xu, load lại màn.");
        GameData.lives = 5;
        GameData.coins = 0;
        GameData.coinsAtLevelStart = 0;
        CheckpointManager.ClearCheckpoint();

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateUI();
        else
            SyncAllLivesLabels();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
