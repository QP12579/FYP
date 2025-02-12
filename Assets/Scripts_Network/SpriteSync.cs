using UnityEngine;
using Mirror;

public class SpriteSync : NetworkBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SyncVar(hook = nameof(OnMaterialChanged))]
    private string materialName;

    private void Start()
    {
        // Move initialization to Start instead of Awake
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (isServer && spriteRenderer != null && spriteRenderer.material != null)
        {
            materialName = spriteRenderer.material.shader.name;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Delay the sprite setup by one frame to ensure all components are ready
        StartCoroutine(SetupSpriteDelayed());
    }

    private System.Collections.IEnumerator SetupSpriteDelayed()
    {
        yield return null; // Wait one frame

        if (spriteRenderer != null)
        {
            spriteRenderer.material = new Material(Shader.Find("Sprites/Default"));

            // Preserve sorting settings
            spriteRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
            spriteRenderer.sortingOrder = spriteRenderer.sortingOrder;
        }
    }

    private void OnMaterialChanged(string oldMat, string newMat)
    {
        if (spriteRenderer != null && !string.IsNullOrEmpty(newMat))
        {
            Material newMaterial = new Material(Shader.Find(newMat));
            if (newMaterial != null)
            {
                spriteRenderer.material = newMaterial;
            }
        }
    }
}