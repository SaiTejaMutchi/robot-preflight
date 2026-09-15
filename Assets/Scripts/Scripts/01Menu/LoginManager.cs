using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class LoginManager : MonoBehaviour
{

	[SerializeField] private InputField emailInput, passwordInput;
	private string postURl;
	private JsonData data;
	public Text loginInfoText;
	[SerializeField]
	private GameObject loadingBG;

	public static string userName, userEmail,password;
	public static bool guestLogin;

	[SerializeField]
	private Image submitBtn;
	void Start()
	{
		postURl = "https://fillar.in/backend-fillar/public/api/auth/login";
		passwordInput.contentType = InputField.ContentType.Password;
		loginInfoText.text = "";
		if (loadingBG != null)
			loadingBG.SetActive(false);
	}

	public void ShowPassWorld()
	{
		passwordInput.contentType = InputField.ContentType.Standard;
		passwordInput.ForceLabelUpdate();
	}

	public void HidePassWorld()
	{
		passwordInput.contentType = InputField.ContentType.Password;
		passwordInput.ForceLabelUpdate();
	}

	public void SubmitButton()
	{
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			loginInfoText.text = "Please Check internet connection!";
		}
		else if (emailInput.text == "")
		{

			loginInfoText.text = "Please enter your email";

		}
		else if (passwordInput.text == "")
		{			
			loginInfoText.text = "Please enter your password";

		}
		else
		{
			StartCoroutine("PostEmailAndPassword");
		}
	}
	//https://fillar.in/backend-fillar/public/api/getExperiencesList
	IEnumerator PostEmailAndPassword()
	{
		WWWForm form = new WWWForm();
		form.AddField("email", emailInput.text);
		form.AddField("password", passwordInput.text);

        UnityWebRequest post = UnityWebRequest.Post(postURl, form);

        yield return post.SendWebRequest();

        data = JsonMapper.ToObject(post.downloadHandler.text);
		Debug.Log("respon "+ post.downloadHandler.text);

		
		if (data["status"].ToString() == "True" || data["status"].ToString() == "true")
		{

			PlayerPrefs.SetString("mytoken", data["access_token"].ToString());

			PlayerPrefs.SetString("login", "mail & password is saved");

		

			loginInfoText.text = "Login sucsess";

			userName = data["user"]["name"].ToString();
			userEmail = data["user"]["email"].ToString();
			password = passwordInput.text;

            PlayerPrefs.SetString("userName", userName);
            PlayerPrefs.SetString("userEmail", userEmail);
            PlayerPrefs.SetString("password", password);

            guestLogin = false;

			yield return new WaitForSeconds(2);
			if (loadingBG != null)
				loadingBG.SetActive(true);

			yield return new WaitForSeconds(3);

			if (loadingBG != null)
				loadingBG.SetActive(true);

			SceneManager.LoadScene(2);

		}
		else
		{
			yield return new WaitForSeconds(1);
			loginInfoText.text = "Please enter valid details";
		}
	}

	public void ClickOnGuest()
	{
		StartCoroutine("LoadAsGuest");
	}

	IEnumerator LoadAsGuest()
	{
		if (loadingBG != null)
			loadingBG.SetActive(true);

		guestLogin = true;

		yield return new WaitForSeconds(3);

		if (loadingBG != null)
			loadingBG.SetActive(true);

		SceneManager.LoadScene(3);
	}

	int fieldCount = 0;
	public void FieldEntered()
	{
		if (emailInput.text != "" || passwordInput.text != "")
		{
			fieldCount++;
		}
		if (fieldCount > 0)
		{
			submitBtn.color = Color.white;
		}

		if (fieldCount > 1)
		{
			if (emailInput.text != "" && passwordInput.text != "")
			{
				submitBtn.color = Color.blue;
				fieldCount = 0;
			}
		}
	}
	//harikrishna@fillar.in
   //harikrishna
}
