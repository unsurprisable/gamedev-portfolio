using System;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance { get; private set; }

    public event EventHandler OnGameLobbyEntered;

    private bool isCreatingLobby;
    private bool currentLobbyIsPublic;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SteamMatchmaking.OnLobbyCreated += SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += SteamFriends_OnGameLobbyJoinRequested;
    }
    private void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated -= SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= SteamMatchmaking_OnLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= SteamFriends_OnGameLobbyJoinRequested;
    }

    private void SteamMatchmaking_OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            if (currentLobbyIsPublic) {
                lobby.SetPublic();
            } else {
                lobby.SetFriendsOnly();
            }
            lobby.SetJoinable(true);
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("ExperimentalScene", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Lobby creation failed...finished with result: " + result.ToString());
        }
        isCreatingLobby = false;
    }

    private void SteamMatchmaking_OnLobbyEntered(Lobby lobby)
    {
        GameLobby.Instance.currentLobby = lobby;
        Debug.Log("entered a Lobby with ID: " + lobby.Id);
        OnGameLobbyEntered?.Invoke(this, EventArgs.Empty);

        if (NetworkManager.Singleton.IsServer) return;
        NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();

    }

    private void SteamFriends_OnGameLobbyJoinRequested(Lobby lobby, SteamId id)
    {
        lobby.Join();
    }

    public void LeaveLobby()
    {
        GameLobby.Instance.currentLobby?.Leave();
        GameLobby.Instance.currentLobby = null;
        NetworkManager.Singleton.Shutdown();
    }

    public async void HostLobby(bool isPublic) 
    {
        if (isCreatingLobby) {
            Debug.LogWarning("Already attempting to create a lobby...");
            return;
        }

        isCreatingLobby = true;
        currentLobbyIsPublic = isPublic;
        await SteamMatchmaking.CreateLobbyAsync(16);
    }

    public async void JoinLobbyWithID(string id)
    {
        ulong ID;

        if (!ulong.TryParse(id, out ID)) {
            Debug.LogWarning("Invalid ID format!");
            return;
        }

        Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

        foreach (Lobby lobby in lobbies) {
            if (lobby.Id == ID) {
                await lobby.Join();
                return;
            }
        }
        Debug.LogWarning("No lobbies found with that ID!");
    }
}
