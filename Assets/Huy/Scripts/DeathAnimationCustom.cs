using System.Collections;
using UnityEngine;

public class DeathAnimationCustom : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;
    public Sprite DeadSprite;

    private void Reset()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        UpdateSprite();
        DisablePhysics();
        StartCoroutine(Animate());
    }

    private void UpdateSprite()
    {
        SpriteRenderer.enabled = true;
        SpriteRenderer.sortingOrder = 10;

        if (DeadSprite != null)
            SpriteRenderer.sprite = DeadSprite;
    }

    private void DisablePhysics()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();

        foreach (Collider2D collider in colliders)
            collider.enabled = false;

        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

        MarioMovement playerMovement = GetComponent<MarioMovement>();
        EntityMovement entityMovement = GetComponent<EntityMovement>();

        if (playerMovement != null)
            playerMovement.enabled = false;
        if (entityMovement != null)
            entityMovement.enabled = false;
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        float duration = 3f;

        float jumpVelocity = 10f;
        float gravity = -36f;

        Vector3 velocity = Vector3.up * jumpVelocity;

        while (elapsed < duration)
        {
            transform.position += velocity * Time.deltaTime;
            velocity.y += gravity * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}