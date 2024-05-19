using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {

    [SerializeField] private Button playSingleplayerButton;
    [SerializeField] private Button playMultiplayerButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject settingsGameObject;

    private void Awake() {
        playSingleplayerButton.onClick.AddListener(() => {
            GameMultiplayer.playMultiplayer = false;
            Loader.Load(Loader.Scene.LobbyScene);
        });
        playMultiplayerButton.onClick.AddListener(() => {
            GameMultiplayer.playMultiplayer = true;
            Loader.Load(Loader.Scene.LobbyScene);
        });
        settingsButton.onClick.AddListener(() => {
            settingsGameObject.SetActive(true);
        });
        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });
    }
}
