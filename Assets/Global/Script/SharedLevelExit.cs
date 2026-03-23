using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SharedLevelExit : MonoBehaviour
{
    [Tooltip("Name of the scene to load (e.g. 1-2). Must match the scene file name and be in Build Settings.")]
    public string nextSceneName = "1-2";

    [Tooltip("If enabled, use the scene index in Build Settings instead of the scene name.")]
    public bool useSceneIndex;

    [Tooltip("Scene index in Build Settings (0, 1, 2...). Only used when Use Scene Index is enabled.")]
    public int nextSceneIndex = 2;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Checkpoint chỉ dùng trong một màn; sang màn mới phải xóa kẻo spawn theo tọa độ cờ màn trước.
        CheckpointManager.ClearCheckpoint();

        if (useSceneIndex)
            SceneManager.LoadScene(nextSceneIndex);
        else
            SceneManager.LoadScene(nextSceneName);
    }
}
