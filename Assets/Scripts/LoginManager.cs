using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class LoginManager : MonoBehaviour
{

    public Button loginButton;
    public Button registerButton;
    public Button goToRegister;
    public Button goToLogin;
    public Button registerQuitBtn;
    public Button quitBtn;
    public Button enterAsGuestBtn;

    public TMP_InputField registerUsernameText;
    public TMP_InputField registerEmailText;
    public TMP_InputField registerPassword;

    public TMP_InputField loginUsernameText;
    public TMP_InputField loginPasswordText;

    private NetworkManagerUI networkManagerUI;
    private UserData userData;

    private string loginUrl = "https://catbattle.duckdns.org/odoo-api/common/login";
    private string dbName = "";

    private void Awake()
    {
        networkManagerUI = GameObject.Find("NetworkManagerUI").GetComponent<NetworkManagerUI>();
        userData = GameObject.Find("UserData").GetComponent<UserData>();

        loginButton.onClick.AddListener(() =>
        {
            string username = loginUsernameText.text;
            string password = loginPasswordText.text;
            StartCoroutine(LoginToOdoo(username, password));
        });

        registerButton.onClick.AddListener(() =>
        {
            string username = registerUsernameText.text;
            string password = registerPassword.text;
            string email = registerEmailText.text;
            StartCoroutine(RegisterToOdoo(username, email, password));
        });

        goToLogin.onClick.AddListener(() =>
        {
            networkManagerUI.ChangeToLogin();
        });

        goToRegister.onClick.AddListener(() =>
        {
            networkManagerUI.ChangeToRegister();
        });

        registerQuitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        quitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        enterAsGuestBtn.onClick.AddListener(() =>
        {
            EnterAsGuest();
        });
    }

    private IEnumerator LoginToOdoo(string username, string password)
    {
        LoginParams loginParams = new LoginParams();
        ValsParams vals = new ValsParams();
        vals.login = username;
        vals.password = password;
        loginParams.vals = vals;

        string jsonData = JsonUtility.ToJson(loginParams);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(loginUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            //feedbackText.text = "Login Failed: " + request.error;
        } 
        else
        {
            Debug.Log(request.downloadHandler.text);
            networkManagerUI.LoginSuccess();
        }
    }

    private IEnumerator RegisterToOdoo(string username, string email, string password)
    {
        RegisterParams registerParams = new RegisterParams();
        RegisterValsParams vals = new RegisterValsParams();
        vals.login = username;
        vals.email = email;
        vals.password = password;
        registerParams.vals = vals;

        string jsonData = JsonUtility.ToJson(registerParams);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(loginUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            //feedbackText.text = "Login Failed: " + request.error;
        } else
        {
            Debug.Log(request.downloadHandler.text);
            networkManagerUI.RegisterSuccess();
        }
    }

    [System.Serializable]
    public class LoginParams
    {
        public string model = "res.users";
        public ValsParams vals;
        public string db = "CatsBattles";
        public string login = "admin";
        public string password = "Almi123";
    }

    [System.Serializable]
    public class ValsParams
    {
        public string login;
        public string password;
    }

    [System.Serializable]
    public class RegisterParams
    {
        public string model = "res.users";
        public RegisterValsParams vals;
        public string db = "CatsBattles";
        public string login = "admin";
        public string password = "Almi123";
    }

    [System.Serializable]
    public class RegisterValsParams
    {
        public string login;
        public string email;
        public string password;
    }

    private void EnterAsGuest()
    {
        userData.username = "Guest" + Random.Range(0, 9999);
        userData.rank = "Bronze";
        userData.elo = 0;
        networkManagerUI.LoginSuccess();
    }

}