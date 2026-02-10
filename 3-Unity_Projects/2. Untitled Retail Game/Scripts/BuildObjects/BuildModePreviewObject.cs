using UnityEngine;

public class BuildModePreviewObject : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    public Transform buildBounds;

    void Start()
    {
        if (meshRenderer.GetComponent<Collider>() != null) {
            Debug.LogWarning("BuildModePreviewObject " + name + " still has a physical collider! Make sure to remove it from the prefab.");
        }
    }

    public void SetMaterial(Material material)
    {
        meshRenderer.material = material;
    }
}
