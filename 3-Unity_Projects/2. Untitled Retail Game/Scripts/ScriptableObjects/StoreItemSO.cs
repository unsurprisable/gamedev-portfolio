using System;
using UnityEngine;

[CreateAssetMenu()]
[Serializable]
public class StoreItemSO : ScriptableObject
{

    [Space]

    [Header("Metadata")]
    [Tooltip("The item's display name.")]
    public new string name;
    [Tooltip("The item's icon (make sure it's 128x128).")]
    public Sprite icon; // 128x128 for consistent sizing!!!
    [Tooltip("The item's in-game model.")]
    public Transform prefab;

    [Space]
    
    [Header("Order Data")]
    [Tooltip("Amount of this item that a container can hold.")]
    public int containerAmount;
    [Tooltip("Price per unit (not per stack).")]
    public float unitPrice;
    [Tooltip("The type of storage this item requires.")]
    public StoreManager.StorageType storageType;
    [Tooltip("The shop category this item will appear in.")]
    public OrderMenuUI.ProductCategory category;

    [Space]

    [Header("Storage Data")]
    [Tooltip("Represents the dimensions of the 3D shape formed from a full storage of this item.")]
    public Vector3Int storageCapacity;
    [Tooltip("Represents the distance each model should be away from each other.")]
    public Vector3 modelDimensions;
    [Tooltip("Should just be equal to the volume of the storage capacity.")]
    public int storageAmount;


    public int ID => SerializeManager.Instance.StoreItemToID(this);

    public static StoreItemSO FromId(int id) {
        return SerializeManager.Instance.GetStoreItemFromId(id);
    }

    public override string ToString() {
        return name;
    }
}
