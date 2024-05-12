using UnityEngine;

public class WaitingForOpponentUI : MonoBehaviour {

    private void Start() {
        GameManager.Instance.OnLocalPlayerReadyChanged += GameManager_OnLocalPlayerReadyChanged;
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        Hide();
    }

    private void GameManager_OnLocalPlayerReadyChanged(object sender, System.EventArgs e) {
        if (GameManager.Instance.IsLocalPlayerReady()) {
            Show();
        }
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e) {
        if (GameManager.Instance.IsCountdownToStartActive()) {
            Hide();
        }
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}