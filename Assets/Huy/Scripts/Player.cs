using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerSpriteRender SmallRender;
    public PlayerSpriteRender BigRender;

    private DeathAnimation _deathAnimation;

    public bool Big => BigRender.enabled;
    public bool Small => SmallRender.enabled;
    public bool Dead => _deathAnimation.enabled;

    private void Awake()
    {
        _deathAnimation = GetComponent<DeathAnimation>();
    }

    public void Hit()
    {
        if (Big)
            Shrink();
        else
            Death();
    }

    private void Death()
    {
        SmallRender.enabled = false;
        BigRender.enabled = false;
        _deathAnimation.enabled = true;

        GameManager.Instance.ResetLevel(3f);
    }

    private void Shrink()
    {
        // TODO
    }
}
