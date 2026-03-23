using UnityEngine;
using System.Collections;

public class PowerUpPop : MonoBehaviour
{
    public float popHeight = 1f; // Độ cao trồi lên khỏi viên gạch
    public float duration = 0.5f; // Thời gian chui ra

    void Start()
    {
        // Lúc mới sinh ra thì tắt va chạm để không bị vướng vào gạch
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(PopRoutine());
    }

    IEnumerator PopRoutine()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * popHeight;

        float elapsed = 0;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;

        // CHỐT HẠ: Bật va chạm lên để Mario chạy lại "đụng" là ăn được
        GetComponent<Collider2D>().enabled = true;

        Debug.Log("Vật phẩm đã sẵn sàng chờ Mario ăn!");
    }
}