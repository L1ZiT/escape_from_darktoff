using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 3f;
    
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraPos;
    [SerializeField] private float lookSensitivity = 3f;
    [SerializeField] private float lookAngleLimit = 90f;
    private GameObject playerCamera;
    private float cameraVerticalAngle = 0f;
    
    private void Start() 
    {
        if (!IsLocalPlayer) { return; }

        playerCamera = GameObject.Find("PlayerCamera");
        playerCamera.transform.SetParent(cameraPos);
        playerCamera.transform.position = cameraPos.position;
    }
    
    private void Update() 
    {
        if (!IsOwner) { return; }
        
        HandleInput();
        Move();
        MoveCamera();
    }
    
    public override void OnNetworkSpawn()
    {
        Debug.Log("Hello world I spawned, my id is: " + NetworkObjectId);
        base.OnNetworkSpawn();
    }
    
    private void MoveCamera()
    {
        var mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        var mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;
        
        // Rotate the player on the X axis
        transform.Rotate(Vector3.up, mouseX);
        
        // Rotate the camera on the Y axis
        cameraVerticalAngle -= mouseY;
        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -lookAngleLimit, lookAngleLimit);
        playerCamera.transform.localEulerAngles = new Vector3(cameraVerticalAngle, 0f, 0f);
    }

    private void Move()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");
        
        Vector3 movementDirection = 
            (transform.forward * vertical + transform.right * horizontal).normalized * (movementSpeed * Time.deltaTime);


        transform.position += movementDirection;
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ShootServerRpc(NetworkObjectId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShootServerRpc(ulong playerId)
    {
        Debug.Log("Player " + playerId + " shoot");
    }
}