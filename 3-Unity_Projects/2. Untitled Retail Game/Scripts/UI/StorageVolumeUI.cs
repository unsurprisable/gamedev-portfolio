using UnityEngine;
using UnityEngine.UI;

public class StorageVolumeUI : MonoBehaviour
{

    public static StorageVolumeUI Instance { get; private set; }

    [SerializeField] private Image icon;
    [SerializeField] private TMPro.TextMeshProUGUI itemName;
    [SerializeField] private TMPro.TextMeshProUGUI itemAmount;


    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void UpdateInfo(StoreItemSO storeItem, int amount)
    {
        icon.sprite = storeItem.icon;
        itemName.text = storeItem.name;
        itemAmount.text = "x" + amount.ToString();
    }

    public void Show() {
        gameObject.SetActive(true);
    }
    public void Hide() {
        gameObject.SetActive(false);
    }
}
