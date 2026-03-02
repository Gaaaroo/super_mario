using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteRender : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private PlayerMovement movement;

    public Sprite idle;
    public Sprite jump;
    public Sprite slide;
    public AnimatedSprite run;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponentInParent<PlayerMovement>();
    }

    private void OnEnable()
    {
        spriteRenderer.enabled = true;
    }

    private void OnDisable()
    {
        spriteRenderer.enabled = false;
        run.enabled = false;
    }

    private void LateUpdate()
    {
        run.enabled = movement.running;
        if (movement.jumping)
            spriteRenderer.sprite = jump;
        else if (movement.sliding)
            spriteRenderer.sprite = slide;
        else if (!movement.running)
            spriteRenderer.sprite = idle;
    }
}
