using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour {

    [Header("Components")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Animator hitmarkerAnimator;
    private Animator miAnimator;


    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float lookSensitivity;
    [SerializeField] private float lookAngleLimit;

    private float speedMultiplier = 0f;
    private bool isWalking;
    private bool isSprinting;

    public int health = 100;
    public int ammo = 100;
    private NetworkManagerUI networkManagerUI;

    private float cameraVerticalAngle = 0f;

    public override void OnNetworkSpawn()
    {
        Debug.Log("Hello world I spawned, my id is: " + NetworkObjectId);
        base.OnNetworkSpawn();
    }

    private void Start()
    {
        if (!IsLocalPlayer) return;

        networkManagerUI = GameObject.Find("NetworkManagerUI").GetComponent<NetworkManagerUI>();
        playerCamera.transform.Find("GunCamera").gameObject.SetActive(false);
        transform.Find("DummyGun").gameObject.SetActive(false);
        playerCamera.transform.Find("GunCamera").gameObject.SetActive(true);
        miAnimator = GetComponent<Animator>();
        hitmarkerAnimator = GameObject.Find("Hitmarker").GetComponent<Animator>();
        //GameObject.Find("MyId").GetComponent<TextMeshProUGUI>().text = $"{NetworkObjectId}";
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleInput();
        Move();
        MoveCamera();
    }
    
    private void Move()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");

        float sprintBoost = 1f;

        if (isSprinting)
        {
            sprintBoost = 1.7f;
        }

        Vector3 movementDirection =
            (transform.forward * vertical + transform.right * horizontal).normalized * (speed * sprintBoost * Time.deltaTime);

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
            ammo -= 1;
            ammoText.text = ammo + "";
            miAnimator.ResetTrigger("Shoot");
            miAnimator.SetTrigger("Shoot");
            ShootRpc(playerCamera.transform.position, playerCamera.transform.forward, NetworkObjectId);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            isSprinting = true;
            miAnimator.SetBool("Running",true);
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;
            miAnimator.SetBool("Running", false);
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
            health -= Random.Range(5, 12);
            healthText.text = health + "";
        }
    }
}