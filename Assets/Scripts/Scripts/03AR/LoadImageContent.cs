
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


public class LoadImageContent : MonoBehaviour
{

    public static LoadImageContent instanceImage;
    GameObject image;
    Texture imageTexture;
    private Scene scene;

    private void Start()
    {
        scene = SceneManager.GetActiveScene();
        instanceImage = this;
    }

    public void GetImageDetails(string imgPath, string imgID, Vector3 imgPos, Vector3 imgRot, Vector3 imgScale)
    {
        StartCoroutine(LoadImages(imgPath, imgID, imgPos, imgRot, imgScale));
    }
    public IEnumerator LoadImages(string imgPath, string imgID, Vector3 imgPos, Vector3 imgRot, Vector3 imgScale)
    {
        image = (GameObject)Instantiate(Resources.Load("Image", typeof(GameObject)));
        if (imgPath != "")
        {
            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imgPath);
            uwr.SendWebRequest();

            while (!uwr.isDone)
            {
                yield return 1;
            }
            if (uwr.isDone)
            {
                imageTexture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
            }
            if (uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(uwr.error);
            }
            uwr.Dispose();
            image.transform.GetComponent<Renderer>().material.mainTexture = imageTexture;
        }

        image.name = imgID;

        GameObject rootObject = new GameObject();
        GameObject downloadeImage = new GameObject();
        rootObject.name = "ImageRootObject";
        downloadeImage.name = "DownloadeImage";

        rootObject.transform.SetParent(GameObject.Find("Tracking").transform);
        rootObject.transform.localPosition = Vector3.zero;
        rootObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        rootObject.transform.localScale = new Vector3(1f, 1f, 1f);

        downloadeImage.transform.SetParent(rootObject.transform);
        downloadeImage.transform.localPosition = Vector3.zero;
        downloadeImage.transform.localRotation = Quaternion.Euler(Vector3.zero);
        downloadeImage.transform.localScale = Vector3.one;

        imgRot.y = -imgRot.y;
        imgRot.z = -imgRot.z;

        Quaternion qX = Quaternion.Euler(imgRot.x, 0, 0);
        Quaternion qY = Quaternion.Euler(0, imgRot.y, 0);
        Quaternion qZ = Quaternion.Euler(0, 0, imgRot.z);
        Quaternion finalRotation = qZ * qX * qY;

        if (scene.name == "05Vuforia")
        {
            downloadeImage.transform.localPosition = new Vector3(-imgPos.x, imgPos.y, imgPos.z);
        }
        else
        {
            downloadeImage.transform.localPosition = new Vector3(-imgPos.x, imgPos.y, imgPos.z);
        }

        downloadeImage.transform.localRotation = finalRotation;
        downloadeImage.transform.localScale = new Vector3(imgScale.x, imgScale.y, imgScale.z);

        image.transform.SetParent(downloadeImage.transform);
        image.transform.localPosition = Vector3.zero;
        image.transform.localRotation = Quaternion.Euler(90, 0, 0);
        image.transform.localScale = Vector3.one;



        FindFirstObjectByType<ContentManager>().imgCount++;
        FindFirstObjectByType<ContentManager>().assets_Count++;
        if (FindFirstObjectByType<ContentManager>().assets_Count == FindFirstObjectByType<ContentManager>().totalNumber_of_assets)
        {
            Debug.Log("Downloaded All Assets : ");
        }
        if (FindFirstObjectByType<ContentManager>().imgCount < FindFirstObjectByType<ContentManager>().totalImags)
        {
            FindFirstObjectByType<ContentManager>().NextImageToDownlod();
        }
    }
}
