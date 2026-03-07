using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public Health bossHealth;
    public Image healthFill;

    private void Update()
    {
        float ratio = (float)bossHealth.currentHealth / bossHealth.maxHealth;
        healthFill.fillAmount = ratio;
    }
}