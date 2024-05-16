using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour {

    [Header("Components")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Animator hitmarkerAnimator;


    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float lookSensitivity;
    [SerializeField] private float lookAngleLimit;

    [Header("Network Variables")]
    [SerializeField] private NetworkVariable<int> health = new NetworkVariable<int>(5);

    private float cameraVerticalAngle = 0f;
    private bool inGame = false;
    private NetworkManagerUI networkManagerUI;
    private PlayerInfo myPlayerInfo;

    public override void OnNetworkSpawn()
    {
        networkManagerUI = GameObject.Find("NetworkManagerUI").GetComponent<NetworkManagerUI>();
        string username = networkManagerUI.username;
        int elo = networkManagerUI.elo;
        PlayerInfo playerInfo = new PlayerInfo(username,elo);
        //playerInfo.InitializePlayer();
        //myPlayerInfo = playerInfo;
        
        networkManagerUI.AddPlayerToLobby(playerInfo);
        Debug.Log("Hello world I spawned, my id is: " + NetworkObjectId);
        base.OnNetworkSpawn();
    }

    private void Start()
    {
        if (!IsLocalPlayer || !inGame) return;

        networkManagerUI = GameObject.Find("NetworkManagerUI").GetComponent<NetworkManagerUI>();
        playerCamera.SetActive(true);
        hitmarkerAnimator = GameObject.Find("Hitmarker").GetComponent<Animator>();
        GameObject.Find("MyId").GetComponent<TextMeshProUGUI>().text = $"{NetworkObjectId}";
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void StartGame()
    {
        if(!IsLocalPlayer) return;

        playerCamera.SetActive(true);
        hitmarkerAnimator = GameObject.Find("Hitmarker").GetComponent<Animator>();
        GameObject.Find("MyId").GetComponent<TextMeshProUGUI>().text = $"{NetworkObjectId}";
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inGame = true;
    }

    private void Update()
    {
        if (!IsOwner ||!inGame) return;

        HandleInput();
        Move();
        MoveCamera();
    }
    
    private void Move()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");

        Vector3 movementDirection =
            (transform.forward * vertical + transform.right * horizontal).normalized * (speed * Time.deltaTime);

        transform.position += movementDirection;
    }

    private void MoveCamera() {
        var mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        var mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;
        
        // Rotate the player on the X axis
        transform.Rotate(Vector3.up, mouseX);
        
        // Rotate the camera on the Y axis
        cameraVerticalAngle -= mouseY;
        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -lookAngleLimit, lookAngleLimit);
        playerCamera.transform.localEulerAngles = new Vector3(cameraVerticalAngle, 0f, 0f);
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ShootRpc(playerCamera.transform.position, playerCamera.transform.forward, NetworkObjectId);
        }
    }

    // Shooting Mechanic

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void ShootRpc(Vector3 startPos, Vector3 dir, ulong shooterId)
    {
        RaycastHit hit;
        Ray ray = new Ray(startPos, dir);

        if (Physics.Raycast(ray, out hit))
        {
            if (!hit.transform.CompareTag("Player")) return;

            TriggerHitmarkerRpc(shooterId);   
            GotShootRpc(hit.transform.gameObject.GetComponent<PlayerNetwork>().NetworkObjectId);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void TriggerHitmarkerRpc(ulong receiverId)
    {
        if (!IsOwner) return;
        if (NetworkObjectId == receiverId)
        {
            hitmarkerAnimator.ResetTrigger("Hit");
            hitmarkerAnimator.SetTrigger("Hit");
        }
        
    }

    [Rpc(SendTo.Everyone)]
    private void GotShootRpc(ulong receiverId)
    {
        if (!IsOwner) return;
        if(NetworkObjectId == receiverId)
        {
            Debug.Log("Got hit");
        }
    }
}