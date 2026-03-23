using System.Collections;
using UnityEngine;

public class FlagPole : MonoBehaviour
{
    public Transform flag;
    public Transform poleBottom;
    public Transform castle;
    public float speed = 6f;

    private AudioSource audioSource;
    public AudioClip winSound;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(MoveTo(flag, poleBottom.position));
            StartCoroutine(LevelCompleteSequence(other.transform));

            MusicManager.Instance.SetMusic(false);
            PlaySound(winSound);
        }
    }

    private IEnumerator LevelCompleteSequence(Transform player)
    {
        player.GetComponent<PlayerMovement>().enabled = false;

        yield return MoveTo(player, poleBottom.position);
        yield return MoveTo(player, player.position + Vector3.right);
        yield return MoveTo(player, player.position + Vector3.right + Vector3.down);
        yield return MoveTo(player, castle.position);

        player.gameObject.SetActive(false);
    }

    private IEnumerator MoveTo(Transform subject, Vector3 destination)
    {
        while (Vector3.Distance(subject.position, destination) > 0.125f) 
        {
            subject.position = Vector3.MoveTowards(subject.position, destination, speed * Time.deltaTime);
            yield return null;
        }

        subject.position = destination;
    }

    private void PlaySound(AudioClip audioClip)
    {
        if (audioClip == null) return;
        audioSource.PlayOneShot(audioClip);
    }
}
