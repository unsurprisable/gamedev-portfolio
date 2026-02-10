using UnityEngine;
using UnityEngine.UI;

public class ProductOrderSingleUI : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI label;
    [SerializeField] private TMPro.TextMeshProUGUI price;
    [SerializeField] private Button removeButton;

    public StoreItemSO storeItemSO;

    public void SetStoreItemSO(StoreItemSO storeItemSO)
    {
        this.storeItemSO = storeItemSO;
        label.text = storeItemSO.name + " [x" + storeItemSO.containerAmount + "]";
        price.text = "- $" + (storeItemSO.unitPrice * storeItemSO.containerAmount).ToString("0.00");

        removeButton.onClick.AddListener(() => {
            OrderMenuUI.Instance.RemoveItemFromOrder(this);
        });
    }
}
