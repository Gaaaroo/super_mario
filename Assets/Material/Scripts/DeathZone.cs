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

        if (spawnPoint != null)
            CheckpointManager.SetCheckpoint(spawnPoint.position);

        ReloadCurrentScene();
    }

    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
