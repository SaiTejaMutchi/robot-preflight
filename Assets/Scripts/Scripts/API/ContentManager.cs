using LitJson;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System;




public class ContentManager : MonoBehaviour
{
    public static ContentManager instance;
    #region Get url and data
    //for getting content 
    private string experienceURL;
    public static JsonData contentData;
    public static float scaleValue = 10;
    #endregion

    #region Image data variebles
    //private List<string> image_info_message = new List<string>();
    private List<string> imgPath = new List<string>();
    private List<string> imgID = new List<string>();
    private List<Vector3> imgPos = new List<Vector3>();
    private List<Vector3> imgRot = new List<Vector3>();
    private List<Vector3> imgScale = new List<Vector3>();
    // Public varabiles
    public int imgCount = 0;
    public int totalImags;
    #endregion

    #region Video data variebles
    // private List<string> video_info_message = new List<string>();
    private List<string> video_url = new List<string>();
    private List<string> video_id = new List<string>();
    private List<Vector3> video_Position = new List<Vector3>();
    private List<Vector3> video_Rotation = new List<Vector3>();
    private List<Vector3> video_Scale = new List<Vector3>();
    private List<string> video_source = new List<string>();
    private List<float> video_playback_speed = new List<float>();
    private List<string> video_play_on_awake = new List<string>();
    private List<string> video_wait_for_first_frame = new List<string>();
    private List<string> video_loop = new List<string>();
    private List<string> video_mute = new List<string>();
    private List<string> video_rendermode = new List<string>();
    private List<string> video_apsectratio = new List<string>();
    private List<string> video_volume = new List<string>();
    private List<string> video_play = new List<string>();


    #endregion



    #region Audio data variebles
    //  private List<string> audio_info_message = new List<string>();
    private List<string> audio_url = new List<string>();
    private List<string> audio_id = new List<string>();
    private List<Vector3> audio_Position = new List<Vector3>();
    private List<Vector3> audio_Rotation = new List<Vector3>();
    private List<Vector3> audio_Scale = new List<Vector3>();
    private List<string> audio_Spatial_sound = new List<string>();
    private List<float> audio_volume = new List<float>();
    private List<bool> audio_mute = new List<bool>();
    private List<bool> audio_bypass_effects = new List<bool>();
    private List<bool> audio_bypass_listener_effects = new List<bool>();
    private List<bool> audio_bypass_reverb_zones = new List<bool>();
    private List<bool> audio_play_on_awake = new List<bool>();
    private List<bool> audio_loop = new List<bool>();
    private List<int> audio_priority = new List<int>();
    private List<float> audio_pitch = new List<float>();
    private List<float> audio_stereo_pan = new List<float>();
    private List<float> audio_spatial_blend = new List<float>();
    private List<float> audio_reverb_zone_mix = new List<float>();
    private List<string> audio_thumbnail = new List<string>();

    #endregion

    #region Model data variebles
    //private List<string> model_info_message = new List<string>();
    private List<string> model_url = new List<string>();
    private List<string> model_id = new List<string>();
    private List<Vector3> model_Position = new List<Vector3>();
    private List<Vector3> model_Rotation = new List<Vector3>();
    private List<Vector3> model_Scale = new List<Vector3>();
    #endregion


    public int totalNumber_of_assets;
    public int assets_Count;
    void Start()
    {
        Debug.Log("................. " + VirtualKeyboardHandler.submittedText);
        instance = this;
        string apiUrl = APIConfig.BaseUrl;
        experienceURL = apiUrl + "experience/detail/" + "69e8484261ee0389d46cd076";//"69b79ba2ba60065720aeccd1"
        //experienceURL = "https://devserver.spatialgrid.ai/api/v1/experience/redirect/" + VirtualKeyboardHandler.submittedText;//"69b2489dba60065720adf0c1"

        // int id = int.Parse(contentData["data"]["id"].ToString());


        // GameObject obj = GameObject.Find(id.ToString());


        // obj.SetActive(false);


        LoadContent();
    }

    IEnumerator ParseJson(string url)
    {
        Debug.Log(url);
        UnityWebRequest uwr = UnityWebRequest.Get(url);

        yield return uwr.SendWebRequest();

        // Check for errors
        if (uwr.result == UnityWebRequest.Result.ConnectionError ||
            uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + uwr.error); // Log the error
            yield break;
        }

