using UnityEngine;
using System;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class VirtualKeyboardHandler : MonoBehaviour
{
    public NonNativeKeyboard keyboard;
    public TMP_InputField separateInputFeild;
    bool isTextSubmitted = false;
    public static string submittedText;
    public string nextScene;

    private void OnEnable()
    {
        keyboard.OnTextSubmitted += HandleTextSubmitted;
        keyboard.OnTextUpdated += HandleTextUpdated;
    }

    private void OnDisable()
    {
        keyboard.OnTextSubmitted -= HandleTextSubmitted;
        keyboard.OnTextUpdated -= HandleTextUpdated;
    }

    void HandleTextSubmitted(object sender, EventArgs e)
    {
        isTextSubmitted = true;
        submittedText = keyboard.InputField.text;
        // separateInputFeild.text = submittedText;

        Debug.Log("Captured Text: " + submittedText);

        SceneManager.LoadScene(nextScene);
    }

    void HandleTextUpdated(string text)
    {
        if (!isTextSubmitted && separateInputFeild != null)
        {
            separateInputFeild.text = text;
            Debug.Log("TextUpdated !!");
        }

        isTextSubmitted = false;
    }
}
