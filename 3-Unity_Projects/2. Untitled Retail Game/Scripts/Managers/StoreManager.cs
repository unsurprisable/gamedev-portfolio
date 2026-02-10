using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class StoreManager : NetworkBehaviour
{
    // POSSIBLE WARNING:
    // this whole class is just managed per-client since it works with a lot of unserializable types.
    // i don't know the full extent of how this class is going to be used in the future, but for right now this seems fine.
    // (the server will be the one handling customer behavior so clients should only use this class to semi-reliably track the products that the store sells)

    public static StoreManager Instance { get; private set; }

    public enum StorageType { ITEM_RACK, CLOSED_FRIDGE, OPEN_FRIDGE, FREEZER, PRODUCE_BIN, WARMER }

    public event EventHandler OnStoreOpenChanged;

    // [Header("Logistics")]
    private Dictionary<StoreItemSO, int> storeItemData;
    private List<ProductDisplayObject> registeredDisplayObjects;
    // private List<StorageVolume> registeredStorageVolumes;
    // private List<...> registeredCheckouts;
    // private List<...> registeredInventoryStorages;

    [Header("Store Properties")]
    [SerializeField] private bool isOpen;

    [Space]
    [Header("Customers")]
    [SerializeField] private Transform storeSpawnPoint;
    [SerializeField] private Transform customerPrefab;
    [SerializeField] private float customerCooldown;
    [SerializeField] private float customerCooldownLeft;


    public void Awake() {
        Instance = this;

        registeredDisplayObjects = new List<ProductDisplayObject>();
    }

    void Start()
    {
        StartCoroutine(InitializeRegisteredData());

        // temp until i figure out how this is actually gonna be determined
        // (for now customers will just look for every item that exists in the game automatically)
    }

    /// <summary>
    /// Required for the store to recognize a ProductDisplayObject's product data
    /// </summary>
    public void Register(ProductDisplayObject displayObject) {
        registeredDisplayObjects.Add(displayObject);
    }

    // runs one frame AFTER start to make sure every scene object has registered itself by the time this runs
    private IEnumerator InitializeRegisteredData() {
        yield return null; // wait for one frame

        RetrieveStoreItemData();
        Debug.Log(FormatItemDataDictionary(storeItemData));
    }

    /// <summary>
    /// Generates a dictionary of every store item quantity by iterating through every registered display object.
    /// </summary>
    private void RetrieveStoreItemData() {
        Dictionary<StoreItemSO, int> storeItemData = new Dictionary<StoreItemSO, int>();       

        foreach (ProductDisplayObject displayObject in registeredDisplayObjects) {
            var itemDict = displayObject.GetStoreItemDictionary();
            foreach (StoreItemSO storeItemSO in itemDict.Keys) {
                storeItemData[storeItemSO] = storeItemData.GetValueOrDefault(storeItemSO, 0) + itemDict[storeItemSO];
            }
        }

        this.storeItemData = storeItemData;
    }

    private string FormatItemDataDictionary(Dictionary<StoreItemSO, int> dictionary) {
        StringBuilder formattedDict = new StringBuilder("{");
        foreach (StoreItemSO itemSO in dictionary.Keys) {
            if (formattedDict.Length > 1) formattedDict.Append(", ");
            formattedDict.Append($"{itemSO.name}: {dictionary[itemSO]}");
        }
        formattedDict.Append("}");
        return formattedDict.ToString();
    }

    public void OpenStore() {
        if (isOpen) return;
        isOpen = false;
        OnStoreOpenChanged?.Invoke(this, EventArgs.Empty);
    }
    public void CloseStore() {
        if (!isOpen) return;
        isOpen = false;
        OnStoreOpenChanged?.Invoke(this, EventArgs.Empty);
    }



    private void Update() {
        if (isOpen) {
            customerCooldownLeft -= Time.deltaTime;
            if (customerCooldownLeft <= 0) {
                NetworkManager.SpawnManager.InstantiateAndSpawn(customerPrefab.GetComponent<NetworkObject>(), destroyWithScene: true, position: storeSpawnPoint.position);
                customerCooldownLeft = customerCooldown;
            }
        }
    }


    private HashSet<StoreItemSO> GetAvailableProducts() {
        return new HashSet<StoreItemSO>(storeItemData.Keys);
    }

    public ProductDisplayObject SearchForItemInStore(StoreItemSO storeItemSO) {
        // probably quicker overall than risking having to search the entire display object list when there's no product available
        if (!GetAvailableProducts().Contains(storeItemSO)) return null;

        // this could get very slow if the store is large; probably should cache this and then track changes w/ events to optimize
        List<ProductDisplayObject> validDisplayObjects = registeredDisplayObjects.FindAll(obj => obj.GetStoreItemAmount(storeItemSO) > 0);
        if (validDisplayObjects.Count == 0) {
            return null;
        } else {
            return validDisplayObjects[UnityEngine.Random.Range(0, validDisplayObjects.Count)];
        }
    }

}
