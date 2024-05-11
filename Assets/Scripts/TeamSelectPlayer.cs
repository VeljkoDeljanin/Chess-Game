using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectPlayer : MonoBehaviour {
    [SerializeField] private int playerIndex;
    [SerializeField] private PlayerVisual[] playerVisuals;
    [SerializeField] private Button kickButton;
    [SerializeField] private TextMeshPro playerNameText;

    private void Awake() {
        kickButton.onClick.AddListener(() => {
            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            GameLobby.Instance.KickPlayer(playerData.playerId.ToString());
            GameMultiplayer.Instance.KickPlayer(playerData.clientId);
        });
    }

    private void Start() {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;

        kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer && playerIndex != 0);

        UpdatePlayer();
    }

    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e) {
        UpdatePlayer();
    }

    private void UpdatePlayer() {
        if (GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex)) {
            Show();

            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            
            playerNameText.text = playerData.playerName.ToString();

            foreach (var playerVisual in playerVisuals)
                playerVisual.SetPlayerColor(GameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
        }
        else {
            Hide();
        }
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}
