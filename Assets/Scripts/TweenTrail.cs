
using UnityEngine;

public class TweenTrail : MonoBehaviour
{
    //Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // iTween.MoveTo(gameObject, new Vector3(2, 0.5f, 0), 2);
        // iTween.RotateTo(gameObject, iTween.Hash("rotation", new Vector3(0, 180, 0), "time", 1f));
        //iTween.FadeFrom(gameObject, iTween.Hash("alpha", 1, "time", 4.0f));

        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 4.0f, "onupdate", "OnFadeUpdate", "onupdatetarget", gameObject));

        Renderer rend = GetComponent<Renderer>();
        Material mat = rend.material;

        Debug.Log(mat.color.r);
    }
    void OnFadeUpdate(float alpha)
    {

        Renderer rend = GetComponent<Renderer>();
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
