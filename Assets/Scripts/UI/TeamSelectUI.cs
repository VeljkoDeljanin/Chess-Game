using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectUI : MonoBehaviour {
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    private void Awake() {
        mainMenuButton.onClick.AddListener(() => {
            GameLobby.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        startGameButton.GetComponent<Button>().onClick.AddListener(() => {
            GameLobby.Instance.DeleteLobby();
            Loader.LoadNetwork(Loader.Scene.GameScene);
        });
    }

    private void Start() {
        Lobby lobby = GameLobby.Instance.GetLobby();

        lobbyNameText.text = "Lobby Name: " + lobby.Name;
        lobbyCodeText.text = "Lobby Code: " + lobby.LobbyCode;

        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;

        startGameButton.SetActive(NetworkManager.Singleton.IsServer);
        startGameButton.GetComponent<Button>().interactable = false;
    }

    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e) {
        if (GameMultiplayer.Instance.playerDataNetworkList.Count == 2) {
            startGameButton.GetComponent<Button>().interactable = true;
        } else {
            startGameButton.GetComponent<Button>().interactable = false;
        }
    }

    private void OnDestroy() {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;

    }
}
