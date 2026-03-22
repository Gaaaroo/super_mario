using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DeathZone : MonoBehaviour
{
    [Tooltip("If set, player respawns here. If empty, current scene is reloaded.")]
    public Transform spawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // 1. Kiểm tra xem có phải Mario (Tag: Player) đụng vào không
        if (other.CompareTag("Player"))
        {
            // 2. Tìm script DeathAnimation nằm trên người Mario
            DeathAnimation deathScript = other.GetComponent<DeathAnimation>();

            if (deathScript != null)
            {
                // 3. BẬT script đó lên
                // Khi script này ON, nó sẽ tự chạy hàm OnEnable:
                // - Trừ GameData.lives
                // - Cập nhật UIManager
                // - Diễn hoạt ảnh Mario nằm im/văng lên
                // - Tự load lại scene sau 2-3 giây
                deathScript.enabled = true;
            }
        }

        if (spawnPoint != null)
            CheckpointManager.SetCheckpoint(spawnPoint.position);

        ReloadCurrentScene();
    }

    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
