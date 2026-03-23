using UnityEngine;

public class PlayerSpriteRenderCustom : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private MarioMovement marioMovement;
    private PlayerMovement playerMovement;

    public Sprite idle;
    public Sprite jump;
    public Sprite slide;
    public AnimatedSprite run;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        marioMovement = GetComponentInParent<MarioMovement>();
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    private bool Running => marioMovement != null ? marioMovement.running : playerMovement != null && playerMovement.running;
    private bool Jumping => marioMovement != null ? marioMovement.jumping : playerMovement != null && playerMovement.jumping;
    private bool Sliding => marioMovement != null ? marioMovement.sliding : playerMovement != null && playerMovement.sliding;

    private void OnEnable()
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }

    private void OnDisable()
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        if (run != null)
            run.enabled = false;
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null || run == null)
            return;
        if (marioMovement == null && playerMovement == null)
            return;

        run.enabled = Running;
        if (Jumping)
            spriteRenderer.sprite = jump;
        else if (Sliding)
            spriteRenderer.sprite = slide;
        else if (!Running)
            spriteRenderer.sprite = idle;
    }
}
