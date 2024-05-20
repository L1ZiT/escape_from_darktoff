using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using QFSW.QC;
using TMPro;
using System.Linq;
using System;

public class LobbyManager : MonoBehaviour
{
    public Button startGameBtn;

    private Lobby currentLobby;
    private string playerId;

    public GameObject lobbyPrefab;
    public Transform lobbyListContainer;
    public GameObject lobbyPlayerPrefab;
    public Transform lobbyContainer;

    public TextMeshProUGUI lobbyPlayerCount;
    private NetworkManagerUI networkManagerUI;

    private void Start()
    {
        InitializeUnityServices();
        networkManagerUI = GameObject.Find("NetworkManagerUI").GetComponent<NetworkManagerUI>();
        startGameBtn.onClick.AddListener(OnStartGameButtonClicked);
    }

    private async void InitializeUnityServices()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerId = AuthenticationService.Instance.PlayerId;
    }

    [Command]
    public async void CreateLobby(string lobbyName, int maxPlayers)
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "Username", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, networkManagerUI.userData.username) },
                        { "Elo", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, networkManagerUI.userData.elo.ToString()) }
                    }
                }
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            Debug.Log($"Lobby created with ID: {currentLobby.Id}");
            networkManagerUI.EnterLobby();
            startGameBtn.gameObject.SetActive(true);
            lobbyPlayerCount.text = $"{currentLobby.Players.Count}/6";
            CheckLobbyData();
            InvokeRepeating(nameof(CheckLobbyData), 2.0f, 2.0f);
            InvokeRepeating(nameof(SendHeartbeat), 15.0f, 15.0f); // Heartbeat every 15 seconds
        } catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobby(string lobbyId)
    {
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "Username", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, networkManagerUI.userData.username) },
                        { "Elo", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, networkManagerUI.userData.elo.ToString()) }
                    }
                }
            };

            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            Debug.Log($"Joined lobby with ID: {currentLobby.Id}");
            networkManagerUI.EnterLobby();
            CheckLobbyData();
            InvokeRepeating(nameof(CheckLobbyData), 2.0f, 2.0f);
        } catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void ExitLobby()
    {
        if(currentLobby != null)
        {
            try
            {
                CancelInvoke(nameof(SendHeartbeat));
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, playerId);
                currentLobby = null;
                Debug.Log("Exited lobby");
                startGameBtn.gameObject.SetActive(false);
            } catch(LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Filters = new List<QueryFilter> {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);

            // Clear existing items
            foreach(Transform child in lobbyListContainer)
            {
                Destroy(child.gameObject);
            }

            // Add new items
            int lobbyCount = 0;
            foreach(var lobby in response.Results)
            {
                GameObject instantiatedlobbyPrefab = Instantiate(lobbyPrefab, lobbyListContainer);
                instantiatedlobbyPrefab.transform.position += new Vector3(0, -60 * lobbyCount, 0);
                LobbyInfo lobbyInfo = instantiatedlobbyPrefab.GetComponent<LobbyInfo>();
                lobbyInfo.lobbyId = lobby.Id;
                lobbyInfo.lobbyName = lobby.Name;
                lobbyInfo.playerCount = lobby.Players.Count;
                lobbyInfo.lobbyNameText.text = lobby.Name;
                lobbyInfo.playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
                lobbyCount++;
            }

        } catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void CheckLobbyData()
    {
        if(currentLobby != null)
        {
            try
            {
                var updatedLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
                if(updatedLobby.Data != null && updatedLobby.Data.ContainsKey("GameStarted") && updatedLobby.Data["GameStarted"].Value == "true")
                {
                    // Stop checking as we detected the game has started
                    CancelInvoke(nameof(CheckLobbyData));

                    if(updatedLobby.Data.ContainsKey("JoinCode"))
                    {
                        string joinCode = updatedLobby.Data["JoinCode"].Value;

                        // Join the Relay allocation using the join code
                        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                        // Set up the Relay client data
                        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                        transport.SetRelayServerData(
                            joinAllocation.RelayServer.IpV4,
                            (ushort)joinAllocation.RelayServer.Port,
                            joinAllocation.AllocationIdBytes,
                            joinAllocation.Key,
                            joinAllocation.ConnectionData,
                            joinAllocation.HostConnectionData
                        );

                        // Connect to the host
                        NetworkManager.Singleton.StartClient();
                        networkManagerUI.StartGame();
                    }
                }

                // Update the player list UI
                foreach(Transform child in lobbyContainer)
                {
                    Destroy(child.gameObject);
                }

                int playerCount = 0;

                foreach(Player player in updatedLobby.Players)
                {
                    GameObject instLobbyPlayer = Instantiate(lobbyPlayerPrefab, lobbyContainer);
                    instLobbyPlayer.GetComponent<PlayerInfo>().username = player.Data["Username"].Value;
                    instLobbyPlayer.GetComponent<PlayerInfo>().elo = player.Data["Elo"].Value;
                    instLobbyPlayer.GetComponent<PlayerInfo>().eloText.text = player.Data["Elo"].Value;
                    instLobbyPlayer.GetComponent<PlayerInfo>().usernameText.text = player.Data["Username"].Value;
                    instLobbyPlayer.transform.position += new Vector3(0, -60 * playerCount, 0);
                    playerCount++;
                }

                lobbyPlayerCount.text = $"{currentLobby.Players.Count}/6";
            } catch(LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
    }

    private async void OnStartGameButtonClicked()
    {
        if(currentLobby != null)
        {
            try
            {
                // Create a Relay allocation for the host
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4); // 4 is the max number of players
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                // Update lobby data to indicate game start and include the join code
                var lobbyData = new Dictionary<string, DataObject>
                {
                    { "GameStarted", new DataObject(DataObject.VisibilityOptions.Member, "true") },
                    { "JoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                };
                await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions { Data = lobbyData });

                // Set up the Relay server data for the host
                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );

                CancelInvoke(nameof(CheckLobbyData));
                CancelInvoke(nameof(SendHeartbeat));

                // Start the host
                NetworkManager.Singleton.StartHost();
                networkManagerUI.StartGame();
            } catch(RelayServiceException e)
            {
                Debug.LogError(e);
            }
        }
    }

    private async void CheckHostDisconnection()
    {
        if(currentLobby != null)
        {
            try
            {
                var updatedLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
                if(updatedLobby.HostId == null)
                {
                    Debug.Log("Host has disconnected, removing all players.");

                    foreach(var player in currentLobby.Players)
                    {
                        await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, player.Id);
                    }

                    currentLobby = null;
                }
            } catch(LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
    }

    private async void SendHeartbeat()
    {
        if (currentLobby != null)
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                Debug.Log("Heartbeat sent to keep the lobby alive.");
            } catch(LobbyServiceException e)
            {
                Debug.LogError(e);
            }
        }
    }
}
