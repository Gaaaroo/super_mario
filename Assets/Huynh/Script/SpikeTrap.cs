using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpikeTrap : MonoBehaviour
{
    [Tooltip("Transform của sprite gai. BẮT BUỘC phải gán (thường là child SpikeSprite).")]
    public Transform spike;

    [Tooltip("Khoảng dịch xuống dưới (local Y) để ẩn gai lúc ban đầu.")]
    public float hideDistance = 1f;

    [Tooltip("Thời gian gai trồi lên.")]
    public float riseTime = 0.1f;

    private Vector3 shownLocalPos;
    private Vector3 hiddenLocalPos;
    private bool triggered;

    private void Awake()
    {
        if (spike == null)
            spike = transform;

        shownLocalPos = spike.localPosition;
        hiddenLocalPos = shownLocalPos + Vector3.down * hideDistance;
        spike.localPosition = hiddenLocalPos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;
        StartCoroutine(RiseSpike());
    }

    private IEnumerator RiseSpike()
    {
        float elapsed = 0f;

        while (elapsed < riseTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / riseTime);
            spike.localPosition = Vector3.Lerp(hiddenLocalPos, shownLocalPos, t);
            yield return null;
        }
    }
}

