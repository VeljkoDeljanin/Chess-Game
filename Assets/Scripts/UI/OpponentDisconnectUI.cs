using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OpponentDisconnectUI : MonoBehaviour {

    [SerializeField] private Button mainMenuButton;
    
    private void Awake() {
        mainMenuButton.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }

    private void Start() {
        GameManager.Instance.OnOpponentDisconnect += GameManager_OnOpponentDisconnect;

        Hide();
    }

    private void GameManager_OnOpponentDisconnect(object sender, System.EventArgs e) {
        if (!GameManager.Instance.IsGameOverActive()) {
            Show();
        }
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
