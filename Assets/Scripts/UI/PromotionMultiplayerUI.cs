using UnityEngine;

public class PromotionMultiplayerUI : MonoBehaviour {

    private void Start() {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e) {
        if (GameManager.Instance.IsPromotionActive() && GameManager.Instance.GetPromotionColorId() != GameMultiplayer.Instance.GetPlayerData().colorId && GameMultiplayer.playMultiplayer) {
            Show();
        } else {
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
