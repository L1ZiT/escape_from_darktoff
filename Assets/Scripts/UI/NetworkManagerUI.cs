using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour {

    [Header("Testing Buttons")]
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private Button disconnectBtn;

    [Header("Main Buttons")]
    [SerializeField] private Button playBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button testBtn;
    [SerializeField] private Button quitBtn;

    [Header("Menu Stages")]
    [SerializeField] private GameObject mainStage;
    [SerializeField] private GameObject playStage;
    [SerializeField] private GameObject lobbyStage;
    [SerializeField] private GameObject settingsStage;

    [Header("Back Buttons")]
    [SerializeField] private Button testBackBtn;
    [SerializeField] private Button mainBackBtn;
    [SerializeField] private Button playBackBtn;

    [Header("Play Buttons")]
    [SerializeField] private Button createLobbyBtn;

    [Header("Lobby Buttons")]
    [SerializeField] private Button leaveBtn;

    [Header("Other")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject ingameUIElements;
    [SerializeField] private Camera mainCamera;

    private List<PlayerInfo> lobbyPlayers;
    private bool insideLobby = false;
    private int lobbyPlayerCount = 0;

    public TMP_InputField usernameText;
    public TMP_InputField eloText;
    public string username;
    public int elo;

    [SerializeField] private GameObject lobbyPlayerPrefab;
    [SerializeField] private Transform lobbyListContentContainer;

    private void Awake() {
        lobbyPlayers = new List<PlayerInfo>();
        username = usernameText.textComponent.text;
        elo = 400;

        // Testing Events
        /*serverBtn.onClick.AddListener((() => {
            //ingameUIElements.SetActive(true);
            mainMenu.SetActive(false);
            mainCamera.gameObject.SetActive(false);
            NetworkManager.Singleton.StartServer();
        }));*/
        hostBtn.onClick.AddListener((() => {
            //ingameUIElements.SetActive(true);
            //mainMenu.SetActive(false);
            //mainCamera.gameObject.SetActive(false);
            NetworkManager.Singleton.StartHost();
            playStage.SetActive(false);
            lobbyStage.SetActive(true);
            insideLobby = true;
        }));
        clientBtn.onClick.AddListener((() => {
            //ingameUIElements.SetActive(true);
            //mainMenu.SetActive(false);
            //mainCamera.gameObject.SetActive(false);
            NetworkManager.Singleton.StartClient();
            playStage.SetActive(false);
            lobbyStage.SetActive(true);
            insideLobby = true;
        }));
        /*disconnectBtn.onClick.AddListener((() => {
            if (NetworkManager.Singleton.IsHost) {
                NetworkManager.Singleton.Shutdown();
            } else if (NetworkManager.Singleton.IsServer) {
                NetworkManager.Singleton.Shutdown();
            } else if (NetworkManager.Singleton.IsClient) {
                NetworkManager.Singleton.Shutdown();
            }

            ingameUIElements.SetActive(false);
            mainMenu.SetActive(true);
            mainCamera.gameObject.SetActive(false);
        }));*/

        // Main menu events
        playBtn.onClick.AddListener((() => {
            mainStage.SetActive(false);
            playStage.SetActive(true);
        }));
        settingsBtn.onClick.AddListener((() => {
            mainStage.SetActive(false);
            settingsStage.SetActive(true);
        }));
        quitBtn.onClick.AddListener((() => {
            Application.Quit();
        }));
        
    }

    private void Update()
    {
        /*if (insideLobby)
        {
            int playerCount = 0;
            foreach (PlayerInfo player in lobbyPlayers)
            {
                GameObject lobbyPlayerInstace = Instantiate(lobbyPlayerPrefab, lobbyListContentContainer);
                lobbyPlayerInstace.transform.position += new Vector3(0, -50 * playerCount, 0);
                playerCount++;
            }
        }*/
    }

    public void AddPlayerToLobby(PlayerInfo player)
    {
        GameObject lobbyPlayerInstace = Instantiate(lobbyPlayerPrefab, lobbyListContentContainer);
        lobbyPlayerInstace.GetComponent<PlayerInfo>().username = player.username;
        lobbyPlayerInstace.GetComponent<PlayerInfo>().elo = player.elo;
        lobbyPlayerInstace.GetComponent<PlayerInfo>().InitializePlayer();
        lobbyPlayerInstace.transform.position += new Vector3(0, -60 * lobbyPlayerCount, 0);
        lobbyPlayers.Add(player);
        lobbyPlayerCount++;
    }
}
