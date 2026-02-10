using UnityEngine;
using UnityEngine.UI;

public class BuildButtonSingleUI : MonoBehaviour
{
    [SerializeField] private Image preview;
    [SerializeField] private TMPro.TextMeshProUGUI label;
    [SerializeField] private TMPro.TextMeshProUGUI price;
    [SerializeField] private Button button;
    [Space]
    [SerializeField] private Button favoriteButton;
    [SerializeField] private Sprite unfavoritedIcon;
    [SerializeField] private Sprite favoritedIcon;
    [SerializeField] private bool isFavorited;

    private BuildObjectSO buildObjectSO;

    public void SetBuildObjectSO(BuildObjectSO buildObjectSO)
    {
        // retrieve if its favorited or not and update icon
        isFavorited = BuildMenuUI.Instance.IsBuildObjectSOFavorited(buildObjectSO);
        favoriteButton.image.sprite = isFavorited ? favoritedIcon : unfavoritedIcon;

        this.buildObjectSO = buildObjectSO;
        preview.sprite = buildObjectSO.preview;
        label.text = buildObjectSO.name;
        price.text = "$" + buildObjectSO.price;

        button.onClick.AddListener(SelectBuild);

        favoriteButton.onClick.AddListener(ToggleFavoriteButton);
    }

    private void SelectBuild()
    {
        Debug.Log("enter build mode overlay");
        BuildMenuUI.Instance.Hide();

        PlayerController.LocalInstance.EnterBuildMode(buildObjectSO);
    }

    private void ToggleFavoriteButton()
    {
        isFavorited = !isFavorited;
        if (isFavorited) {
            BuildMenuUI.Instance.AddFavorite(buildObjectSO);
            favoriteButton.image.sprite = favoritedIcon;
        } else {
            BuildMenuUI.Instance.RemoveFavorite(buildObjectSO);
            favoriteButton.image.sprite = unfavoritedIcon;
        }
    }
}
