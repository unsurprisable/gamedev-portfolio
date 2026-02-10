using System;
using Unity.Netcode;
using UnityEngine;

public class StorageVolume : InteractableNetworkObject
{

    public event EventHandler OnStoreItemChanged;
    public event EventHandler<OnItemAmountChangedEventArgs> OnItemAmountChanged;
    public class OnItemAmountChangedEventArgs {
        public int previousAmount;
        public int newAmount;
    }
    
    [Space]
    [Header("Item Storage Space")]
    [SerializeField] private Transform displayOrigin;
    [SerializeField] private StoreManager.StorageType storageType;

    [Space]

    [SerializeField] private StoreItemSO storeItemSO;
    private NetworkVariable<int> itemAmount = new NetworkVariable<int>();

    private Transform[] itemDisplayPool;

    [Space]

    [SerializeField] private AnimationCurve interactHeldCooldownCurve; // should probably make this static somewhere to save memory (rn every single volume has one of these in memory)
    private float interactHeldCooldownLeft;

    // private bool performanceTestForward;



    #region Functionality

    public override void OnNetworkSpawn()
    {
        if (IsServer) {
            NetworkManager.Singleton.SceneManager.OnSynchronize += NetworkManager_OnSynchronize;
        }

        // StoreManager.Instance.Register(this);


        itemAmount.OnValueChanged += ItemAmount_OnValueChanged;

        if (IsServer && storeItemSO != null) {
            SetStoreItemSO(storeItemSO);
            // if (transform.parent.GetComponent<Animator>() != null) {
            //     performanceTestForward = true;
            //     return; //for performance test
            // }
            itemAmount.Value = storeItemSO.storageAmount;
        }
    }

    private void ItemAmount_OnValueChanged(int previousValue, int newValue)
    {
        OnItemAmountChanged?.Invoke(this, new OnItemAmountChangedEventArgs {
            previousAmount = previousValue,
            newAmount = newValue,
        });

        UpdateVisualForItemAmountChange(previousValue, newValue);
    }

