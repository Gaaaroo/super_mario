using UnityEngine;

public class KoopaCustom : MonoBehaviour
{
    public Sprite ShellSprite;
    public float ShellSpeed = 12f;

    private bool _shelled;
    private bool _pushed;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with the player and if the player is above the goomba
        if (!_shelled && collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();

            if (collision.transform.DotTest(transform, Vector2.down))
                EnterShell();
            else
                player.Hit();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_shelled && other.CompareTag("Player"))
        {
            if (!_pushed)
            {
                Vector2 direction = new(transform.position.x - other.transform.position.x, 0f);
                PushShell(direction);
            }
            else
            {
                Player player = other.GetComponent<Player>();
                player.Hit();
            }
        }
        else if (!_shelled && other.gameObject.layer == LayerMask.NameToLayer("Shell"))
            Hit();
    }

    private void PushShell(Vector2 direction)
    {
        _pushed = true;

        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

        EntityMovement movement = GetComponent<EntityMovement>();
        movement.Direction = direction.normalized;
        movement.Speed = ShellSpeed;
        movement.enabled = true;

        gameObject.layer = LayerMask.NameToLayer("Shell");
    }

    private void EnterShell()
    {
        _shelled = true;

        GetComponent<EntityMovement>().enabled = false;
        GetComponent<AnimatedSprite>().enabled = false;

        GetComponent<SpriteRenderer>().sprite = ShellSprite;
    }

    private void Hit()
    {
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<DeathAnimationCustom>().enabled = true;
        Destroy(gameObject, 3f);
    }
}