using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace TMPro.Examples
{

    public class SimpleScript : MonoBehaviour
    {

        public void PlayGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        public void QuitGame()
        {
            Debug.Log("Đã thoát game!");
            Application.Quit();
        }

    }
}
