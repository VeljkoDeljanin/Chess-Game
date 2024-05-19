using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour {

    private const string PLAYER_PREFS_FULLSCREEN_VALUE = "FullscreenValue";

    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider soundEffectsSlider;
    [SerializeField] private TextMeshProUGUI soundEffectsText;
    [SerializeField] private Button closeButton;

    private int fullscreenValue = 1;

    private void Awake() {
        fullscreenToggle.onValueChanged.AddListener((bool isFullscreen) => {
            ChangeFullscreenValue(isFullscreen);
        });
        soundEffectsSlider.onValueChanged.AddListener((float volume) => {
            SoundManager.ChangeVolume(volume);
            UpdateVisual();
        });
        closeButton.onClick.AddListener(() => {
            Hide();
        });

        fullscreenValue = PlayerPrefs.GetInt(PLAYER_PREFS_FULLSCREEN_VALUE, 1);
    }

    private void Start() {
        SoundManager.InitVolume(PlayerPrefs.GetFloat(SoundManager.PLAYER_PREFS_SOUND_EFFECT_VOLUME, 1f));

        fullscreenToggle.isOn = (fullscreenValue == 1);
        soundEffectsSlider.value = SoundManager.GetVolume() * 10f;
        Hide();
    }

    private void UpdateVisual() {
        soundEffectsText.text = "Sound Effects: " + Mathf.Round(SoundManager.GetVolume() * 10f);
    }

    private void ChangeFullscreenValue(bool isFullscreen) {
        if (isFullscreen) {
            Screen.SetResolution(1920, 1080, true);
        } else {
            Screen.SetResolution(1280, 720, false);
        }

        PlayerPrefs.SetInt(PLAYER_PREFS_FULLSCREEN_VALUE, fullscreenValue);
        PlayerPrefs.Save();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
