using System.Collections;
using System.Collections.Generic;
using GLTFast.Schema;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadAudioContent : MonoBehaviour
{
    AudioClip clip;
    public static LoadAudioContent instanceAudio;
    GameObject audio;

    private UnityEngine.SceneManagement.Scene scene;
    //public RawImage targetImage;





    private void Start()
    {
        scene = SceneManager.GetActiveScene();
        instanceAudio = this;
        Debug.Log("Creating plane from: " + this.gameObject.name);
        //StartCoroutine(DownloadImage(imageUrl));
    }



    public void GetAudioDetails(string url, string audioID, Vector3 audioPos, Vector3 audioRot, Vector3 audioScal, string Spatial_sound, float audio_volume, bool mute, bool bypass_effects, bool bypass_listener_effects, bool bypass_reverb_zones, bool play_on_awake, bool audio_loop, int audio_priority, float audio_pitch, float stereo_pan, float spatial_blend, float reverb_zone_mix, string audiothumbnail)
    {
        StartCoroutine(LoadAudio(url, audioID, audioPos, audioRot, audioScal, Spatial_sound, audio_volume, mute, bypass_effects, bypass_listener_effects, bypass_reverb_zones, play_on_awake, audio_loop, audio_priority, audio_pitch, stereo_pan, spatial_blend, reverb_zone_mix, audiothumbnail));
        //StartCoroutine(DownloadImage(imageUrl));

    }



    IEnumerator LoadAudio(string url, string audioID, Vector3 audioPos, Vector3 audioRot, Vector3 audioScal, string Spatial_sound, float audio_volume, bool mute, bool bypass_effects, bool bypass_listener_effects, bool bypass_reverb_zones, bool play_on_awake, bool loop, int audio_priority, float audio_pitch, float stereo_pan, float spatial_blend, float reverb_zone_mix, string audiothumbnail)
    {



        UnityWebRequest webRequest;


        if (url != "")
        {
            webRequest = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
            yield return webRequest.SendWebRequest();

            if (webRequest.isDone)
            {
                clip = DownloadHandlerAudioClip.GetContent(webRequest);
            }
            webRequest.Dispose();
        }

        audio = (GameObject)Instantiate(Resources.Load("Audio", typeof(GameObject)));
        audio.GetComponent<Renderer>().enabled = false;
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(audiothumbnail);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("imageUrl " + request.result);
            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            if (texture != null)
            {


                if (audio != null)
                {
                    audio.GetComponent<Renderer>().enabled = true;
                    audio.GetComponent<Renderer>().material.mainTexture = texture;
                    Debug.Log("Image successfully applied as texture.");
                }
                else
                {
                    Debug.LogError("Target GameObject does not have a Renderer component.");
                }
            }
            else
            {
                Debug.LogError("Downloaded texture is null.");
            }
        }


        audio.AddComponent<AudioSource>();
        audio.name = "audio";


        //audio.GetComponent<AudioSource>().Play();

        GameObject rootObject = new GameObject();
        GameObject downloadAudio = new GameObject();
        rootObject.name = "AudioRootObject";
        downloadAudio.name = "DownloadAudio";

        rootObject.transform.SetParent(GameObject.Find("Tracking").transform);
        rootObject.transform.localPosition = Vector3.zero;
        rootObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        rootObject.transform.localScale = new Vector3(1f, 1f, 1f);

        downloadAudio.transform.SetParent(rootObject.transform);
        downloadAudio.transform.localPosition = Vector3.zero;
        downloadAudio.transform.localRotation = Quaternion.Euler(Vector3.zero);
        downloadAudio.transform.localScale = Vector3.one;

        audioRot.y = -audioRot.y;
        audioRot.z = -audioRot.z;

        Quaternion qX = Quaternion.Euler(audioRot.x, 0, 0);
        Quaternion qY = Quaternion.Euler(0, audioRot.y, 0);
        Quaternion qZ = Quaternion.Euler(0, 0, audioRot.z);
        Quaternion finalRotation = qZ * qX * qY;

        if (scene.name == "05Vuforia")
        {
            downloadAudio.transform.localPosition = new Vector3(-audioPos.x, audioPos.y, audioPos.z);
        }
        else
        {
            downloadAudio.transform.localPosition = new Vector3(-audioPos.x, audioPos.y, audioPos.z);
        }

        downloadAudio.transform.localRotation = finalRotation;
        downloadAudio.transform.localScale = new Vector3(audioScal.x, audioScal.y, audioScal.z);

        audio.transform.SetParent(downloadAudio.transform);
        audio.transform.localPosition = Vector3.zero;
        audio.transform.localRotation = Quaternion.Euler(90, 0, 0);
        audio.transform.localScale = Vector3.one;


        audio.transform.SetParent(downloadAudio.transform);
        audio.GetComponent<AudioSource>().clip = clip;
        audio.GetComponent<AudioSource>().Play();


        //yield return StartCoroutine(DownloadImage(imageUrl));
        audio.GetComponent<AudioSource>().volume = audio_volume / 100;
        audio.GetComponent<AudioSource>().pitch = audio_pitch / 100;
        audio.GetComponent<AudioSource>().mute = mute;
        audio.GetComponent<AudioSource>().loop = loop;
        audio.GetComponent<AudioSource>().priority = audio_priority;
        Debug.Log("data added priority" + audio_priority);
        audio.GetComponent<AudioSource>().playOnAwake = play_on_awake;
        audio.GetComponent<AudioSource>().bypassEffects = bypass_effects;
        Debug.Log("Data added" + spatial_blend);
        audio.GetComponent<AudioSource>().spatialBlend = spatial_blend / 100;
        audio.GetComponent<AudioSource>().panStereo = stereo_pan / 100;
        Debug.Log("data added" + stereo_pan);
        audio.GetComponent<AudioSource>().reverbZoneMix = reverb_zone_mix / 100;
        audio.GetComponent<AudioSource>().bypassListenerEffects = bypass_listener_effects;
        audio.GetComponent<AudioSource>().bypassReverbZones = bypass_reverb_zones;






        //obj.GetComponent<Renderer>().material.mainTexture = texture2D;

        FindFirstObjectByType<ContentManager>().audioCount++;
        if (FindFirstObjectByType<ContentManager>().audioCount < FindFirstObjectByType<ContentManager>().numberOfAudios)
        {
            FindFirstObjectByType<ContentManager>().DownlodAudio();
        }
    }

}





