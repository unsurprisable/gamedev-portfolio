using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnlineChatUI : Menu
{
    public static OnlineChatUI Instance { get; private set; }

    [SerializeField] private Transform messageContainer;
    [SerializeField] private Transform messagePrefab;
    [SerializeField] private TMP_InputField inputField;

    private bool isInOfflineMode;

    private void Awake()
    {
        if (GameLobby.Instance == null || GameLobby.Instance.currentLobby == null) {
            isInOfflineMode = true;
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnChatMessage += SteamMatchmaking_OnChatMessage;

        GameInput.Instance.OnEnterChat += (sender, context) => {
            if (isEnabled) return;
            Show(false);
        };
        inputField.onSubmit.AddListener((call) => {
            if (inputField.text == string.Empty) return;
            string message = SteamClient.Name + ": " + inputField.text.Trim();
            GameLobby.Instance.currentLobby?.SendChatString(message);
            Hide(false);
        });
    }

    private void OnDisable()
    {
        if (isInOfflineMode) return;
        SteamMatchmaking.OnLobbyMemberJoined -= SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnChatMessage -= SteamMatchmaking_OnChatMessage;
    }

    private void SteamMatchmaking_OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        AddChatMessage(friend.Name + " joined the lobby");
    }

    private void SteamMatchmaking_OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        AddChatMessage(friend.Name + " left the lobby");
    }

    private void SteamMatchmaking_OnChatMessage(Lobby lobby, Friend friend, string message)
    {
        AddChatMessage(message);
    }

    public void AddChatMessage(string message)
    {
        Transform messageObj = Instantiate(messagePrefab, messageContainer);
        messageObj.GetComponent<TextMeshProUGUI>().text = message;
    }


    protected override void OnShow()
    {
        GameInput.Instance.SetPlayerInputActive(false);
        inputField.text = "";
        inputField.Select();
    }

    protected override void OnHide()
    {
        EventSystem.current.SetSelectedGameObject(null);
        GameInput.Instance.SetPlayerInputActive(true);
    }
}
