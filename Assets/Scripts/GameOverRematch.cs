using System;
using System.Collections.Generic;
using Unity.Netcode;

public class GameOverRematch : NetworkBehaviour {
    public static GameOverRematch Instance { get; private set; }

    public event EventHandler<OnRematchChangedEventArgs> OnRematchChanged;
    public class OnRematchChangedEventArgs : EventArgs {
        public ulong clientId;
    }

    private Dictionary<ulong, bool> playerRematchDictionary;

    private void Awake() {
        Instance = this;

        playerRematchDictionary = new Dictionary<ulong, bool>();
    }

    public void SetPlayerWantsRematch() {
        SetPlayerWantsRematchServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerWantsRematchServerRpc(ServerRpcParams serverRpcParams = default) {
        SetPlayerWantsRematchClientRpc(serverRpcParams.Receive.SenderClientId);

        playerRematchDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsRematch = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (!playerRematchDictionary.ContainsKey(clientId) || !playerRematchDictionary[clientId]) {
                // This player does NOT want a rematch
                allClientsRematch = false;
                break;
            }
        }

        if (allClientsRematch) {
            Loader.LoadNetwork(Loader.Scene.GameScene);
        }
    }

    [ClientRpc]
    private void SetPlayerWantsRematchClientRpc(ulong clientId) {
        playerRematchDictionary[clientId] = true;

        OnRematchChanged?.Invoke(this, new OnRematchChangedEventArgs { clientId = clientId });
    }

    public bool PlayerWantsRematch(ulong clientId) {
        return playerRematchDictionary.ContainsKey(clientId) && playerRematchDictionary[clientId];
    }
}
