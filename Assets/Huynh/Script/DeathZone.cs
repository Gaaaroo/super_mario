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
            DeathAnimation deathScript = other.GetComponent<DeathAnimation>();

            if (deathScript != null)
            {
                if (spawnPoint != null)
                    CheckpointManager.SetCheckpoint(spawnPoint.position);

                deathScript.enabled = true;
                // DeathAnimation tự LoadScene sau khi chạy xong — không reload ngay (tránh cắt anim / thời gian chết lúc nhanh lúc chậm).
                return;
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
