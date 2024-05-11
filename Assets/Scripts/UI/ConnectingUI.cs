using System;
using UnityEngine;

public class ConnectingUI : MonoBehaviour {

    private void Start() {
        GameMultiplayer.Instance.OnTryingToJoinGame += GameMultiplayer_OnTryingToJoinGame;
        GameMultiplayer.Instance.OnFailedToJoinGame += GameMultiplayer_OnFailedToJoinGame;

        Hide();
    }

    private void GameMultiplayer_OnTryingToJoinGame(object sender, EventArgs e) {
        Show();
    }

    private void GameMultiplayer_OnFailedToJoinGame(object sender, EventArgs e) {
        Hide();
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        GameMultiplayer.Instance.OnTryingToJoinGame -= GameMultiplayer_OnTryingToJoinGame;
        GameMultiplayer.Instance.OnFailedToJoinGame -= GameMultiplayer_OnFailedToJoinGame;
    }
}
