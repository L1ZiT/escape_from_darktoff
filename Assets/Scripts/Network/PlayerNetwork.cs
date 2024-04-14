using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour {
    
    [SerializeField] Camera playerCamera ;
    
    public override void OnNetworkSpawn() {
        Debug.Log("Hello world I spawned, my id is: " + NetworkObjectId);
        base.OnNetworkSpawn();
    }

    private void Update() {
        if (!IsOwner) {
            return;
        }
        
        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void Start() {
        if (!IsLocalPlayer) {
            playerCamera.gameObject.SetActive(false);
        }
    }
}
