using TMPro;
using UnityEngine;

public class GamePlayingTimerUI : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI MyGamePlayingTimerText;
    [SerializeField] private TextMeshProUGUI OpponentGamePlayingTimerText;

    private void Start() {
        if (GameMultiplayer.Instance.GetGamePlayingTimerId() != 0) {
            Show();
        } else {
            Hide();
        }
    }

    private void Update() {
        if (GameMultiplayer.playMultiplayer) {
            UpdateTimers();
        }
    }

    private void UpdateTimers() {
        int myGamePlayingTimerNumber, opponentGamePlayingTimerNumber;
        if (GameMultiplayer.Instance.GetPlayerData().colorId == 0) {
            myGamePlayingTimerNumber = Mathf.CeilToInt(GameManager.Instance.GetWhitePlayerGamePlayingTimer());
            opponentGamePlayingTimerNumber = Mathf.CeilToInt(GameManager.Instance.GetBlackPlayerGamePlayingTimer());
        } else {
            myGamePlayingTimerNumber = Mathf.CeilToInt(GameManager.Instance.GetBlackPlayerGamePlayingTimer());
            opponentGamePlayingTimerNumber = Mathf.CeilToInt(GameManager.Instance.GetWhitePlayerGamePlayingTimer());
        }

        MyGamePlayingTimerText.text = GetFormatedGamePlayingTimerString(myGamePlayingTimerNumber);
        OpponentGamePlayingTimerText.text = GetFormatedGamePlayingTimerString(opponentGamePlayingTimerNumber);
    }

    private string GetFormatedGamePlayingTimerString(int gamePlayingTimerNumber) {
        string timerMinutes = (gamePlayingTimerNumber / 60).ToString();
        if (timerMinutes.Length == 1) {
            timerMinutes = "0" + timerMinutes;
        }

        string timerSeconds = (gamePlayingTimerNumber % 60).ToString();
        if (timerSeconds.Length == 1) {
            timerSeconds = "0" + timerSeconds;
        }

        return timerMinutes + ":" + timerSeconds;
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
