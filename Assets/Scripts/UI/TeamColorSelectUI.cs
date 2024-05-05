using Unity.Netcode;
using UnityEngine;

public class TeamColorSelectUI : MonoBehaviour {
    private void Start() {
        gameObject.SetActive(NetworkManager.Singleton.IsServer);
    }
}