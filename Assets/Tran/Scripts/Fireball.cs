using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 8f;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * speed;
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (!hitInfo.CompareTag("Player"))
            return;

        // Prefab Mario có thể dùng PlayerMovementTran (một số scene) hoặc PlayerMovement (prefab mặc định).
        PlayerMovementTran tran = hitInfo.GetComponentInParent<PlayerMovementTran>();
        if (tran != null)
            tran.DieFromCrushOrHazard();
        else
        {
            PlayerMovement movement = hitInfo.GetComponentInParent<PlayerMovement>();
            if (movement != null)
                movement.DieFromCrushOrHazard();
        }

        Destroy(gameObject);
    }
}