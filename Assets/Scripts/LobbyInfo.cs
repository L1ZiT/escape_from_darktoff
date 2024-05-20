using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInfo : MonoBehaviour
{
    public string lobbyId;
    public int playerCount;
    public string lobbyName;

    public TextMeshProUGUI lobbyNameText;
    public TextMeshProUGUI playerCountText;
    public Button JoinBtn;

    public void Awake()
    {
        JoinBtn.onClick.AddListener(() =>
        {
            GameObject.Find("LobbyManager").GetComponent<LobbyManager>().JoinLobby(lobbyId);
        });
    }
}
