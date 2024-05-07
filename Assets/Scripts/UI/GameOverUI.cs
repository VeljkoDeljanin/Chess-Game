using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour {
    [SerializeField] private Button rematchButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI resultText;

    private void Awake() {
        rematchButton.onClick.AddListener(() => {
            GameOverRematch.Instance.SetPlayerWantsRematch();
        });
        mainMenuButton.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }

    private void Start() {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        GameOverRematch.Instance.OnRematchChanged += GameRematch_OnRematchChanged;
        GameManager.Instance.OnGameOver += GameManager_OnGameOver;

        Hide();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId) {
        if (GameManager.Instance.gameOverUIActive) {
            rematchButton.interactable = false;
            messageText.text = "Opponent has left!";
            messageText.color = Color.red;
        }
    }

    private void GameRematch_OnRematchChanged(object sender, GameOverRematch.OnRematchChangedEventArgs e) {
        if (!GameOverRematch.Instance.PlayerWantsRematch(NetworkManager.Singleton.LocalClientId)) {
            messageText.text = "Opponent wants a rematch!";
            messageText.color = Color.green;
        }
    }

    private void GameManager_OnGameOver(object sender, GameManager.OnGameOverEventArgs e) {
        Show(e.resultText);
    }

    private void Show(string resultText) {
        gameObject.SetActive(true);
        this.resultText.text = resultText;
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }
}
