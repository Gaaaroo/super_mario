using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpikeTrap : MonoBehaviour
{
    [Tooltip("Transform của sprite gai. BẮT BUỘC phải gán (thường là child SpikeSprite).")]
    public Transform spike;

    [Tooltip("Khoảng dịch xuống dưới (local Y) để ẩn gai lúc ban đầu.")]
    public float hideDistance = 1f;

    [Tooltip("Khoảng cách (local Y) gai bay thêm phía trên vị trí hiển thị sau khi trồi lên. 0 = dừng đúng vị trí thiết kế.")]
    public float extraFlyDistance = 0f;

    [Tooltip("Dịch thêm spike theo local -X (trái). 0 = không dịch.")]
    public float shiftLeft = 0f;

    [Tooltip("Dịch thêm spike theo local +X (phải). 0 = không dịch.")]
    public float shiftRight = 0f;

    [Tooltip("Dịch thêm spike theo local -Y (xuống dưới) tại vị trí cuối, giống shift trái/phải. 0 = không dịch.")]
    public float shiftDown = 0f;

    [Tooltip("Thời gian gai trồi lên.")]
    public float riseTime = 0.1f;

    [Tooltip("Tick vào thì khi Player chạm trigger, gai sẽ phóng to theo hệ số bên dưới. Mặc định tắt = gai giữ kích thước bình thường.")]
    public bool scaleUpOnTrigger = false;

    [Tooltip("Nhân kích thước localScale khi Scale Up On Trigger được bật (ví dụ 3 = to gấp 3).")]
    public float triggeredScaleMultiplier = 3f;

    [Tooltip("Thời gian (giây) để gai phóng to hết hệ số. Chỉ khi Scale Up On Trigger bật. 0 hoặc âm = dùng chung Rise Time.")]
    public float scaleUpTime = 0f;

    [Tooltip("Bật thì ẩn sprite gai lúc đầu; chỉ hiện khi Player chạm Box Collider trigger.")]
    public bool hideSpriteUntilTriggered = false;

    private Vector3 shownLocalPos;
    private Vector3 hiddenLocalPos;
    private Vector3 extendedLocalPos;
    private Vector3 initialLocalScale;
    private Vector3 scaledUpLocalScale;
    private bool triggered;
    private SpriteRenderer[] spikeSpriteRenderers;

    private Vector3 HorizontalLocalOffset => Vector3.right * (shiftRight - shiftLeft);

    private void Awake()
    {
        if (spike == null)
            spike = transform;

        shownLocalPos = spike.localPosition;
        hiddenLocalPos = shownLocalPos + Vector3.down * hideDistance;
        extendedLocalPos = shownLocalPos + Vector3.up * Mathf.Max(0f, extraFlyDistance) + HorizontalLocalOffset + Vector3.down * shiftDown;
        initialLocalScale = spike.localScale;
        scaledUpLocalScale = initialLocalScale * Mathf.Max(0.01f, triggeredScaleMultiplier);
        spike.localPosition = hiddenLocalPos;

        spikeSpriteRenderers = spike.GetComponentsInChildren<SpriteRenderer>(true);
        if (hideSpriteUntilTriggered)
            SetSpikeSpritesVisible(false);
    }

    private void SetSpikeSpritesVisible(bool visible)
    {
        if (spikeSpriteRenderers == null)
            return;
        for (int i = 0; i < spikeSpriteRenderers.Length; i++)
        {
            if (spikeSpriteRenderers[i] != null)
                spikeSpriteRenderers[i].enabled = visible;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;
        if (hideSpriteUntilTriggered)
            SetSpikeSpritesVisible(true);
        StartCoroutine(RiseSpike());
    }

    private IEnumerator RiseSpike()
    {
        float elapsed = 0f;
        scaledUpLocalScale = initialLocalScale * Mathf.Max(0.01f, triggeredScaleMultiplier);
        extendedLocalPos = shownLocalPos + Vector3.up * Mathf.Max(0f, extraFlyDistance) + HorizontalLocalOffset + Vector3.down * shiftDown;

        float riseDuration = Mathf.Max(0.0001f, riseTime);
        float scaleDuration = scaleUpOnTrigger
            ? Mathf.Max(0.0001f, scaleUpTime > 0f ? scaleUpTime : riseTime)
            : 0f;
        float totalDuration = Mathf.Max(riseDuration, scaleDuration);

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            float posT = Mathf.Clamp01(elapsed / riseDuration);
            spike.localPosition = Vector3.Lerp(hiddenLocalPos, extendedLocalPos, posT);

            if (scaleUpOnTrigger)
            {
                float scaleT = Mathf.Clamp01(elapsed / scaleDuration);
                spike.localScale = Vector3.Lerp(initialLocalScale, scaledUpLocalScale, scaleT);
            }
            else
                spike.localScale = initialLocalScale;

            yield return null;
        }

        spike.localPosition = extendedLocalPos;
        spike.localScale = scaleUpOnTrigger ? scaledUpLocalScale : initialLocalScale;
    }
}

