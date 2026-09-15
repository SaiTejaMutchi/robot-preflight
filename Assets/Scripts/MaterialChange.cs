using UnityEngine;

public class MaterialChange : MonoBehaviour
{
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        Material mat = rend.material;
        SetURPTransparent(mat);
    }

    void SetURPTransparent(Material mat)
    {
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetInt("_ZWrite", 0);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // 0.5f = 50% transparent — adjust this value as needed
        mat.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.0f));
        mat.color = new Color(1f, 1f, 1f, 0.0f);
    }
}