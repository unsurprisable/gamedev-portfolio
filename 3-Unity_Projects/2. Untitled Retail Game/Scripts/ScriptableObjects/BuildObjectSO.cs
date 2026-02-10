using UnityEngine;

[CreateAssetMenu()]
public class BuildObjectSO : ScriptableObject
{
    [Tooltip("The name of this object.")]
    public new string name;
    [Tooltip("The cost of building this object.")]
    public int price;
    [Tooltip("The prefab of the object itself.")]
    public Transform prefab;
    [Tooltip("The prefab of the object's build mode variant.")]
    public Transform buildModePrefab;
    [Tooltip("Preview of the object, shown in the build menu.")]
    public Sprite preview;
    [Tooltip("The category that this object will be displayed in.")]
    public BuildMenuUI.BuildCategory category;


    public int ID => SerializeManager.Instance.BuildObjectToID(this);

    public static BuildObjectSO FromId(int id) {
        return SerializeManager.Instance.GetBuildObjectFromId(id);
    }
}
