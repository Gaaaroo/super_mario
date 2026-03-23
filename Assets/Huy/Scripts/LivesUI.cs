using UnityEngine;
using TMPro;

public class LivesUI : MonoBehaviour
{
    public TMP_Text livesText;

    void Update()
    {
        livesText.text = "x " + PlayerHealth.Instance.Lives;
    }
}