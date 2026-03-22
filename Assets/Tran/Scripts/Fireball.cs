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
        if (hitInfo.CompareTag("Player"))
        {
            // Vi du: hitInfo.GetComponent<MarioHealth>().LoseLife();
            Debug.Log("Mario bị trúng đạn! -1 Live");
            Destroy(gameObject);
        }
    }
}