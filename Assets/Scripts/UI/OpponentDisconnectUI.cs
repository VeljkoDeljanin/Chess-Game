using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OpponentDisconnectUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    
    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        Hide();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        Show();
    }

    private void Show()
    {
        Chessboard.opponentDisconnectUIActive = true;
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        Chessboard.opponentDisconnectUIActive = false;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }
}
