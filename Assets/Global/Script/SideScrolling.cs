using UnityEngine;
using UnityEngine.Serialization;

public class SideScrolling : MonoBehaviour
{
    private Transform player;

    [Header("Vùng camera lùi + tới (Min/Max X)")]
    [Tooltip("Tick: bật dùng Min/Max X. Bỏ tick: không dùng Min/Max, camera chỉ tiến (Mario cổ điển).")]
    [FormerlySerializedAs("useBidirectionalBand")]
    [SerializeField] private bool useBandMinMax;

    [Tooltip("Cận trái vùng camera hai chiều (mặc định 0). Chỉ có hiệu lực khi Use Band Min Max được bật.")]
    [SerializeField] private float bandMinX;

    [Tooltip("Cận phải vùng camera hai chiều (mặc định 0). Chỉ có hiệu lực khi Use Band Min Max được bật.")]
    [SerializeField] private float bandMaxX;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 cameraPosition = transform.position;
        float px = player.position.x;

        if (useBandMinMax)
        {
            float lo = Mathf.Min(bandMinX, bandMaxX);
            float hi = Mathf.Max(bandMinX, bandMaxX);
            if (px >= lo && px <= hi)
                cameraPosition.x = Mathf.Clamp(px, lo, hi);
            else
                cameraPosition.x = Mathf.Max(cameraPosition.x, px);
        }
        else
        {
            cameraPosition.x = Mathf.Max(cameraPosition.x, px);
        }

        transform.position = cameraPosition;
    }
}