    private void NetworkManager_OnSynchronize(ulong clientId)
    {
        if (storeItemSO == null) return;
        SynchronizeItemDataRpc(storeItemSO.ID, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SynchronizeItemDataRpc(int storeItemId, RpcParams rpcParams)
    {
        SetStoreItemSO(StoreItemSO.FromId(storeItemId));

        UpdateVisualForItemAmountChange(0, itemAmount.Value);
    }

    private void UpdateVisualForItemAmountChange(int preValue, int newValue)
    {
            if (preValue == newValue) return;
            if (preValue < newValue) {
                for (int i = preValue; i < newValue; i++) {
                    AddItemToDisplay(i);
                }
            }
            if (preValue > newValue) {
                for (int i = preValue-1; i >= newValue; i--) {
                    RemoveItemFromDisplay(i);
                }
            }
            UpdateStorageVolumeUI();
    }

    public override void OnHovered()
    {
        if (storeItemSO == null) return;

        StorageVolumeUI.Instance.UpdateInfo(storeItemSO, itemAmount.Value);
        StorageVolumeUI.Instance.Show();

        if (PlayerController.LocalInstance.GetHeldItem() is ItemScannerItem scanner) {
            scanner.SetStoreItemSO(storeItemSO);
        }
    }
    public override void OnUnhovered()
    {
        StorageVolumeUI.Instance.Hide();

        if (PlayerController.LocalInstance.GetHeldItem() is ItemScannerItem scanner) {
            scanner.ResetStoreItemSO();
        }
    }

    // double repetition here, but imo it's more efficient than making a general "interact OR alternate interact" event
    public override void OnInteract(PlayerController player)
    {
        // reset the cooldown so the actual functionality in OnInteractHeld triggers instantly
        interactHeldCooldownLeft = 0f;
    }

    public override void OnInteractSecondary(PlayerController player)
    {
        interactHeldCooldownLeft = 0f;
    }

    public override void OnInteractHeld(PlayerController player, float time)
    {
        interactHeldCooldownLeft -= Time.deltaTime;

        if (interactHeldCooldownLeft <= 0) {
            AddItemServerRpc(player);
            interactHeldCooldownLeft = interactHeldCooldownCurve.Evaluate(time);
        }
    }

    public override void OnAlternateHeld(PlayerController player, float time)
    {
        interactHeldCooldownLeft -= Time.deltaTime;

        if (interactHeldCooldownLeft <= 0) {
            RemoveItemServerRpc(player);
            interactHeldCooldownLeft = interactHeldCooldownCurve.Evaluate(time);
        }
    }

    public void CustomerRemoveItem() {
        if (!IsServer) return;
        
        itemAmount.Value--;
    }

    [Rpc(SendTo.Server)]
    private void AddItemServerRpc(NetworkBehaviourReference senderObject)
    {
        senderObject.TryGet(out PlayerController sender);
        if (sender.GetHeldItem() != null && sender.GetHeldItem() is ContainerItem container)
        { 
            if (container.IsEmpty() ||
                container.GetStoreItemSO().storageType != storageType ||
                (storeItemSO != null && container.GetStoreItemSO() != storeItemSO) ||
                (storeItemSO != null && itemAmount.Value == storeItemSO.storageAmount)
            ) return;

            AddItemClientRpc(container);
            itemAmount.Value++;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AddItemClientRpc(NetworkBehaviourReference containerObject)
    {
        containerObject.TryGet(out ContainerItem container);
        if (storeItemSO == null) SetStoreItemSO(container.GetStoreItemSO());
        container.RemoveItem();
    }

    [Rpc(SendTo.Server)]
    private void RemoveItemServerRpc(NetworkBehaviourReference senderObject)
    {
        senderObject.TryGet(out PlayerController sender);
        if (sender.GetHeldItem() != null && sender.GetHeldItem() is ContainerItem container && itemAmount.Value > 0 && !container.IsFull() && container.GetStoreItemSO() == storeItemSO) {
            
            RemoveItemClientRpc(container);
            itemAmount.Value--;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RemoveItemClientRpc(NetworkBehaviourReference containerObject)
    {
        containerObject.TryGet(out ContainerItem container);
        container.AddItem();
    }

    private void GenerateItemDisplay(int itemIndex)
    {
        Transform itemDisplay = Instantiate(storeItemSO.prefab);
        itemDisplayPool[itemIndex] = itemDisplay;

        // move them away from the corner
        Vector3 addedPosition = new Vector3(storeItemSO.modelDimensions.x/2, 0f, -storeItemSO.modelDimensions.z/2);

        // shift each object based on existing ones... how fun :)
        addedPosition.x += storeItemSO.modelDimensions.x * itemIndex;

        int horizontalWraps = Mathf.FloorToInt(itemIndex / storeItemSO.storageCapacity.x);
        addedPosition.x -= horizontalWraps * storeItemSO.modelDimensions.x * storeItemSO.storageCapacity.x;
        addedPosition.z -= horizontalWraps * storeItemSO.modelDimensions.z;

        int verticalWraps = Mathf.FloorToInt(itemIndex / (storeItemSO.storageCapacity.x * storeItemSO.storageCapacity.z));
        addedPosition.z += verticalWraps * storeItemSO.modelDimensions.z * storeItemSO.storageCapacity.z;
        addedPosition.y += verticalWraps * storeItemSO.modelDimensions.y;

        float slantTheta = transform.rotation.eulerAngles.x * Mathf.Deg2Rad;
        float verticalSlant = addedPosition.z * Mathf.Tan(slantTheta);
        addedPosition.y -= verticalSlant;

        // rotate the addedPosition relative to the Y axis rotation of the storage object
        // (so that these translations will always be relative to the object, regardless of its rotation)
        float theta = -transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
        addedPosition = new Vector3(
            addedPosition.x * Mathf.Cos(theta) - addedPosition.z * Mathf.Sin(theta), addedPosition.y,
            addedPosition.x * Mathf.Sin(theta) + addedPosition.z * Mathf.Cos(theta)
        );

        Vector3 itemPosition = displayOrigin.position + addedPosition;

        itemDisplay.SetPositionAndRotation(itemPosition + itemDisplay.transform.position, Quaternion.Euler(0f, transform.rotation.eulerAngles.y + 180f, 0f));

        itemDisplay.gameObject.SetActive(false);
    }

    private void UpdateStorageVolumeUI()
    {
        if (isHovered) {
            StorageVolumeUI.Instance.UpdateInfo(storeItemSO, itemAmount.Value);
            StorageVolumeUI.Instance.Show();
        }
    }

    private void AddItemToDisplay(int itemIndex)
    {
        itemDisplayPool[itemIndex].gameObject.SetActive(true);
    }

    private void RemoveItemFromDisplay(int itemIndex)
    {
        itemDisplayPool[itemIndex].gameObject.SetActive(false);
    }

    // serves as an event so that the object pool can be re-generated when the storeItemSO changes
    private void SetStoreItemSO(StoreItemSO storeItemSO)
    {
        this.storeItemSO = storeItemSO;

        OnStoreItemChanged?.Invoke(this, EventArgs.Empty);

        if (itemDisplayPool != null) {
            foreach (Transform itemDisplay in itemDisplayPool) {
                Destroy(itemDisplay.gameObject);
            }
        }
        
        itemDisplayPool = new Transform[storeItemSO.storageAmount];

        for (int i = 0; i < storeItemSO.storageAmount; i++) {
            GenerateItemDisplay(i);
        }
    }

    public override void OnDestroy()
    {
        if (itemDisplayPool != null) {
            foreach (Transform itemDisplay in itemDisplayPool) {
                if (itemDisplay == null) continue;
                Destroy(itemDisplay.gameObject);
            }
        }
    }

    // private void FixedUpdate()
    // {
    //     if (performanceTestForward) {
    //         itemAmount.Value++;
    //         if (itemAmount.Value == storeItemSO.storageAmount) performanceTestForward = false;
    //     } else {
    //         itemAmount.Value--;
    //         if (itemAmount.Value == 0) performanceTestForward = true;
    //     }
    // }

    #endregion





    #region Getters

    public StoreItemSO GetStoreItemSO() {
        return storeItemSO;
    }
    public int GetItemAmount() {
        return itemAmount.Value;
    }

    #endregion
}
