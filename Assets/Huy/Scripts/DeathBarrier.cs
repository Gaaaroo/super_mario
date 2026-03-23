using UnityEngine;

public class DeathBarrier : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip dieSound;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.SetActive(false);
            Die();
        }
        else
            Destroy(other.gameObject);
    }

    private void Die()
    {
        MusicManager.Instance.SetMusic(false);

        PlaySound(dieSound);

        PlayerHealth.Instance.ResetLevelAgain(10f);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}
