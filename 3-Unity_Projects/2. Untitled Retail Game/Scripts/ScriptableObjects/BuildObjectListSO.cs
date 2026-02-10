using UnityEngine;

// [CreateAssetMenu()]
public class BuildObjectListSO : ScriptableObject
{
    [Tooltip("Place all BuildObjectSO's into this list to register them (DO NOT CHANGE THE ORDER OR IT WILL MESS WITH STORED IDs).")]
    public BuildObjectSO[] list;
}
