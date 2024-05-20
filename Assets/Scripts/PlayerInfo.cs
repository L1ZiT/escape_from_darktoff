using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI eloText;

    public string username;
    public string elo;

    public PlayerInfo(string username, string elo)
    {
        this.username = username;
        this.elo = elo;
    }

    public void InitializePlayer()
    {
        usernameText.text = username + "";
        eloText.text = elo + "";
    }
}
