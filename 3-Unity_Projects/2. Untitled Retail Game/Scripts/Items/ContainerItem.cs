using Unity.Netcode;
using UnityEngine;

public class ContainerItem : HoldableItem
{
    [Space]
    [Header("Container")]
    [SerializeField] private StoreItemSO storeItem;
    [SerializeField] private SpriteRenderer[] iconObjects;
    [SerializeField] private int itemAmount;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnSynchronize += NetworkManager_OnSynchronize;

            if (storeItem != null) {
                SetStoreItemSORpc(storeItem.ID, RpcTarget.ClientsAndHost);
            }
        }
    }

    private void NetworkManager_OnSynchronize(ulong clientId)
    {
        SetStoreItemSORpc(storeItem.ID, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    // make sure to set this instantly whenever instantiating a new container
    [Rpc(SendTo.SpecifiedInParams, RequireOwnership = true)]
    public void SetStoreItemSORpc(int storeItemId, RpcParams rpcParams)
    {
        storeItem = StoreItemSO.FromId(storeItemId);

        transform.name = "ContainerItem (" + storeItem.name + ")";
        foreach(SpriteRenderer r in iconObjects) {
            r.sprite = storeItem.icon;
        }

        itemAmount = storeItem.containerAmount;
    }

    public bool IsEmpty() {
        return itemAmount == 0;
    }
    public bool IsFull() {
        return itemAmount == storeItem.containerAmount;
    }

    public StoreItemSO GetStoreItemSO() {
        return storeItem;
    }

    public void RemoveItem() {
        itemAmount--;
        if (itemAmount < 0) {
            Debug.LogError(storeItem.name + " container is storing a negative amount of items..?");
        }
    }
    public void AddItem() {
        itemAmount++;
    }
}
