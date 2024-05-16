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

    public Button joinButton;

    public void InitializeLobby(string id, string name, int players)
    {
        lobbyName = name;
        lobbyId = id;
        playerCount = players;

        lobbyNameText.text = lobbyName;
        playerCountText.text = playerCount + "/6";
    }
}
