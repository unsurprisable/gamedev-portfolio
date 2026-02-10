using System.Collections.Generic;
using UnityEngine;

public class SerializeManager : MonoBehaviour
{
    public static SerializeManager Instance { get; private set; }

    [SerializeField] private BuildObjectListSO buildObjectList;
    private Dictionary<BuildObjectSO, int> buildToIdDict;

    [SerializeField] private StoreItemListSO storeItemList;
    private Dictionary<StoreItemSO, int> itemToIdDict;



    private void Awake()
    {
        Instance = this;
        
        buildToIdDict = new Dictionary<BuildObjectSO, int>();
        for (int id = 0; id < buildObjectList.list.Length; id++) {
            buildToIdDict[buildObjectList.list[id]] = id;
        }

        itemToIdDict = new Dictionary<StoreItemSO, int>();
        for (int id = 0; id < storeItemList.list.Length; id++) {
            itemToIdDict[storeItemList.list[id]] = id;
        }
    }



    public int BuildObjectToID(BuildObjectSO buildObjectSO)
    {
        return buildToIdDict[buildObjectSO];
    }

    public BuildObjectSO GetBuildObjectFromId(int id)
    {
        return buildObjectList.list[id];
    }



    public int StoreItemToID(StoreItemSO storeItemSO)
    {
        return itemToIdDict[storeItemSO];
    }

    public StoreItemSO GetStoreItemFromId(int id)
    {
        return storeItemList.list[id];
    }



    public BuildObjectListSO GetBuildObjectListSO() {
        return buildObjectList;
    }

    public StoreItemListSO GetStoreItemListSO() {
        return storeItemList;
    }
}
