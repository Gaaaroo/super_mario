using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DDeathZone : MonoBehaviour
{
    [Tooltip("If set, player respawns here. If empty, current scene is reloaded.")]
    public Transform spawnPoint;

    ////hàm chạy khi có va chạm với collider khác
    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    //ko phải player thì kệ
    //    if (!other.CompareTag("Player")) 
    //        return;

    //    //nếu có spawnPoint thì quay lại spawnPoint
    //    if (spawnPoint != null)
    //        CheckpointManager.SetCheckpoint(spawnPoint.position);

    //    ReloadCurrentScene();
    //}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tìm script PlayerDeath trên người Mario và gọi hàm Die()
            DPlayerDeath deathScript = other.GetComponent<DPlayerDeath>();
            if (deathScript != null)
            {
                deathScript.Die();
            }
            else
            {
                // Nếu quên chưa gắn script Death thì thôi load scene luôn cho chắc
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    //hàm reload lại scene hiện tại
    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
