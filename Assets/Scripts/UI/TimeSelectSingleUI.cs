using UnityEngine;
using UnityEngine.UI;

public class TeamSelectSingleUI : MonoBehaviour {

    [SerializeField] private int gamePlayingTimerId;
    [SerializeField] private GameObject selectedGameObject;

    private void Awake() {
        GetComponent<Button>().onClick.AddListener(() => {
            GameMultiplayer.Instance.ChangeGamePlayingTime(gamePlayingTimerId);
        });
    }

    private void Start() {
        GameMultiplayer.Instance.OnGamePlayingTimerChanged += GameMultiplayer_OnGamePlayingTimerChanged;

        UpdateIsSelected();
    }

    private void GameMultiplayer_OnGamePlayingTimerChanged(object sender, System.EventArgs e) {
        UpdateIsSelected();
    }

    private void UpdateIsSelected() {
        if (GameMultiplayer.Instance.GetGamePlayingTimerId() == gamePlayingTimerId) {
            selectedGameObject.SetActive(true);
        } else {
            selectedGameObject.SetActive(false);
        }
    }

    private void OnDestroy() {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnGamePlayingTimerChanged;
    }
}
