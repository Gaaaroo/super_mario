using UnityEngine;

public class GoombaCustom : MonoBehaviour
{
    public Sprite FlatSprite;

    private AudioSource audioSource;
    public AudioClip dieSound;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with the player and if the player is above the goomba
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerCustom player = collision.gameObject.GetComponent<PlayerCustom>();

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

        PlaySound(dieSound);

        Destroy(gameObject, 1f);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}
