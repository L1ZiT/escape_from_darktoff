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
    [SerializeField] private GameObject lobbyListStage;
    [SerializeField] private GameObject lobbyStage;
    [SerializeField] private GameObject settingsStage;
    [SerializeField] private GameObject loginStage;
    [SerializeField] private GameObject registerStage;

    [Header("Back Buttons")]
    [SerializeField] private Button testBackBtn;
    [SerializeField] private Button mainBackBtn;
    [SerializeField] private Button playBackBtn;

    [Header("Play Buttons")]
    [SerializeField] private Button createLobbyBtn;
    [SerializeField] private Button backBtn;
    [SerializeField] private Button refreshBtn;

    [Header("Lobby Buttons")]
    [SerializeField] private Button leaveBtn;

    [Header("Other")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject ingameUIElements;
    [SerializeField] private Camera mainCamera;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI ammoText;

    public UserData userData;
    private LobbyManager lobbyManager;

    private void Awake() {

        healthText = GameObject.Find("Health").GetComponent<TextMeshProUGUI>();
        ammoText = GameObject.Find("Ammo").GetComponent<TextMeshProUGUI>();
        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        userData = GameObject.Find("UserData").GetComponent<UserData>();

        // Testing Events
        /*serverBtn.onClick.AddListener((() => {
            //ingameUIElements.SetActive(true);
            mainMenu.SetActive(false);
            mainCamera.gameObject.SetActive(false);
            NetworkManager.Singleton.StartServer();
        }));*/
        //hostBtn.onClick.AddListener((() => {
            //ingameUIElements.SetActive(true);
            //mainMenu.SetActive(false);
            //mainCamera.gameObject.SetActive(false);
            //NetworkManager.Singleton.StartHost();
            //playStage.SetActive(false);
            //lobbyStage.SetActive(true);
        //}));
        //clientBtn.onClick.AddListener((() => {
            //ingameUIElements.SetActive(true);
            //mainMenu.SetActive(false);
            //mainCamera.gameObject.SetActive(false);
        //    NetworkManager.Singleton.StartClient();
        //   playStage.SetActive(false);
        //    lobbyStage.SetActive(true);
        //}));
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
            lobbyListStage.SetActive(true);
            lobbyManager.ListLobbies();
        }));
        settingsBtn.onClick.AddListener((() => {
            mainStage.SetActive(false);
            settingsStage.SetActive(true);
        }));
        quitBtn.onClick.AddListener((() => {
            Application.Quit();
        }));

        // Lobby List btn
        backBtn.onClick.AddListener((() =>
        {
            mainStage.SetActive(true);
            lobbyListStage.SetActive(false);
        }));
        refreshBtn.onClick.AddListener((() =>
        {
            lobbyManager.ListLobbies();
        }));
        createLobbyBtn.onClick.AddListener((() =>
        {
            lobbyManager.CreateLobby(userData.username + "'s game", 6);
        }));

        // Lobby btn
        leaveBtn.onClick.AddListener((() =>
        {
            lobbyStage.SetActive(false);
            lobbyListStage.SetActive(true);
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

    public void EnterLobby()
    {
        lobbyListStage.SetActive(false);
        lobbyStage.SetActive(true);
    }

    public void ChangeToLogin()
    {
        loginStage.SetActive(true);
        registerStage.SetActive(false);
    }

    public void ChangeToRegister()
    {
        registerStage.SetActive(true);
        loginStage.SetActive(false);
    }

    public void LoginSuccess()
    {
        loginStage.SetActive(false);
        mainStage.SetActive(true);
    }

    public void RegisterSuccess()
    {
        registerStage.SetActive(false);
        loginStage.SetActive(true);
    }

    public void StartGame()
    {
        ingameUIElements.SetActive(true);
        mainMenu.SetActive(false);
        mainCamera.gameObject.SetActive(false);
    }
}
