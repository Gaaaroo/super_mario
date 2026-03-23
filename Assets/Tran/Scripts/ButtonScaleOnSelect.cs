using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonScaleOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private Vector3 originalScale;

    [Header("Effect settings")]
    [Range(0.1f, 1.0f)]
    public float selectedScaleFactor = 0.9f;

    public float smoothTime = 0.1f;
    private Vector3 targetScale;
    private Vector3 velocity = Vector3.zero;

    [Header("Sounds")]
    public AudioClip selectSound;
    public AudioClip clickSound;

    private AudioSource audioSource;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref velocity, smoothTime);
    }

    public void OnSelect(BaseEventData eventData)
    {
        targetScale = originalScale * selectedScaleFactor;

        if (selectSound != null)
        {
            audioSource.PlayOneShot(selectSound);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    private void OnDisable()
    {
        transform.localScale = originalScale;
    }
}