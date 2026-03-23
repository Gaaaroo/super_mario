using UnityEngine;

public class GoombaCustom : MonoBehaviour
{
    public Sprite FlatSprite;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with the player and if the player is above the goomba
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();

            if (collision.transform.DotTest(transform, Vector2.down))
                Flatten();
            else
                player.Hit();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Shell"))
            Hit();
    }

    private void Hit()
    {
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<DeathAnimationCustom>().enabled = true;
        Destroy(gameObject, 3f);
    }

    private void Flatten()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<EntityMovement>().enabled = false;
        GetComponent<AnimatedSprite>().enabled = false;

        GetComponent<SpriteRenderer>().sprite = FlatSprite;

        Destroy(gameObject, 0.5f);
    }
}
