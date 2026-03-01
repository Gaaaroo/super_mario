using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AnimatedSprite))]
[RequireComponent(typeof(EntityMovement))]
[RequireComponent(typeof(SpriteRenderer))]
public class Goomba : MonoBehaviour
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

    private void Flatten()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<EntityMovement>().enabled = false;
        GetComponent<AnimatedSprite>().enabled = false;

        GetComponent<SpriteRenderer>().sprite = FlatSprite;

        Destroy(gameObject, 0.5f);
    }
}
