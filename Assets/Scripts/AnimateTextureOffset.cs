using UnityEngine;

public class AnimateTextureOffset : MonoBehaviour
{
    public Renderer targetRenderer;
    public float speed = 1.0f; // Speed of animation

    private Material material;
    private Vector2 offset;

    void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        material = targetRenderer.material;
        offset = material.mainTextureOffset;
    }

    void Update()
    {
        offset.x += speed * Time.deltaTime; // Move texture in X direction
        material.mainTextureOffset = offset;
    }
}
