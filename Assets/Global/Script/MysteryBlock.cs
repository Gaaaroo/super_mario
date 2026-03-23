using UnityEngine;
using System.Collections;

public class MysteryBlock : MonoBehaviour
{
    public GameObject itemPrefab;      // Kéo Prefab Đồng xu hoặc Nấm vào đây
    public Sprite emptySprite;        // Hình cái gạch sau khi đã ăn hết đồ
    public GameObject hitParticle;    // Kéo Prefab BlockSmoke vào đây

    public int maxItems = 1;          // Số lượng đồ trong gạch
    private bool isAnimating = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (maxItems <= 0) return;

        // Kiểm tra nếu Mario húc từ dưới lên
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f) // Hướng từ dưới lên
                {
                    StartCoroutine(HitSequence());
                    break;
                }
            }
        }
    }

    private IEnumerator HitSequence()
    {
        isAnimating = true;
        maxItems--;

        // 1. Hiện hiệu ứng bụi
        if (hitParticle != null)
            Instantiate(hitParticle, transform.position, Quaternion.identity);

        // 2. Hiệu ứng nẩy gạch (Move up then down)
        Vector3 restingPosition = transform.localPosition;
        Vector3 animatedPosition = restingPosition + Vector3.up * 0.5f;

        yield return Move(restingPosition, animatedPosition); // Nẩy lên

        // 3. Tạo vật phẩm (Coin/Nấm)
        SpawnItem();

        yield return Move(animatedPosition, restingPosition); // Rơi về chỗ cũ

        // 4. Nếu hết đồ thì đổi hình sang gạch trống
        if (maxItems <= 0)
        {
            GetComponent<SpriteRenderer>().sprite = emptySprite;
        }

        isAnimating = false;
    }

    private IEnumerator Move(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;
        float duration = 0.125f;
        while (elapsed < duration)
        {
            transform.localPosition = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = to;
    }

    private void SpawnItem()
    {
        if (itemPrefab != null)
        {
            // Tạo vật phẩm ngay tại vị trí gạch
            Instantiate(itemPrefab, transform.position, Quaternion.identity);

            //// Nếu là tiền, tự cộng vào GameData luôn cho tiện
            //if (itemPrefab.name.Contains("Coin"))
            //{
            //    GameData.coins++;
            //    if (UIManager.Instance != null) UIManager.Instance.UpdateUI();
            //}
        }
    }
}