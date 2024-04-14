using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour {
    
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private Button disconnectBtn;

    private void Awake() {
        serverBtn.onClick.AddListener((() => {
            NetworkManager.Singleton.StartServer();
        }));
        hostBtn.onClick.AddListener((() => {
            NetworkManager.Singleton.StartHost();
        }));
        clientBtn.onClick.AddListener((() => {
            NetworkManager.Singleton.StartClient();
        }));
        disconnectBtn.onClick.AddListener((() => {
            if (NetworkManager.Singleton.IsHost) {
                NetworkManager.Singleton.Shutdown();
            } else if (NetworkManager.Singleton.IsServer) {
                NetworkManager.Singleton.Shutdown();
            } else if (NetworkManager.Singleton.IsClient) {
                NetworkManager.Singleton.Shutdown();
            }
        }));
        
    }
}
