using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LifeManager : MonoBehaviour
{
    public static int lives = 5;

    public TextMeshProUGUI livesText;

    void Start()
    {
        if (livesText == null)
        {
            GameObject textObject = GameObject.Find("LivesText");
            if (textObject != null)
            {
                livesText = textObject.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogWarning("Not found");
            }
        }

        UpdateUI();
    }

    public void LoseLife()
    {
        lives--;
        UpdateUI();

        if (lives <= 0)
        {
            GameOver();
        }
    }

    void UpdateUI()
    {
        if (livesText != null)
        {
            livesText.text = "LIVES: " + lives;
        }
    }

    void GameOver()
    {
        Debug.Log("GAME OVER!");

        lives = 5;

        SceneManager.LoadScene(3);
    }
}