using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : Menu
{
    public static PauseMenuUI Instance { get; private set; }

    [SerializeField] private GameObject copyLobbyButton;
    [SerializeField] private GameObject inviteFriendsButton;

    private void Awake()
    {
        Instance = this;

        if (GameLobby.Instance != null && GameLobby.Instance.currentLobby != null) {
            copyLobbyButton.SetActive(true);
            inviteFriendsButton.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        Hide();
    }
    public void OpenSettings()
    {
        SettingsMenuUI.Instance.Show();
    }
    public void QuitToMenu()
    {
        if (SteamManager.Instance != null) {
            SteamManager.Instance.LeaveLobby();
        } else {
            Debug.LogWarning("SteamManager is not initialized! Perhaps you didn't load the MainMenu scene yet?");
        }
        SceneManager.LoadScene("MainMenu");
    }
    public void QuitGame()
    {
        if (SteamManager.Instance != null) {
            SteamManager.Instance.LeaveLobby();
        }
        Application.Quit();
    }
    public void CopyLobbyCode()
    {
        if (GameLobby.Instance == null || GameLobby.Instance.currentLobby == null) {
            Debug.LogError("This button should not be enabled when a lobby connection is not established.");
            return;
        }

        TextEditor textEditor = new TextEditor();
        textEditor.text = GameLobby.Instance.currentLobby?.Id.ToString();
        textEditor.SelectAll();
        textEditor.Copy();
    }
    public void OpenFriendInvites()
    {
        if (GameLobby.Instance == null || GameLobby.Instance.currentLobby == null) {
            Debug.LogError("This button should not be enabled when a lobby connection is not established.");
            return;
        }
        SteamFriends.OpenGameInviteOverlay((SteamId)(GameLobby.Instance.currentLobby?.Id));
    }
}
