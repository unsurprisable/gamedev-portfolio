using System;
using Unity.Netcode;
using UnityEngine;

public class ItemScannerItem : HoldableItem
{
    [Header("Item Scanner")]
    [SerializeField] private TMPro.TextMeshPro itemName;
    [SerializeField] private TMPro.TextMeshPro stockPrice;
    [SerializeField] private TMPro.TextMeshPro sellPrice;

    private StoreItemSO storeItemSO;
    private float itemPrice;
    private float newItemPrice;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color unsavedColor;



    public override void OnUse(PlayerController player)
    {
        if (storeItemSO != null && newItemPrice != itemPrice)
        {
            SetItemPriceServerRpc(storeItemSO.ID, newItemPrice);
        }
    }

    public override void OnUseSecondary(PlayerController player)
    {
        if (storeItemSO == null) return;

        newItemPrice = itemPrice;
        PreviewNewItemPrice();
    }

    public override void OnDrop()
    {
        ResetStoreItemSO();
    }

    public void SetStoreItemSO(StoreItemSO storeItemSO)
    {
        this.storeItemSO = storeItemSO;
        itemName.text = storeItemSO.name;
        stockPrice.text = "$" + storeItemSO.unitPrice.ToString("0.00");
        itemPrice = EconomyManager.Instance.GetStoreItemPrice(storeItemSO);
        newItemPrice = itemPrice;

        PreviewNewItemPrice();

        GameInput.Instance.OnScroll += HandleScrollWheel;
        EconomyManager.Instance.OnItemPriceChanged += GameManager_OnItemPriceChanged;
    }

    private void GameManager_OnItemPriceChanged(object sender, EconomyManager.OnItemPriceChangedEventArgs e)
    {
        if (storeItemSO == e.storeitemSO) {
            itemPrice = e.newItemPrice;
            PreviewNewItemPrice();
        }
    }

    public void ResetStoreItemSO()
    {
        storeItemSO = null;
        itemName.text = "";
        stockPrice.text = "$0.00";
        itemPrice = 0;
        newItemPrice = 0;

        PreviewNewItemPrice();
        
        GameInput.Instance.OnScroll -= HandleScrollWheel;
        EconomyManager.Instance.OnItemPriceChanged -= GameManager_OnItemPriceChanged;
    }

    [Rpc(SendTo.Server)]
    private void SetItemPriceServerRpc(int storeItemId, float newItemPrice)
    {
        EconomyManager.Instance.SetStoreItemPriceRpc(storeItemId, newItemPrice);
    }

    private void PreviewNewItemPrice()
    {
        sellPrice.text = "$" + newItemPrice.ToString("0.00");
        sellPrice.color = newItemPrice == itemPrice ? defaultColor : unsavedColor;
    }

    private void HandleScrollWheel(object sender, EventArgs e)
    {
        if (storeItemSO == null) {
            Debug.LogWarning("A scanner was listening to scroll inputs but didn't have a StoreItemSO!");
            return;
        }

        float priceDelta = GameInput.Instance.GetIsSprinting() ? 0.1f : 0.01f;
        
        int scrollDirection = GameInput.Instance.GetScrollDirection();
        if (newItemPrice == 0f && scrollDirection == -1) return;

        newItemPrice += priceDelta * scrollDirection;
        if (newItemPrice < 0f) newItemPrice = 0f;
        PreviewNewItemPrice();
    }
}
