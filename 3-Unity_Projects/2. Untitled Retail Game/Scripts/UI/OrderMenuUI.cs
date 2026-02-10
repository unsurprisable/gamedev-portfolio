using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OrderMenuUI : NetworkMenu
{
    public static OrderMenuUI Instance { get; private set; }

    public enum ProductCategory { BASIC_PRODUCTS, DAIRY_PRODUCTS, }

    [SerializeField] private GameObject[] categoryButtons;
    [Space]
    [SerializeField] private Transform productButtonTemplate;
    [SerializeField] private Transform productButtonParent;
    [SerializeField] private Transform productOrderTemplate;
    [SerializeField] private Transform productOrderParent;
    [SerializeField] private int maxOrders;
    private float orderTotalPrice;

    [Space]
    [SerializeField] private Button placeOrderButton;
    [SerializeField] private TMPro.TextMeshProUGUI orderTotalText;
    [SerializeField] private Color orderButtonEnabledColor;
    [SerializeField] private Color orderButtonDisabledColor;

    private Dictionary<StoreItemSO, int> currentOrder;
    private List<GameObject> activeOrderButtons = new List<GameObject>();
    private int queuedOrderCount;

    private HashSet<GameObject> activeProductButtons;
    private Dictionary<ProductCategory, List<StoreItemSO>> categoryDict;
    
    [Space]

    [SerializeField] private Transform containerPrefab;
    [SerializeField] private Transform containerSpawnLocation;


    private void Awake()
    {
        Instance = this;

        currentOrder = new Dictionary<StoreItemSO, int>();
        activeProductButtons = new HashSet<GameObject>();

        categoryButtons[0].GetComponent<Button>().onClick.AddListener(()=>{ ShowCategory(ProductCategory.BASIC_PRODUCTS); });
        categoryButtons[1].GetComponent<Button>().onClick.AddListener(()=>{ ShowCategory(ProductCategory.DAIRY_PRODUCTS); });

        placeOrderButton.onClick.AddListener(() => {
            int[] orderItemSOArray = EconomyManager.Instance.ConvertStoreItemArrayToID(currentOrder.Keys.ToArray());
            int[] orderAmountArray = currentOrder.Values.ToArray();
            
            PlaceOrderServerRpc(orderTotalPrice, orderItemSOArray, orderAmountArray);
        });
    }

    private void Start()
    {
        categoryDict = new Dictionary<ProductCategory, List<StoreItemSO>>();
        foreach (StoreItemSO storeItemSO in EconomyManager.Instance.GetStoreItemSOList())
        {
            if (!categoryDict.ContainsKey(storeItemSO.category)) {
                categoryDict[storeItemSO.category] = new List<StoreItemSO>();
            }
            categoryDict[storeItemSO.category].Add(storeItemSO);
        }
        
        EconomyManager.Instance.OnBalanceChanged += (sender, args) => {
            UpdateOrderButtonState();
        };
        UpdateOrderButtonState();
    }

    [Rpc(SendTo.Server)]
    private void PlaceOrderServerRpc(float orderTotalPrice, int[] orderItemSOArray, int[] orderAmountArray, RpcParams rpcParams = default)
    {
        if (!EconomyManager.Instance.CanAfford(orderTotalPrice)) return;

        EconomyManager.Instance.RemoveFromBalance(orderTotalPrice);

        for (int item = 0; item < orderItemSOArray.Length; item++)
        {
            int orderAmount = orderAmountArray[item];
            // account for ordering multiple of the same item
            for (int amount = 0; amount < orderAmount; amount++) {
                Transform container = Instantiate(containerPrefab, containerSpawnLocation.position, Quaternion.identity);
                container.GetComponent<NetworkObject>().Spawn(true);
                container.GetComponent<ContainerItem>().SetStoreItemSORpc(orderItemSOArray[item], RpcTarget.ClientsAndHost);
            }
        }

        PlaceOrderClientCallbackRpc(RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void PlaceOrderClientCallbackRpc(RpcParams rpcParams)
    {
        foreach (GameObject orderButton in activeOrderButtons) {
            Destroy(orderButton);
        }
        activeOrderButtons.Clear();
        currentOrder.Clear();
        queuedOrderCount = 0;
        orderTotalPrice = 0f;
        UpdateOrderTotalText();
        UpdateOrderButtonState();
        
        Hide();
    }

    private void ShowCategory(ProductCategory category)
    {
        if (activeProductButtons.Count != 0) {
            foreach (GameObject productButton in activeProductButtons) {
                Destroy(productButton);
            }
            activeProductButtons.Clear();
        }

        if (!categoryDict.ContainsKey(category)) {
            Debug.LogWarning("That category does not contain any items!");
            return;
        }

        List<StoreItemSO> storeItems = categoryDict[category];
        foreach (StoreItemSO storeItemSO in storeItems) {
            activeProductButtons.Add(CreateProductButton(storeItemSO));
        }
    }

    private GameObject CreateProductButton(StoreItemSO storeItemSO)
    {
        Transform productButton = Instantiate(productButtonTemplate, productButtonParent);
        productButton.GetComponent<ProductButtonSingleUI>().SetStoreItemSO(storeItemSO);
        return productButton.gameObject;
    }

    public void AddItemToOrder(StoreItemSO storeItemSO)
    {
        if (queuedOrderCount == maxOrders) {
            return;
        }
        if (!currentOrder.ContainsKey(storeItemSO)) {
            currentOrder[storeItemSO] = 0;
        }
        currentOrder[storeItemSO] += 1;
        activeOrderButtons.Add(CreateOrderButton(storeItemSO));
        orderTotalPrice += storeItemSO.unitPrice * storeItemSO.containerAmount;
        UpdateOrderTotalText();
        queuedOrderCount++;
        UpdateOrderButtonState();
    }

    private GameObject CreateOrderButton(StoreItemSO storeItemSO)
    {
        Transform orderButton = Instantiate(productOrderTemplate, productOrderParent);
        orderButton.GetComponent<ProductOrderSingleUI>().SetStoreItemSO(storeItemSO);
        return orderButton.gameObject;
    }

    public void RemoveItemFromOrder(ProductOrderSingleUI order)
    {
        Destroy(order.gameObject);
        activeOrderButtons.Remove(order.gameObject);
        orderTotalPrice -= order.storeItemSO.unitPrice * order.storeItemSO.containerAmount;
        UpdateOrderTotalText();
        queuedOrderCount--;
        UpdateOrderButtonState();
    }

    private void UpdateOrderTotalText()
    {
        orderTotalText.text = "Order Total: $" + orderTotalPrice.ToString("0.00");
    }

    private void UpdateOrderButtonState()
    {
        bool canOrder = queuedOrderCount != 0 && EconomyManager.Instance.CanAfford(orderTotalPrice);
        placeOrderButton.gameObject.GetComponent<Image>().color = canOrder ? orderButtonEnabledColor : orderButtonDisabledColor;
        placeOrderButton.enabled = canOrder;
    }
}
