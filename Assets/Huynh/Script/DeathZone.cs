using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DeathZone : MonoBehaviour
{
    [Header("Cài đặt loại vùng chết")]
    [Tooltip("Tích vào nếu đây là Hố Sâu (Tàng hình vẫn chết). Bỏ tích nếu là Gai/Lửa (Tàng hình không chết).")]
    public bool isInstantDeathPit = true;

    [Tooltip("If set, player respawns here. If empty, current scene is reloaded.")]
    public Transform spawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!isInstantDeathPit && GameData.isInvincible)
        {
            Debug.Log("Mario đang tàng hình, đi xuyên qua vật cản!");
            return;
        }

        DeathAnimation deathScript = other.GetComponent<DeathAnimation>();

        if (deathScript != null)
        {
            if (deathScript.enabled == false)
            {
                if (spawnPoint != null)
                    CheckpointManager.SetCheckpoint(spawnPoint.position);

                deathScript.enabled = true;
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
