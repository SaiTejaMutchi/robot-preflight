using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class DashBoardManager : MonoBehaviour
{
    public static DashBoardManager dashBoardManager;
    public string downloadHandlerContent;
    private string getURl;
    private JsonData data;

    [SerializeField]
    TextMeshProUGUI userName;

    [SerializeField]
    GameObject logoutParent;


    void Start()
    {
        logoutParent.SetActive(false);
        userName.text = "Hi, "+ PlayerPrefs.GetString("userName");

        dashBoardManager = this;
        getURl = "https://fillar.in/backend-fillar/public/api/getExperiencesList";
        StartCoroutine("GetExperience");
      
    }

	IEnumerator GetExperience()
	{

        UnityWebRequest uwr = UnityWebRequest.Get(getURl);
      
        uwr.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("mytoken"));
        yield return uwr.SendWebRequest();

        if (uwr.isDone)
        {
            yield return new WaitForSeconds(1);
            data = JsonMapper.ToObject(uwr.downloadHandler.text);
            if (data["status"].ToString() == "true" || data["status"].ToString() == "True")
            {      
                for (int i = 0; i < data["data"].Count; i++)
                {
                    AddExperiences(data["data"][i]["project_name"].ToString(),data["data"][i]["experience_code"].ToString());
                }
            }
            else
            {
                Debug.Log("Invalid data");
            }
        }
    }

    public GameObject prefabToInstantiate;
    public GameObject parentObject;
    void AddExperiences(string projectName,string projectCode)
    {
        GameObject instantiatedObject = Instantiate(prefabToInstantiate, Vector3.zero, Quaternion.identity);
        instantiatedObject.transform.SetParent(parentObject.transform);
        instantiatedObject.transform.localScale = Vector3.one; 
        instantiatedObject.name = projectCode;
        //instantiatedObject.transform.GetChild(0).name = projectName;
        instantiatedObject.transform.GetChild(0).transform.GetComponent<Text>().text = projectName;
       // instantiatedObject.GetComponent<Button>().onClick.AddListener(ButtonClicked);

        instantiatedObject.GetComponent<Button>().onClick.AddListener(() => ButtonClicked(projectCode));
    }

    void ButtonClicked(string id)
    {
        StartCoroutine(GetCode(id));
    }


    IEnumerator GetCode(string id)
    {
        UnityWebRequest uwr = UnityWebRequest.Get("https://fillar.in/backend-fillar/public/api/getExperienceData/"+ id);
        uwr.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("mytoken"));
        yield return uwr.SendWebRequest();

        if (uwr.isDone)
        {
          
            JsonData data = JsonMapper.ToObject(uwr.downloadHandler.text);
            if (data["status"].ToString() == "true" || data["status"].ToString() == "True")
            {
                downloadHandlerContent = uwr.downloadHandler.text;
                SceneManager.LoadScene(3);
            }
            else
            {
                Debug.Log("Invalid data");
            }
        }
    }

    public void SettingsUser()
    {
        logoutParent.SetActive(true);
    }

    public void Logout()
    {
        PlayerPrefs.DeleteAll();

        SceneManager.LoadScene(1);
    }

    public void CancleLogout()
    {
        logoutParent.SetActive(false);
    }
    ///getExperienceData/{experience_code}
}
