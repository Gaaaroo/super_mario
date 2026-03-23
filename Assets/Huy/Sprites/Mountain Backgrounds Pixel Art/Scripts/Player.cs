using UnityEngine;

namespace MountainBackgroundsPixelArt
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        private Rigidbody2D rb;

        [SerializeField]
        [Range(2.0f, 8.0f)]
        private float speed = 4.0f;

        [SerializeField]
        [Range(10.0f, 20.0f)]
        private float jumpForce = 16.0f;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");

            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);

            if (Input.GetButtonDown("Jump"))
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }

            if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            }
        }
    }
}

