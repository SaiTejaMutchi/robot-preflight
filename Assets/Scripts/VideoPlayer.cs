using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoUIController : MonoBehaviour
{
    public Button playButton;
    public Button pauseButton;
    public VideoPlayer videoPlayer;

    void Start()
    {
        // Assign button listeners
        playButton.onClick.AddListener(PlayVideo);
        pauseButton.onClick.AddListener(PauseVideo);
    }

    public void PlayVideo()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }
    }

    public void PauseVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
    }
}