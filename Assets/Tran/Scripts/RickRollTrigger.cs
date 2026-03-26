using UnityEngine;
using UnityEngine.Video;

public class RickRollTrigger : MonoBehaviour
{
    public GameObject videoScreen;
    public VideoPlayer videoPlayer;

    [Tooltip("Nút \"reward\" — ẩn khi video chạy, hiện lại khi tắt video.")]
    public GameObject rewardButton;

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += TatVideo;
        }
    }

    void Update()
    {
        if (videoScreen != null && videoScreen.activeSelf && Input.GetMouseButtonDown(0))
        {
            TatVideo(videoPlayer);
        }
    }

    public void BatVideoLen()
    {
        if (rewardButton != null) rewardButton.SetActive(false);
        if (videoScreen != null) videoScreen.SetActive(true);
        if (videoPlayer != null) videoPlayer.Play();
    }

    void TatVideo(VideoPlayer vp)
    {
        if (videoPlayer != null) videoPlayer.Stop();

        if (videoScreen != null) videoScreen.SetActive(false);
        if (rewardButton != null) rewardButton.SetActive(true);
    }
}