        // Parse JSON
        try
        {
            contentData = JsonMapper.ToObject(uwr.downloadHandler.text);
            Debug.Log(uwr.downloadHandler.text + contentData["success"].ToString().ToLower());



            if (contentData["success"].ToString().ToLower() == "true")
            {
                GetAllAssetsData();
                Debug.Log("Data parsed successfully." + contentData);
            }
            else
            {
                Debug.Log("Invalid data"); // Show popup: Data not available or network issue
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("JSON Parsing Error: " + e.Message);
        }
    }

    public void LoadContent()
    {
        StartCoroutine(ParseJson(experienceURL));
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //StartCoroutine(ParseJson(experienceURL));
            //GetAllAssetsData();

            GetAnimationData();
        }

    }



    public void GetAllAssetsData()
    {
        GetImageObjectData();
        GetVideoObjectData();
        GetAudioObjectData();
        GetModelObjectData();
        //GetAnimationData();
    }

    void GetImageObjectData()
    {
        totalImags = contentData["data"]["image"].Count;
        totalNumber_of_assets = totalNumber_of_assets + totalImags;
        if (contentData["data"]["image"].Count > 0)
        {
            for (int i = 0; i < contentData["data"]["image"].Count; i++)
            {
                imgPath.Add(contentData["data"]["image"][i]["asset_file_path"].ToString());
                imgID.Add(contentData["data"]["image"][i]["image_code"].ToString());

                imgPos.Add(new Vector3(float.Parse(contentData["data"]["image"][i]["position_x"].ToString()), float.Parse(contentData["data"]["image"][i]["position_y"].ToString()), float.Parse(contentData["data"]["image"][i]["position_z"].ToString())));
                imgRot.Add(new Vector3(float.Parse(contentData["data"]["image"][i]["rotation_x"].ToString()), float.Parse(contentData["data"]["image"][i]["rotation_y"].ToString()), float.Parse(contentData["data"]["image"][i]["rotation_z"].ToString())));
                imgScale.Add(new Vector3(float.Parse(contentData["data"]["image"][i]["scale_x"].ToString()), float.Parse(contentData["data"]["image"][i]["scale_y"].ToString()), float.Parse(contentData["data"]["image"][i]["scale_z"].ToString())));

            }
            NextImageToDownlod();
        }
    }

    public void NextImageToDownlod()
    {
        LoadImageContent.instanceImage.GetImageDetails(imgPath[imgCount], imgID[imgCount], imgPos[imgCount], imgRot[imgCount], imgScale[imgCount]);
    }


    public int videoCount = 0;
    public int numberOfVideos;
    void GetVideoObjectData()
    {
        numberOfVideos = contentData["data"]["video"].Count;
        totalNumber_of_assets = totalNumber_of_assets + numberOfVideos;
        if (contentData["data"]["video"].Count > 0)
        {
            for (int i = 0; i < contentData["data"]["video"].Count; i++)
            {

                video_url.Add(contentData["data"]["video"][i]["asset_file_path"].ToString());
                video_id.Add(contentData["data"]["video"][i]["video_code"].ToString());
                video_Position.Add(new Vector3(float.Parse(contentData["data"]["video"][i]["position_x"].ToString()), float.Parse(contentData["data"]["video"][i]["position_y"].ToString()), float.Parse(contentData["data"]["video"][i]["position_z"].ToString())));
                video_Rotation.Add(new Vector3(float.Parse(contentData["data"]["video"][i]["rotation_x"].ToString()), float.Parse(contentData["data"]["video"][i]["rotation_y"].ToString()), float.Parse(contentData["data"]["video"][i]["rotation_z"].ToString())));
                video_Scale.Add(new Vector3(float.Parse(contentData["data"]["video"][i]["scale_x"].ToString()), float.Parse(contentData["data"]["video"][i]["scale_y"].ToString()), float.Parse(contentData["data"]["video"][i]["scale_z"].ToString())));
                video_play_on_awake.Add(contentData["data"]["video"][i]["video_settings"]["play_on_awake"].ToString());
                video_play.Add(contentData["data"]["video"][i]["video_settings"]["playback_state"].ToString());

                video_loop.Add(contentData["data"]["video"][i]["video_settings"]["loop"].ToString());

                string speedStr = contentData["data"]["video"][i]["video_settings"]["playback_speed"].ToString();
                float speed;
                string vidstr = contentData["data"]["video"][i]["video_settings"]["loop"].ToString();
                string videosource = contentData["data"]["video"][i]["video_settings"]["source"].ToString();
                string videoMute = contentData["data"]["video"][i]["video_settings"]["mute"].ToString();
                string videoRendermode = contentData["data"]["video"][i]["video_settings"]["render_mode"].ToString();
                string videoAspectRatio = contentData["data"]["video"][i]["video_settings"]["aspect_ratio"].ToString();
                string videoWaitForFirstFrame = contentData["data"]["video"][i]["video_settings"]["wait_for_first_frame"].ToString();
                string videoVolume = contentData["data"]["video"][i]["video_settings"]["volume"].ToString();
                string videoPlay = contentData["data"]["video"][i]["video_settings"]["playback_state"].ToString();
                string videoLoop = contentData["data"]["video"][i]["video_settings"]["loop"].ToString();



                if (float.TryParse(speedStr, out speed))
                {
                    video_playback_speed.Add(speed);
                }

                video_play_on_awake.Add(contentData["data"]["video"][i]["video_settings"]["play_on_awake"].ToString());
                video_loop.Add(contentData["data"]["video"][i]["video_settings"]["loop"].ToString());
                video_source.Add(contentData["data"]["video"][i]["video_settings"]["source"].ToString());
                video_mute.Add(contentData["data"]["video"][i]["video_settings"]["mute"].ToString());
                video_rendermode.Add(contentData["data"]["video"][i]["video_settings"]["render_mode"].ToString());
                video_apsectratio.Add(contentData["data"]["video"][i]["video_settings"]["aspect_ratio"].ToString());
                video_wait_for_first_frame.Add(contentData["data"]["video"][i]["video_settings"]["wait_for_first_frame"].ToString());
                video_volume.Add(contentData["data"]["video"][i]["video_settings"]["volume"].ToString());
                video_play.Add(contentData["data"]["video"][i]["video_settings"]["playback_state"].ToString());


            }



            DownlodVideo();
        }
    }

    public void DownlodVideo()
    {
        LoadVideoContent.instanceVideo.GetVideoDetails(video_url[videoCount], video_id[videoCount], video_Position[videoCount], video_Rotation[videoCount], video_Scale[videoCount], video_playback_speed[videoCount], video_play_on_awake[videoCount], video_loop[videoCount], video_source[videoCount], video_mute[videoCount], video_rendermode[videoCount], video_apsectratio[videoCount], video_wait_for_first_frame[videoCount], video_volume[videoCount], video_play[videoCount]);
    }


    public int audioCount = 0;
    public int numberOfAudios;
    void GetAudioObjectData()
    {

        numberOfAudios = contentData["data"]["audio"].Count;
        totalNumber_of_assets = totalNumber_of_assets + numberOfAudios;
        if (contentData["data"]["audio"].Count > 0)
        {
            for (int i = 0; i < contentData["data"]["audio"].Count; i++)
            {
                //audio_info_message.Add(contentData["assets"]["audios"][i]["message"].ToString());
                audio_url.Add(contentData["data"]["audio"][i]["asset_file_path"].ToString());
                audio_id.Add(contentData["data"]["audio"][i]["audio_code"].ToString());

                audio_Position.Add(new Vector3(float.Parse(contentData["data"]["audio"][i]["position_x"].ToString()), float.Parse(contentData["data"]["audio"][i]["position_y"].ToString()), float.Parse(contentData["data"]["audio"][i]["position_z"].ToString())));
                audio_Rotation.Add(new Vector3(float.Parse(contentData["data"]["audio"][i]["rotation_x"].ToString()), float.Parse(contentData["data"]["audio"][i]["rotation_y"].ToString()), float.Parse(contentData["data"]["audio"][i]["rotation_z"].ToString())));
                audio_Scale.Add(new Vector3(float.Parse(contentData["data"]["audio"][i]["scale_x"].ToString()), float.Parse(contentData["data"]["audio"][i]["scale_y"].ToString()), float.Parse(contentData["data"]["audio"][i]["scale_z"].ToString())));

                string audiohdr = contentData["data"]["audio"][i]["audio_settings"]["spatial_sound"].ToString();
                audio_Spatial_sound.Add(contentData["data"]["audio"][i]["audio_settings"]["spatial_sound"].ToString());
                audio_mute.Add(bool.Parse(contentData["data"]["audio"][i]["audio_settings"]["mute"].ToString()));

                audio_bypass_effects.Add(bool.Parse(contentData["data"]["audio"][i]["audio_settings"]["bypass_effects"].ToString()));
                audio_bypass_listener_effects.Add(bool.Parse(contentData["data"]["audio"][i]["audio_settings"]["bypass_listener_effects"].ToString()));
                audio_bypass_reverb_zones.Add(bool.Parse(contentData["data"]["audio"][i]["audio_settings"]["bypass_reverb_zones"].ToString()));
                audio_play_on_awake.Add(bool.Parse(contentData["data"]["audio"][i]["audio_settings"]["play_on_awake"].ToString()));
                audio_loop.Add(bool.Parse(contentData["data"]["audio"][i]["audio_settings"]["loop"].ToString()));
                audio_thumbnail.Add(contentData["data"]["audio"][i]["asset_thumbnail"].ToString());

                string volumeStr = contentData["data"]["audio"][i]["audio_settings"]["volume"].ToString();
                float _volume;
                if (float.TryParse(volumeStr, out _volume))
                {
                    audio_volume.Add(_volume);
                }

                /*string priorityStr = contentData["data"]["audio"][i]["audio_settings"]["priority"].ToString();
                float _priority;
                if (float.TryParse(priorityStr, out _priority))
                {
                  audio_priority.Add( _priority);

                }*/

                //audio_priority.Add((int)(float)(int)contentData["data"]["audio"][i]["audio_settings"]["priority"]);

                int priorityInt = (int)contentData["data"]["audio"][i]["audio_settings"]["volume"];
                float _priority = (float)priorityInt;
                audio_priority.Add((int)_priority);

                string pitchStr = contentData["data"]["audio"][i]["audio_settings"]["pitch"].ToString();
                float pitch;
                if (float.TryParse(pitchStr, out pitch))
                {
                    audio_pitch.Add(pitch);
                }

                string stereo_panStr = contentData["data"]["audio"][i]["audio_settings"]["pitch"].ToString();
                float stereo_pan;
                if (float.TryParse(stereo_panStr, out stereo_pan))
                {
                    audio_stereo_pan.Add(stereo_pan);
                }

                string spatial_blendStr = contentData["data"]["audio"][i]["audio_settings"]["pitch"].ToString();
                float spatial_blend;
                if (float.TryParse(spatial_blendStr, out spatial_blend))
                {
                    audio_spatial_blend.Add(spatial_blend);
                }

                string reverb_zone_mixStr = contentData["data"]["audio"][i]["audio_settings"]["pitch"].ToString();
                float reverb_zone_mix;
                if (float.TryParse(reverb_zone_mixStr, out reverb_zone_mix))
                {
                    audio_reverb_zone_mix.Add(reverb_zone_mix);
                }
            }
            DownlodAudio();
        }
    }

    public void DownlodAudio()
    {

        LoadAudioContent.instanceAudio.GetAudioDetails(audio_url[audioCount], audio_id[audioCount], audio_Position[audioCount], audio_Rotation[audioCount], audio_Scale[audioCount], audio_Spatial_sound[audioCount], audio_volume[audioCount], audio_mute[audioCount], audio_bypass_effects[audioCount], audio_bypass_listener_effects[audioCount], audio_bypass_reverb_zones[audioCount], audio_play_on_awake[audioCount], audio_loop[audioCount], audio_priority[audioCount], audio_pitch[audioCount], audio_stereo_pan[audioCount], audio_spatial_blend[audioCount], audio_reverb_zone_mix[audioCount], audio_thumbnail[audioCount]);
    }


    public int modelCount = 0;
    public int numberOfModels;
    void GetModelObjectData()
    {
        numberOfModels = contentData["data"]["mesh"].Count;
        totalNumber_of_assets = totalNumber_of_assets + numberOfModels;
        if (contentData["data"]["mesh"].Count > 0)
        {
            for (int i = 0; i < contentData["data"]["mesh"].Count; i++)
            {
                //model_info_message.Add(contentData["assets"]["mesh"][i]["message"].ToString());
                model_url.Add(contentData["data"]["mesh"][i]["asset_file_path"].ToString());
                model_id.Add(contentData["data"]["mesh"][i]["mesh_code"].ToString());

                //Debug.Log("Aarthiiii.............." + contentData["data"]["mesh"][i]["asset_file_path"].ToString());

                model_Position.Add(new Vector3(float.Parse(contentData["data"]["mesh"][i]["position_x"].ToString()), float.Parse(contentData["data"]["mesh"][i]["position_y"].ToString()), float.Parse(contentData["data"]["mesh"][i]["position_z"].ToString())));
                model_Rotation.Add(new Vector3(float.Parse(contentData["data"]["mesh"][i]["rotation_x"].ToString()), float.Parse(contentData["data"]["mesh"][i]["rotation_y"].ToString()), float.Parse(contentData["data"]["mesh"][i]["rotation_z"].ToString())));
                model_Scale.Add(new Vector3(float.Parse(contentData["data"]["mesh"][i]["scale_x"].ToString()), float.Parse(contentData["data"]["mesh"][i]["scale_y"].ToString()), float.Parse(contentData["data"]["mesh"][i]["scale_z"].ToString())));
                //Debug.Log("Aarthiiii.............. 1");
            }
            DownlodModel();
        }
    }

    public void DownlodModel()
    {

        // Debug.Log("Aarthiiii.............." + model_url[modelCount]);
        // Debug.Log("Aarthiiii.............." + model_id[modelCount]);
        // Debug.Log("Aarthiiii.............." + model_Position[modelCount]);
        // Debug.Log("Aarthiiii.............." + model_Rotation[modelCount]);
        // Debug.Log("Aarthiiii.............." + model_Scale[modelCount]);
        LoadModelContent.instanceModel.GetModel_Info(model_url[modelCount], model_id[modelCount], model_Position[modelCount], model_Rotation[modelCount], model_Scale[modelCount]);
    }

    public int animationCount = 0;
    public int numberOfAnimations;

    public int id;

    private GameObject _fadeTarget;




    public void GetAnimationData()
    {
        numberOfAnimations = contentData["data"]["animation_effects"].Count;
        Debug.Log(numberOfAnimations);
        totalNumber_of_assets = totalNumber_of_assets + numberOfAnimations;


        if (numberOfAnimations > 0)
        {
            for (int i = 0; i < numberOfAnimations; i++)
            {
                string objectId = contentData["data"]["animation_effects"][i]["objectId"].ToString();

                int timelineEffects = contentData["data"]["animation_effects"][i]["timelineEffects"].Count;
                Debug.Log("timelineEffects" + timelineEffects);

                for (int j = 0; j < contentData["data"]["animation_effects"][i]["timelineEffects"].Count; j++)
                {
                    GameObject obj = GameObject.Find(objectId);

                    Debug.Log(contentData["data"]["animation_effects"][i]["timelineEffects"][j]["effectType"].ToString());

                    if (contentData["data"]["animation_effects"][i]["timelineEffects"][j]["effectType"].ToString() == "move")
                    {
                        string startPosX = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["startPosition"]["x"].ToString();
                        string startPosY = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["startPosition"]["y"].ToString();
                        string startPosZ = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["startPosition"]["z"].ToString();

                        string endPosX = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["endPosition"]["x"].ToString();
                        string endPosY = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["endPosition"]["y"].ToString();
                        string endPosZ = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["endPosition"]["z"].ToString();

                        Vector3 startPos = new Vector3(float.Parse(startPosX), float.Parse(startPosY), float.Parse(startPosZ));
                        Vector3 endPos = new Vector3(float.Parse(endPosX), float.Parse(endPosY), float.Parse(endPosZ));

                        Debug.Log(startPos);
                        Debug.Log(endPos);

                        string startTime = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["startTime"].ToString();
                        string endTime = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["endTime"].ToString();

                        float startTimeVal = float.Parse(startTime);
                        float endTimeVal = float.Parse(endTime);
                        float duration = endTimeVal - startTimeVal;

                        obj.transform.position = startPos;

                        iTween.MoveTo(obj, iTween.Hash("position", endPos, "time", duration, "delay", startTimeVal));
                    }

                    else if (contentData["data"]["animation_effects"][i]["timelineEffects"][j]["effectType"].ToString() == "rotate")
                    {


                        string endRotationZ = contentData["data"]["animation_effects"][i]["timelineEffects"][0]["endRotation"]["z"].ToString();
                        string endRotationX = contentData["data"]["animation_effects"][i]["timelineEffects"][0]["endRotation"]["x"].ToString();
                        string endRotationY = contentData["data"]["animation_effects"][i]["timelineEffects"][0]["endRotation"]["y"].ToString();

                        string startTime = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["startTime"].ToString();
                        string endTime = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["endTime"].ToString();

                        float startTimeVal = float.Parse(startTime);
                        float endTimeVal = float.Parse(endTime);
                        float duration = endTimeVal - startTimeVal;

                        Debug.Log(startTimeVal);
                        Debug.Log(endTimeVal);
                        Debug.Log(duration);


                        Vector3 endRotation = new Vector3(float.Parse(endRotationX), float.Parse(endRotationY), float.Parse(endRotationZ));

                        iTween.RotateBy(obj, iTween.Hash("amount", new Vector3(endRotation.x / 360f, endRotation.y / 360f, endRotation.z / 360f), "time", duration, "delay", startTimeVal, "islocal", true));
                    }
                    else if (contentData["data"]["animation_effects"][i]["timelineEffects"][j]["effectType"].ToString() == "scale")
                    {
                        string startScaleX = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["startScale"]["x"].ToString();
                        string startScaleY = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["startScale"]["y"].ToString();
                        string startScaleZ = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["startScale"]["z"].ToString();

                        string endScaleX = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["endScale"]["x"].ToString();
                        string endScaleY = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["endScale"]["y"].ToString();
                        string endScaleZ = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["endScale"]["z"].ToString();


                        Vector3 startScale = new Vector3(float.Parse(startScaleX), float.Parse(startScaleY), float.Parse(startScaleZ));
                        Vector3 endScale = new Vector3(float.Parse(endScaleX), float.Parse(endScaleY), float.Parse(endScaleZ));

                        Debug.Log(startScale);
                        Debug.Log(endScale);

                        string startTime = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["startTime"].ToString();
                        string endTime = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["endTime"].ToString();

                        float startTimeVal = float.Parse(startTime);
                        float endTimeVal = float.Parse(endTime);
                        float duration = endTimeVal - startTimeVal;

                        obj.transform.localScale = startScale;

                        iTween.ScaleTo(obj, iTween.Hash("scale", endScale, "time", duration, "delay", startTimeVal));
                    }
                    else if (contentData["data"]["animation_effects"][i]["timelineEffects"][j]["effectType"].ToString() == "fadein")
                    {
                        string startTime = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["startTime"].ToString();
                        string endTime = contentData["data"]["animation_effects"][i]["timelineEffects"][j]["endTime"].ToString();

                        float startTimeVal = float.Parse(startTime);
                        float endTimeVal = float.Parse(endTime);
                        float duration = endTimeVal - startTimeVal;

                        Renderer childRend = obj.GetComponentInChildren<Renderer>();
                        if (childRend == null)
                        {
                            Debug.LogWarning("FadeIn: No Renderer found in children of " + obj.name);
                            continue;
                        }
                        _fadeTarget = childRend.gameObject;

                        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", duration, "delay", startTimeVal, "onupdate", "OnFadeUpdate", "onupdatetarget", gameObject));
                        //not done yet ...not working properly effect is not visible

                    }


                }

            }

        }

        void OnFadeUpdate(float alpha)
        {

            Renderer rend = _fadeTarget.GetComponent<Renderer>();
            Material mat = rend.material;

            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            mat.SetColor("_BaseColor", new Color(mat.color.r, mat.color.g, mat.color.b, alpha));
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, alpha);

        }

    }

}
