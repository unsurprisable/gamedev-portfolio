using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class EconomyManager : NetworkBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public event EventHandler OnBalanceChanged;

    public event EventHandler<OnItemPriceChangedEventArgs> OnItemPriceChanged;
    public class OnItemPriceChangedEventArgs : EventArgs {
        public StoreItemSO storeitemSO;
        public float newItemPrice;
    }

    private StoreItemListSO storeItemList;
    private Dictionary<int, float> itemPriceDict;

    public float initialStoreBalance;
    private NetworkVariable<float> storeBalance = new NetworkVariable<float>();


    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) {
            NetworkManager.Singleton.SceneManager.OnSynchronize += NetworkManager_OnSynchronize;
            
            storeItemList = SerializeManager.Instance.GetStoreItemListSO();

            itemPriceDict = new Dictionary<int, float>();
            for (int id = 0; id < storeItemList.list.Length; id++) {
                itemPriceDict[id] = storeItemList.list[id].unitPrice;
            }

            storeBalance.Value = initialStoreBalance;
        }

        storeBalance.OnValueChanged += (preValue, newValue) => {
            OnBalanceChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnSynchronize(ulong clientId)
    {
        SynchronizeItemPricesRpc(itemPriceDict.Values.ToArray(), RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SynchronizeItemPricesRpc(float[] values, RpcParams rpcParams)
    {
        itemPriceDict = new Dictionary<int, float>();
        for (int id = 0; id < values.Length; id++) {
            // don't need to pass in keys because the Server itemPriceDict values are created in the same way these are; the order will be the same either way
            itemPriceDict[id] = values[id];
        }
    }

    #region Economy

    public float GetBalance() {
        return storeBalance.Value;
    }

    public void SetBalance(float newBalance) {
        storeBalance.Value = newBalance;
    }

    public void AddToBalance(float income) {
        storeBalance.Value += income;
    }

    public void RemoveFromBalance(float cost) {
        storeBalance.Value -= cost;
    }

    public bool CanAfford(float price) {
        return storeBalance.Value - price >= 0f;
    }

    #endregion

    #region Store Item API

    public StoreItemSO[] GetStoreItemSOList() {
        return storeItemList.list;
    }

    public float GetStoreItemPrice(StoreItemSO storeItemSO) {
        return itemPriceDict[storeItemSO.ID];
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetStoreItemPriceRpc(int storeItemId, float newItemPrice) {
        itemPriceDict[storeItemId] = newItemPrice;
        OnItemPriceChanged?.Invoke(this, new OnItemPriceChangedEventArgs {
            storeitemSO = StoreItemSO.FromId(storeItemId),
            newItemPrice = newItemPrice
        });
    }



    public int[] ConvertStoreItemArrayToID(StoreItemSO[] storeItemSOArray)
    {
        int[] idArray = new int[storeItemSOArray.Length];
        for (int i = 0; i < idArray.Length; i++)
        {
            idArray[i] = storeItemSOArray[i].ID;
        }
        return idArray;
    }

    public StoreItemSO[] ConvertIDArrayToStoreItem(int[] idArray)
    {
        StoreItemSO[] storeItemSOArray = new StoreItemSO[idArray.Length];
        for (int i = 0; i < idArray.Length; i++)
        {
            storeItemSOArray[i] = storeItemList.list[idArray[i]];
        }
        return storeItemSOArray;
    }

    #endregion
}
