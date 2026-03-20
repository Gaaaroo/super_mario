using UnityEngine;

public class FlagPole : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FindAnyObjectByType<LevelLoader>().LoadNextLevel();
        }
    }
}
