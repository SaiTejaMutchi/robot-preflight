using UnityEngine;
using UnityEngine.UI;
using System.Text;
public class UserManager : MonoBehaviour
{
    [SerializeField] private Text userNameText, emailText,password;
    [SerializeField] private GameObject userMenu, guestMenu;

    private string password_originalText;
    [SerializeField] private Image passwordIcon;
    [SerializeField] private Sprite showpassword,hidepassword;
    void Start()
    {
        userNameText.text = "" + LoginManager.userName ;
        emailText.text = ""+ LoginManager.userEmail;
        password.text="" + LoginManager.password;

        password_originalText = LoginManager.password;

       // password_originalText = "forTest";

        if (!LoginManager.guestLogin)
        {
            userMenu.SetActive(true);
            guestMenu.SetActive(false);
        }
        else
        {
            userMenu.SetActive(false);
            guestMenu.SetActive(true);
        }
        SetAsteriskText();
    }

    bool isShowpassword = false;
    public void Show_Hide_Password()
    {
        isShowpassword = !isShowpassword;

        if (isShowpassword)
        {
            RestoreOriginalText();
            if(passwordIcon!=null)
            passwordIcon.sprite = showpassword;
        }
        else
        {
            SetAsteriskText();
            if (passwordIcon != null)
                passwordIcon.sprite = hidepassword;
        }
    }

    public void SetAsteriskText()
    {
        StringBuilder asteriskText = new StringBuilder();
        for (int i = 0; i < password_originalText.Length; i++)
        {
            asteriskText.Append("*");
        }
        password.text = asteriskText.ToString();
    }

    public void RestoreOriginalText()
    {
        password.text = password_originalText;
    }

    public void OpenWebsite()
    {
        Application.OpenURL("https://www.google.com/");
    }
}
