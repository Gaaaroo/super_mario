using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerSpriteRender SmallRender;
    public PlayerSpriteRender BigRender;

    private DeathAnimation _deathAnimation;

    public bool Big => BigRender.enabled;
    public bool Small => SmallRender.enabled;
    public bool Dead => _deathAnimation.enabled;

    private AudioSource audioSource;
    public AudioClip dieSound;

    public float invincibleDuration = 1f;
    private bool isInvincible = false;
    private new Collider2D collider2D;

    private void Awake()
    {
        _deathAnimation = GetComponent<DeathAnimation>();
        audioSource = GetComponent<AudioSource>();
        collider2D = GetComponent<Collider2D>();
    }

    public void Hit()
    {
        if (isInvincible) return; 

        if (Big)
            Shrink();
        else
            Death();
    }

    private void Death()
    {
        int lives = PlayerHealth.Instance.Lives;
        if (lives <= 1)
        {
            SmallRender.enabled = false;
            BigRender.enabled = false;
            _deathAnimation.enabled = true;

            MusicManager.Instance.SetMusic(false);
            PlaySound(dieSound);
        } 
        else
            Respawn();

        PlayerHealth.Instance.TakeDamage(10f);

        // GameManager.Instance.ResetLevel(10f);        
    }

    private void Shrink()
    {
        // TODO
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    public void Respawn()
    {
        StartCoroutine(InvincibleRoutine());
    }

    private System.Collections.IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        collider2D.enabled = false;   // không bị đánh trúng

        float time = 0;
        while (time < invincibleDuration)
        {
            SmallRender.enabled = !SmallRender.enabled;   // nhấp nháy sprite
            time += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // bật lại bình thường
        SmallRender.enabled = true;
        collider2D.enabled = true;
        isInvincible = false;
    }
}
