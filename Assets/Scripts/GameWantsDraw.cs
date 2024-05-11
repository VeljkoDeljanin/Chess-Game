using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameWantsDraw : NetworkBehaviour {

    public static GameWantsDraw Instance { get; private set; }
    
    public event EventHandler<OnWantsDrawChangedEventArgs> OnWantsDrawChanged;
    public class OnWantsDrawChangedEventArgs : EventArgs {
        public ulong clientId;
    }

    private Dictionary<ulong, bool> playerWantsDrawDictionary;

    private void Awake() {
        Instance = this;

        playerWantsDrawDictionary = new Dictionary<ulong, bool>();
    }

    public void SetPlayerWantsDraw() {
        SetPlayerWantsDrawServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerWantsDrawServerRpc(ServerRpcParams serverRpcParams = default) {
        SetPlayerWantsDrawClientRpc(serverRpcParams.Receive.SenderClientId);

        playerWantsDrawDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsWantDraw = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (!playerWantsDrawDictionary.ContainsKey(clientId) || !playerWantsDrawDictionary[clientId]) {
                // This player does NOT want a draw
                allClientsWantDraw = false;
                break;
            }
        }

        if (allClientsWantDraw) {
            GameManager.Instance.SetGameDraw();
        }
    }

    [ClientRpc]
    private void SetPlayerWantsDrawClientRpc(ulong clientId) {

        playerWantsDrawDictionary[clientId] = true;

        OnWantsDrawChanged?.Invoke(this, new OnWantsDrawChangedEventArgs { clientId = clientId });

    }

    public bool PlayerWantsDraw(ulong clientId) {
        return playerWantsDrawDictionary.ContainsKey(clientId) && playerWantsDrawDictionary[clientId];
    }

}
