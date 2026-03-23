using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    public int World { get; private set; }
    public int Stage { get; private set; }
    public int Lives { get; private set; }
    public int Points { get; private set; } 

    private void Awake()
    {
        if (Instance != null)
            DestroyImmediate(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        NewGame();
    }

    private void NewGame()
    {
        Lives = 3;
        Points = 0;

        LoadLevel(1, 1);
    }

    private void LoadLevel(int world, int stage)
    {
        World = world;
        Stage = stage;

        SceneManager.LoadScene($"{world}-{stage}");
    }

    public void NextLevel()
    {
        LoadLevel(World, Stage + 1);
    }

    public void TakeDamage(float delay)
    {
        if (Lives > 0)
            Lives--;

        if (Lives <= 1)
            ResetLevelAgain(delay);
    }

    public void ResetLevelAgain(float delay)
    {
        Invoke(nameof(ResetLevelAgain), delay);
    }

    private void ResetLevelAgain()
    {
        Lives = 3;
        Points = 0;
        LoadLevel(World, Stage);
    }

    public void AddPoint()
    {
        Points += 100;
        
        if (Points == 500)
        {
            AddLife();
            Points = 0;
        }
    }

    public void AddLife()
    {
        Lives++;
    }
}
