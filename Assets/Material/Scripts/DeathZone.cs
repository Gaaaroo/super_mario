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
            RespawnAtPoint(other.gameObject);
        else
            ReloadCurrentScene();
    }

    private void RespawnAtPoint(GameObject player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.RespawnAt(spawnPoint.position);
            return;
        }
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = spawnPoint.position;
        }
        else
            player.transform.position = spawnPoint.position;
    }

    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
