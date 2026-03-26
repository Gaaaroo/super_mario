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

    [Header("Boss / arena cố định")]
    [Tooltip("Bật: không theo player, giữ đúng vị trí camera đặt trong scene (chỉnh X/Y trong Inspector).")]
    [SerializeField] private bool lockToFixedPosition;

    [Tooltip("Đặt > 0 để zoom (orthographic size) khi vào scene — ví dụ boss room rộng hơn. 0 = giữ nguyên Camera.")]
    [SerializeField] private float orthographicSizeOverride;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera != null && orthographicSizeOverride > 0f)
            _camera.orthographicSize = orthographicSizeOverride;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void LateUpdate()
    {
        if (lockToFixedPosition) return;
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
