using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class LoadVideoContent : MonoBehaviour
{
	Texture2D texture2D;
	public static LoadVideoContent instanceVideo;
    private Scene scene;
    private void Start()
    {
        scene = SceneManager.GetActiveScene();
        instanceVideo = this;
	}
    public void GetVideoDetails(string url, string videoID, Vector3 _videoPosition, Vector3 _videoRotation, Vector3 _videoScaling, float _videoPlayback, string _videoPlayonawake, string _videoLoop, string _videoSource, string _videoMute, string _videoRendermode, string _videoAspectratio, string _videoWaitforfirstframe, string videoVolume, string videoPlay)
    {
		StartCoroutine(LoadingVideo( url,  videoID,    _videoPosition,  _videoRotation,  _videoScaling,_videoPlayback, _videoPlayonawake, _videoLoop, _videoSource, _videoMute, _videoRendermode, _videoAspectratio, _videoWaitforfirstframe, videoVolume, videoPlay));
	}
	
	IEnumerator LoadingVideo(string url, string videoID, Vector3 _videoPosition, Vector3 _videoRotation, Vector3 _videoScaling, float videoPlayback, string videoPlayonawake, string videoLoop, string videoSource, string _videoMute, string _videoRendermode, string _videoAspectratio, string _videoWaitforfirstframe, string videoVolume, string videoPlay)
	{
        GameObject video = (GameObject)Instantiate(Resources.Load("Video", typeof(GameObject)));
        video.name = videoID;

        yield return new WaitForSeconds(.1f);

		if (video!= null)
		{
            
            video.transform.gameObject.AddComponent<VideoPlayer>();
			video.transform.GetComponent<VideoPlayer>().playOnAwake = true;
			video.transform.GetComponent<VideoPlayer>().url = url;
            video.transform.GetComponent<VideoPlayer>().time = 0;
            video.transform.GetComponent<VideoPlayer>().playbackSpeed = videoPlayback;
            video.transform.GetComponent<VideoPlayer>().SetDirectAudioVolume(0, float.Parse(videoVolume));
            video.transform.GetComponent<VideoPlayer>().SetDirectAudioMute(0, bool.Parse(_videoMute));
            video.transform.GetComponent<VideoPlayer>().isLooping = bool.Parse(videoLoop);

            if (videoPlay == "Play")
            {
                video.transform.GetComponent<VideoPlayer>().Play();
            }
            else if(videoPlay == "Pause")
            {
                video.transform.GetComponent<VideoPlayer>().Pause();
            }else if (videoPlay == "Stop")
            {
                video.transform.GetComponent<VideoPlayer>().Stop();
            }

            video.transform.GetComponent<VideoPlayer>().waitForFirstFrame = bool.Parse(_videoWaitforfirstframe);
            //video.transform.GetComponent<VideoPlayer>().aspectRatio = (VideoAspectRatio)System.Enum.Parse(typeof(VideoAspectRatio), _videoAspectratio);

            if (video.transform.GetComponent<VideoPlayer>().isPlaying)
            {
                Debug.Log("Enable Pause Button");
            }

            if (video.transform.GetComponent<VideoPlayer>().isPaused)
            {
                Debug.Log("Enable Play Button");
            }




            Debug.Log("props " + videoID);
            Debug.Log("props " + url);
            Debug.Log("props " + videoPlayback);
            Debug.Log("props " + _videoPosition);
            Debug.Log("props " + _videoRotation);
            Debug.Log("props " + _videoScaling);
            Debug.Log("props " + videoID);
            Debug.Log("props " + videoLoop);
            Debug.Log("props " + videoPlayonawake);
            Debug.Log("props " + videoSource);
            Debug.Log("props " + _videoMute);
            Debug.Log("props " + _videoRendermode);
            Debug.Log("props " + _videoAspectratio);
            Debug.Log("props " + _videoWaitforfirstframe);
            Debug.Log("props " + videoVolume);
            Debug.Log("props " + videoPlay);
           


            GameObject rootObject = new GameObject();
            GameObject downloadeImage = new GameObject();
            rootObject.name = "VideoRootObject";
            downloadeImage.name = "DownloadeImage";
            rootObject.transform.SetParent(GameObject.Find("Tracking").transform);
            rootObject.transform.localPosition = Vector3.zero;
            rootObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
            rootObject.transform.localScale = new Vector3(1f, 1f, 1f);
            downloadeImage.transform.SetParent(rootObject.transform);

            downloadeImage.transform.localPosition = Vector3.zero;
            downloadeImage.transform.localRotation = Quaternion.Euler(Vector3.zero);
            downloadeImage.transform.localScale = Vector3.one;

            _videoRotation.y = -_videoRotation.y;
            _videoRotation.z = -_videoRotation.z;

            Quaternion qX = Quaternion.Euler(_videoRotation.x, 0, 0);
            Quaternion qY = Quaternion.Euler(0, _videoRotation.y, 0);
            Quaternion qZ = Quaternion.Euler(0, 0, _videoRotation.z);
            Quaternion finalRotation = qZ * qX * qY;

            if (scene.name == "05Vuforia")
            {
                downloadeImage.transform.localPosition = new Vector3(-_videoPosition.x, _videoPosition.y, _videoPosition.z);
            }
            else
            {
                downloadeImage.transform.localPosition = new Vector3(-_videoPosition.x, _videoPosition.y, _videoPosition.z);
            }

            downloadeImage.transform.localRotation = finalRotation;
            downloadeImage.transform.localScale = new Vector3(_videoScaling.x, _videoScaling.y, _videoScaling.z);

            video.transform.SetParent(downloadeImage.transform);
            video.transform.localPosition = Vector3.zero;
            video.transform.localRotation = Quaternion.Euler(90, 0, 0);
            video.transform.localScale = Vector3.one;
        }

        FindFirstObjectByType<ContentManager>().videoCount++;
        FindFirstObjectByType<ContentManager>().assets_Count++;

		if (FindFirstObjectByType<ContentManager>().assets_Count == FindFirstObjectByType<ContentManager>().totalNumber_of_assets)
		{
			Debug.Log("Downloaded All Assets : ");
		}

		if (FindFirstObjectByType<ContentManager>().videoCount < FindFirstObjectByType<ContentManager>().numberOfVideos)
		{
            FindFirstObjectByType<ContentManager>().DownlodVideo();
		}
	}
}
