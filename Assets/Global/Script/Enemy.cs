using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public Vector2 direction = Vector2.left;

    [Header("Patrol (đi qua lại Left ↔ Right)")]
    public bool usePatrolBounds;
    public float patrolLeft;
    public float patrolRight;
    public int sortingOrder = 5;

    [Header("Stomp")]
    [Tooltip("Sprite when stomped (e.g. Goomba_Flat). Leave empty to just disable and destroy.")]
    public Sprite stompedSprite;
    public float destroyDelay = 0.5f;

    private new Rigidbody2D rigidbody;
    private new Collider2D collider;
    private SpriteRenderer spriteRenderer;
    private bool isStomped;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = sortingOrder;
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        rigidbody.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (isStomped) return;

        if (usePatrolBounds)
        {
            if (rigidbody.position.x <= patrolLeft) { direction.x = 1f; }
            if (rigidbody.position.x >= patrolRight) { direction.x = -1f; }
        }

        rigidbody.linearVelocity = new Vector2(direction.x * moveSpeed, rigidbody.linearVelocity.y);
        if (spriteRenderer != null)
            spriteRenderer.flipX = direction.x > 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isStomped) return;
        if (collision.gameObject.CompareTag("Player")) return;

        if (collision.contacts.Length > 0)
        {
            Vector2 normal = collision.contacts[0].normal;
            if (Mathf.Abs(normal.x) > 0.5f)
                direction.x = -direction.x;
        }
    }

    public void Stomp()
    {
        if (isStomped) return;
        isStomped = true;

        collider.enabled = false;
        if (rigidbody != null)
            rigidbody.simulated = false;

        if (stompedSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = stompedSprite;

        var anim = GetComponent<AnimatedSprite>();
        if (anim != null) anim.enabled = false;

        Destroy(gameObject, destroyDelay);
    }

    public bool IsStomped => isStomped;
}
