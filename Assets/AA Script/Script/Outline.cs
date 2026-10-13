using UnityEngine;

public class Outline : MonoBehaviour
{
    [Header("Highlight Material")]
    public Material highlightMaterial;

    private Renderer objectRenderer;

    private Material[] originalMaterials;
    private Material[] highlightedMaterials;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            originalMaterials = objectRenderer.materials;

            highlightedMaterials =
                new Material[originalMaterials.Length];

            for (int i = 0; i < highlightedMaterials.Length; i++)
            {
                highlightedMaterials[i] = highlightMaterial;
            }
        }
    }

    public void EnableHighlight()
    {
        if (objectRenderer != null)
        {
            objectRenderer.materials = highlightedMaterials;
        }
    }

    public void DisableHighlight()
    {
        if (objectRenderer != null)
        {
            objectRenderer.materials = originalMaterials;
        }
    }
}