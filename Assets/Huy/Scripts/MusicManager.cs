using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void ToggleMusic()
    {
        audioSource.mute = !audioSource.mute;
    }

    public void SetMusic(bool on)
    {
        audioSource.mute = !on;
    }

    public void SetMusic(bool on, float delay)
    {
        StartCoroutine(SetMusicDelayed(on, delay));
    }

    private System.Collections.IEnumerator SetMusicDelayed(bool on, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetMusic(on);
    }
}