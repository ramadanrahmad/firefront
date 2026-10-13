using UnityEngine;

public class ProximityOutline : MonoBehaviour
{
    [Header("Highlight Material")]
    public Material highlightMaterial;

    private Renderer objectRenderer;

    private Material[] originalMaterials;
    private Material[] highlightMaterials;

    private bool highlighted = false;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            originalMaterials = objectRenderer.materials;

            highlightMaterials =
                new Material[originalMaterials.Length];

            for (int i = 0; i < highlightMaterials.Length; i++)
            {
                highlightMaterials[i] = highlightMaterial;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EnableHighlight();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DisableHighlight();
        }
    }

    void EnableHighlight()
    {
        if (highlighted) return;

        objectRenderer.materials = highlightMaterials;

        highlighted = true;
    }

    void DisableHighlight()
    {
        if (!highlighted) return;

        objectRenderer.materials = originalMaterials;

        highlighted = false;
    }
}