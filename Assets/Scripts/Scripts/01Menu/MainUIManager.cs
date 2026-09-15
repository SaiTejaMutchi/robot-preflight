using UnityEngine;
using UnityEngine.SceneManagement;

public class MainUIManager : MonoBehaviour
{
    public GameObject fadeObject;

    private void Awake()
    {
        if (fadeObject != null)
        {
            fadeObject.SetActive(true);
        }
    }
    void Start()
    {
        
    }
    public void SubmitButton()
    {
        SceneManager.LoadScene(2);
    }
}
