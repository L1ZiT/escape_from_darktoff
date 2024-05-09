using System;
using Network;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour {

	[Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 3f;
    
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraPos;
    [SerializeField] private float lookSensitivity = 3f;
    [SerializeField] private float lookAngleLimit = 90f;
    private GameObject playerCamera;
    private float cameraVerticalAngle = 0f;

    [Header("UI Settings")]
    private HitmarkerController hitmarkerController;

	[SerializeField] private InputReader input;
	Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = (InputReader) ScriptableObject.CreateInstance("InputReader");
        input.Enable();

        if (IsLocalPlayer)
        {
            playerCamera = GameObject.Find("PlayerCamera");
            playerCamera.transform.SetParent(cameraPos);
            playerCamera.transform.position = cameraPos.position;
        }
    }

    // Network variables should be value objects
    public struct InputPayload : INetworkSerializable {
        public int tick;
        public Vector3 inputVector;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref inputVector);
        }
    }
    
    public struct StatePayload : INetworkSerializable {
        public int tick;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref angularVelocity);
        }
    }
    
    // Netcode general
    NetworkTimer timer;
    const float k_serverTickRate = 60f; // 60fps
    const int k_bufferSize = 1024;
    
    // Netcode client specific
    CircularBuffer<StatePayload> clientStateBuffer;
    CircularBuffer<InputPayload> clientInputBuffer;
    StatePayload lastServerState;
    StatePayload lastProcessedState;
    
    // Netcode server specific
    CircularBuffer<StatePayload> serverStateBuffer;
    Queue<InputPayload> serverInputQueue;

    [Header("Netcode")]
    [SerializeField] float reconciliationThreshold = 10f;

    void Awake() {
        timer = new NetworkTimer(k_serverTickRate);
        clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);
        
        serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        serverInputQueue = new Queue<InputPayload>();
    }
    
    void Update() {
        timer.Update(Time.deltaTime);
    }

	void FixedUpdate() {
		if (!IsOwner) return;

		while (timer.ShouldTick()) {
			HandleClientTick();
			HandleServerTick();
		}
	}

	void HandleServerTick() {
		var bufferIndex = -1;
		while (serverInputQueue.Count > 0) {
			InputPayload inputPayload = serverInputQueue.Dequeue();

			bufferIndex = inputPayload.tick % k_bufferSize;

			StatePayload statePayload = SimulateMovement(inputPayload);
			serverStateBuffer.Add(statePayload, bufferIndex);
		}

		if (bufferIndex == -1) return;
		SendToClientRpc(serverStateBuffer.Get(bufferIndex));
	}

	StatePayload SimulateMovement(InputPayload inputPayload) {
		Physics.simulationMode = SimulationMode.Script;

		Move(inputPayload.inputVector);
		Physics.Simulate(Time.fixedDeltaTime);
		Physics.simulationMode = SimulationMode.FixedUpdate;

		return new StatePayload() {
			tick = inputPayload.tick,
			position = transform.position,
			rotation = transform.rotation,
			velocity = rb.velocity,
			angularVelocity = rb.angularVelocity
		};
	}

	[ClientRpc]
	void SendToClientRpc(StatePayload statePayload) {
		if (!IsOwner) return;
		lastServerState = statePayload;
	}

	void HandleClientTick() {
		if (!IsClient) return;

		var currentTick = timer.CurrentTick;
		var bufferIndex = currentTick % k_bufferSize;

		InputPayload inputPayload = new InputPayload {
			tick = currentTick,
			inputVector = input.Move
		};

		clientInputBuffer.Add(inputPayload, bufferIndex);
		SendToServerRpc(inputPayload);

		StatePayload statePayload = ProcessMovement(inputPayload);
		clientStateBuffer.Add(statePayload, bufferIndex);

		HandleServerReconciliation();
	}

    bool ShouldReconcile()
    {
        bool isNewServerState = !lastServerState.Equals(default);
        bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default) 
                                                || !lastProcessedState.Equals(lastServerState);

        return isNewServerState && isLastStateUndefinedOrDifferent;
    }

    void HandleServerReconciliation()
    {
        if(!ShouldReconcile()) return;

        float positionError;
        int bufferIndex;
        StatePayload rewindState = default;

        bufferIndex = lastServerState.tick % k_bufferSize;
        if(bufferIndex - 1 < 0) return; // Not enough information to reconcile

        rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState; // Host RPCs execute immediately, so we can use the last server state
        positionError = Vector3.Distance(rewindState.position, clientStateBuffer.Get(bufferIndex).position);

        if (positionError > reconciliationThreshold)
        {
            ReconcileState(rewindState);
        }

        lastProcessedState = lastServerState;
    }

    void ReconcileState(StatePayload rewindState)
    {
        Debug.Log("State reconciliated");
        transform.position = rewindState.position;
        transform.rotation = rewindState.rotation;
        rb.velocity = rewindState.velocity;
        rb.angularVelocity = rewindState.angularVelocity;

        if (!rewindState.Equals(lastServerState)) return;

        clientStateBuffer.Add(rewindState, rewindState.tick);

        // Replay all inputs from the rewind state to the current state
        int tickToReplay = lastServerState.tick;

        while (tickToReplay < timer.CurrentTick)
        {
            int bufferIndex = tickToReplay % k_bufferSize;
            StatePayload statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex));
            clientStateBuffer.Add(statePayload, bufferIndex);
            tickToReplay++;
        }
    }

	[ServerRpc]
	void SendToServerRpc(InputPayload input) {
		serverInputQueue.Enqueue(input);
	}

	StatePayload ProcessMovement(InputPayload input) {
		Move(input.inputVector);

        return new StatePayload()
        {
            tick = input.tick,
            position = transform.position,
            rotation = transform.rotation,
            velocity = rb.velocity,
            angularVelocity = rb.angularVelocity
        };
	}

	void Move(Vector2 inputVector) {
        float horizontal = input.Move.x;
        float vertical = input.Move.y;

        Vector3 movementDirection = 
            (transform.forward * vertical + transform.right * horizontal).normalized * (movementSpeed * Time.deltaTime);

        transform.position += movementDirection;
    }
    
    public override void OnNetworkSpawn() {
        Debug.Log("Hello world I spawned, my id is: " + NetworkObjectId);
        base.OnNetworkSpawn();
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
}