using TMPro;
using UnityEngine;

namespace Assets.Huy.Scripts
{
    public class PointUI : MonoBehaviour
    {
        public TMP_Text livesText;

        void Update()
        {
            livesText.text = PlayerHealth.Instance.Points.ToString();
        }
    }
}
