using UnityEngine;

// [CreateAssetMenu()]
public class StoreItemListSO : ScriptableObject
{
    [Tooltip("Place all StoreItemSO's into this list to register them (order doesn't matter).")]
    public StoreItemSO[] list;
}
