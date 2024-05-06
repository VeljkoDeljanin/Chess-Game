using UnityEngine;
using UnityEngine.UI;

public class TeamColorSelectSingleUI : MonoBehaviour {
    [SerializeField] private int colorId;
    [SerializeField] private GameObject selectedGameObject;

    private void Awake() {
        GetComponent<Button>().onClick.AddListener(() => {
            GameMultiplayer.Instance.ChangePlayerColor(colorId);
        });
    }

    private void Start() {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
        UpdateIsSelected();
    }

    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e) {
        UpdateIsSelected();
    }

    private void UpdateIsSelected() {
        if (GameMultiplayer.Instance.GetPlayerData().colorId == colorId)
            selectedGameObject.SetActive(true);
        else
            selectedGameObject.SetActive(false);
    }

    private void OnDestroy() {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
    }
}
