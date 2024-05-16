using Unity.Netcode;
using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using QFSW.QC;

public class LobbyManager : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float checkLobbyStatusTimer;
    public string username = "L1ZiT";
    private int elo;
    private List<Lobby> lobbyList;

    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button leaveLobbyBtn;
    [SerializeField] private Button joinLobbyBtn;

    [Header("Prefabs")]
    [SerializeField] private GameObject lobbyPrefab;
    [SerializeField] private GameObject lobbyPlayerPrefab;

    [Header("Scroll View Containers")]
    [SerializeField] private Transform lobbyListContentContainer;
    [SerializeField] private Transform playerListContentContainer;

    private void Awake()
    {
        createLobbyBtn.onClick.AddListener(() =>
        {
            CreateLobby(username);
        });
        leaveLobbyBtn.onClick.AddListener(LeaveLobby);
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        CheckLobbyStatus();
    }

    private async void HandleLobbyHeartbeat()
    {
        if(hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void CheckLobbyStatus()
    {
        if(joinedLobby != null)
        {
            checkLobbyStatusTimer -= Time.deltaTime;
            if(checkLobbyStatusTimer < 0f)
            {
                float checkLobbyStatusTimerMax = 5f;
                checkLobbyStatusTimer = checkLobbyStatusTimerMax;

                try
                {
                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    bool playerInLobby = false;

                    foreach(Player player in lobby.Players)
                    {
                        if(player.Id == AuthenticationService.Instance.PlayerId)
                        {
                            playerInLobby = true;
                            break;
                        }
                    }

                    if(!playerInLobby)
                    {
                        LobbyExited();
                    }
                } catch(LobbyServiceException ex)
                {
                    Debug.Log(ex);
                    LobbyExited();
                }
            }
        }
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    [Command]
    public async void CreateLobby(string userName)
    {
        try
        {
            string lobbyName = userName + "'s game";
            int maxPlayers = 6;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, userName) }
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;

            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers);
            PrintPlayers(hostLobby);

            // Start the game as host
            NetworkManager.Singleton.StartHost();
        } catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    [Command]
    public async void ListLobbies()
    {
        for(int i = 0; i < lobbyListContentContainer.childCount; i++)
        {
            Destroy(lobbyListContentContainer.GetChild(i).gameObject);
        }

        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            int lobbyCount = 0;
            foreach(Lobby lobby in queryResponse.Results)
            {
                GameObject lobbyInstance = Instantiate(lobbyPrefab, lobbyListContentContainer);
                lobbyInstance.transform.position += new Vector3(0, -60 * lobbyCount, 0);
                lobbyInstance.GetComponent<LobbyInfo>().InitializeLobby(lobby.Id, lobby.Name, lobby.Players.Count);
                lobbyCount++;

                // Add a button to join this lobby
                Button joinButton = lobbyInstance.GetComponentInChildren<Button>();
                joinButton.onClick.AddListener(() => JoinLobby(lobby.Id));
            }
        } catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    public async void JoinLobby(string lobbyId)
    {
        try
        {
            joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);

            Debug.Log("Joined Lobby! " + joinedLobby.Name);
            PrintPlayers(joinedLobby);

            // Connect as client
            NetworkManager.Singleton.StartClient();
        } catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        foreach(Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            if(joinedLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
                LobbyExited();
            } else if(hostLobby != null)
            {
                await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
                LobbyExited();
            }
        } catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    private void LobbyExited()
    {
        Debug.Log("You have exited the lobby.");
        joinedLobby = null;
        hostLobby = null;
        NetworkManager.Singleton.Shutdown();
        // Additional code to handle post-exit logic can be added here
    }

    [Command]
    private void ChangeUsername(string username, int elo)
    {
        this.username = username;
        this.elo = elo;
    }
}
