using UnityEngine;

public class GameInfoUI : MonoBehaviour
{
    public static GameInfoUI Instance { get; private set; }

    [SerializeField] private TMPro.TextMeshProUGUI balanceText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        EconomyManager.Instance.OnBalanceChanged += (sender, args) => {
            UpdateVisual(((EconomyManager)sender).GetBalance());
        };
        
        UpdateVisual(EconomyManager.Instance.initialStoreBalance);
    }

    private void UpdateVisual(float newBalance) {
        balanceText.text = "$ " + newBalance.ToString("0,000,000.00");
    }
}
