using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenuUI : Menu
{   
    public const string PLAYER_PREFS_FAVORITE_BUILDS = "FavoriteBuildIds";

    public static BuildMenuUI Instance { get; private set; }

    public enum BuildCategory { ITEM_DISPLAYS, CUSTOMERS, DECORATION, FAVORITES }

    [SerializeField] private BuildObjectListSO objectList;
    [SerializeField] private GameObject[] categoryButtons;
    [SerializeField] private Transform buildButtonTemplate;
    [SerializeField] private Transform buildButtonParent;

    private List<BuildObjectSO> favorites;
    private bool favoritesModified;

    private HashSet<GameObject> activeBuildButtons;
    private Dictionary<BuildCategory, List<BuildObjectSO>> categoryDict;
    private Dictionary<BuildObjectSO, int> buildObjectIdDictionary; // quick map from a buildObjectSO to its ID (index in objectList)



    private void Awake()
    {
        Instance = this;

        activeBuildButtons = new HashSet<GameObject>();
        categoryDict = new Dictionary<BuildCategory, List<BuildObjectSO>>();
        buildObjectIdDictionary = new Dictionary<BuildObjectSO, int>();

        for (int i = 0; i < objectList.list.Length; i++)
        {
            BuildObjectSO buildObjectSO = objectList.list[i];

            buildObjectIdDictionary.Add(buildObjectSO, i);

            if (!categoryDict.ContainsKey(buildObjectSO.category)) {
                categoryDict[buildObjectSO.category] = new List<BuildObjectSO>();
            }
            categoryDict[buildObjectSO.category].Add(buildObjectSO);
        }

        // get a list of the player's favorited items from playerprefs
        Debug.Log("Retrieving favorites from PlayerPrefs...");
        string unparsedBuildIds = PlayerPrefs.GetString(PLAYER_PREFS_FAVORITE_BUILDS, "");
        if (unparsedBuildIds.Equals("")) {
            favorites = new List<BuildObjectSO>();
        } else {
            favorites = unparsedBuildIds.Split(' ').Select(n => GetBuildObjectSOFromId(Convert.ToInt32(n))).ToList();
        }
        categoryDict[BuildCategory.FAVORITES] = favorites;

        // manual assignment for now (it's not too inconvenient, so this will probably stay)
        categoryButtons[0].GetComponent<Button>().onClick.AddListener(()=>{ ShowCategory(BuildCategory.ITEM_DISPLAYS) ;});
        categoryButtons[1].GetComponent<Button>().onClick.AddListener(()=>{ ShowCategory(BuildCategory.CUSTOMERS); });
        categoryButtons[2].GetComponent<Button>().onClick.AddListener(()=>{ ShowCategory(BuildCategory.DECORATION); });
        categoryButtons[3].GetComponent<Button>().onClick.AddListener(()=>{ ShowCategory(BuildCategory.FAVORITES); });
    }

    private void Start()
    {
        GameInput.Instance.OnBuildMenu += (sender, args) => {
            if (isEnabled) 
                Hide();
            else if (!MenuManager.Instance.IsInMenu()) 
                Show();
        };
    }

    public new void Hide(bool changeMouseState = true) {
        // TECHNICALLY writing to PlayerPrefs should be done less frequently (but I don't see any immediate issues with doing it every time the menu is closed)
        // save favorites to playerprefs
        if (favoritesModified) {
            StringBuilder idString = new StringBuilder("");
            for (int i = 0; i < favorites.Count; i++) {
                if (i != 0) {
                    idString.Append(" ");
                }
                idString.Append(GetBuildObjectSOId(favorites[i]));
            }
            Debug.Log("Writing favorites to PlayerPrefs...");
            PlayerPrefs.SetString(PLAYER_PREFS_FAVORITE_BUILDS, idString.ToString());
            favoritesModified = false;
        }

        base.Hide(changeMouseState);
    }

    private void ShowCategory(BuildCategory category) 
    {
        if (activeBuildButtons.Count != 0) {
            foreach (GameObject buildButton in activeBuildButtons) {
                Destroy(buildButton);
            }
            activeBuildButtons.Clear();
        }

        if (!categoryDict.ContainsKey(category)) {
            Debug.LogWarning("That category does not contain any items!");
            return;
        }

        List<BuildObjectSO> buildObjects = categoryDict[category];
        foreach (BuildObjectSO buildObjectSO in buildObjects) {
            activeBuildButtons.Add(CreateBuildButton(buildObjectSO));
        }
    }

    private GameObject CreateBuildButton(BuildObjectSO buildObjectSO)
    {
        Transform buildButton = Instantiate(buildButtonTemplate, buildButtonParent);
        buildButton.GetComponent<BuildButtonSingleUI>().SetBuildObjectSO(buildObjectSO);
        return buildButton.gameObject;
    }

    private int GetBuildObjectSOId(BuildObjectSO buildObjectSO) {
        return buildObjectIdDictionary[buildObjectSO];
    }

    private BuildObjectSO GetBuildObjectSOFromId(int id) {
        return objectList.list[id];
    }

    public bool IsBuildObjectSOFavorited(BuildObjectSO buildObjectSO) {
        return favorites.Contains(buildObjectSO);
    }

    public void AddFavorite(BuildObjectSO buildObjectSO) {
        if (favorites.Contains(buildObjectSO)) {
            Debug.LogError("That object is already favorited!");
            return;
        }
        favorites.Add(buildObjectSO);
        favoritesModified = true;
    }
    
    public void RemoveFavorite(BuildObjectSO buildObjectSO) {
        if (!favorites.Contains(buildObjectSO)) {
            Debug.LogError("That object is not favorited!");
            return;
        }
        favorites.Remove(buildObjectSO);
        favoritesModified = true;
    }
}
