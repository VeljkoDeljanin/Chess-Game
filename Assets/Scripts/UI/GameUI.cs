using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour{

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private GameObject resignButton;
    [SerializeField] private GameObject drawButton;
    [SerializeField] private GameObject drawText;

    private void Awake(){

        resignButton.GetComponent<Button>().onClick.AddListener(() => {
            GameManager.Instance.ProcessGameOver(GameMultiplayer.Instance.GetPlayerData().colorId == 0 ? TeamColor.Black : TeamColor.White);
        });
        mainMenuButton.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        drawButton.GetComponent<Button>().onClick.AddListener(() => {
            GameWantsDraw.Instance.SetPlayerWantsDraw();
        });

    }

    private void Start() {

        GameWantsDraw.Instance.OnWantsDrawChanged += GameWantsDraw_OnWantsDrawChanged;

        if (!GameMultiplayer.playMultiplayer) {
            resignButton.SetActive(false);
            drawButton.SetActive(false);
        }

        drawText.SetActive(false);
    }

    private void GameWantsDraw_OnWantsDrawChanged(object sender, GameWantsDraw.OnWantsDrawChangedEventArgs e) {
        if (!GameWantsDraw.Instance.PlayerWantsDraw(NetworkManager.Singleton.LocalClientId)) {
            drawText.SetActive(true);
        }
    }
}
