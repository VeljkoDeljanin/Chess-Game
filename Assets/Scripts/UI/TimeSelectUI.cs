using Unity.Netcode;
using UnityEngine;

public class TimeSelectUI : MonoBehaviour {

    private void Start() {
        gameObject.SetActive(NetworkManager.Singleton.IsServer);
    }
}
