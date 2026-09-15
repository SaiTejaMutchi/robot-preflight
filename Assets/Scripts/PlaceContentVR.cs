using UnityEngine;

public class PlaceContentVR : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FindFirstObjectByType<ContentManager>().LoadContent();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
