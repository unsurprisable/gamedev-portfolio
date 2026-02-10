using UnityEngine;
using UnityEngine.UI;

public class ProductButtonSingleUI : MonoBehaviour
{
    [SerializeField] private Image preview;
    [SerializeField] private TMPro.TextMeshProUGUI label;
    [SerializeField] private TMPro.TextMeshProUGUI amount;
    [SerializeField] private TMPro.TextMeshProUGUI price;
    [SerializeField] private Button addButton;

    private StoreItemSO storeItemSO;

    public void SetStoreItemSO(StoreItemSO storeItemSO)
    {
        this.storeItemSO = storeItemSO;
        preview.sprite = storeItemSO.icon;
        label.text = storeItemSO.name;
        amount.text = "x" + storeItemSO.containerAmount;
        price.text = "$" + (storeItemSO.unitPrice * storeItemSO.containerAmount).ToString("0.00");

        addButton.onClick.AddListener(() => {
            OrderMenuUI.Instance.AddItemToOrder(storeItemSO);
        });
    }
}
