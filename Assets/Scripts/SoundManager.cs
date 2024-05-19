using Unity.Netcode;
using UnityEngine;

public class SoundManager : NetworkBehaviour {

    public static readonly string PLAYER_PREFS_SOUND_EFFECT_VOLUME = "SoundEffectsVolume";

    public static SoundManager Instance { get; private set; }

    private static float volume = 1f;

    [SerializeField] private AudioClip gameStart;
    [SerializeField] private AudioClip gameEnd;
    [SerializeField] private AudioClip moveSelf;
    [SerializeField] private AudioClip moveOpponent;
    [SerializeField] private AudioClip capture;
    [SerializeField] private AudioClip castle;
    [SerializeField] private AudioClip moveCheck;
    [SerializeField] private AudioClip tenSeconds;
    [SerializeField] private AudioClip countdown;
    [SerializeField] private AudioClip promote;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameManager.Instance.OnGameStartSound += GameManager_OnGameStartSound;
        PieceManager.Instance.OnGameEndSound += PieceManager_OnGameEndSound;
        TeamPromotion.Instance.OnGameEndSound += TeamPromotion_OnGameEndSound;
        PieceManager.Instance.OnMoveSelfSound += PieceManager_OnMoveSelfSound;
        PieceManager.Instance.OnMoveOpponentSound += PieceManager_OnMoveOpponentSound;
        PieceManager.Instance.OnCaptureSound += PieceManager_OnCaptureSound;
        PieceManager.Instance.OnCastleSound += PieceManager_OnCastleSound;
        PieceManager.Instance.OnMoveCheckSound += PieceManager_OnMoveCheckSound;
        TeamPromotion.Instance.OnMoveCheckSound += TeamPromotion_OnMoveCheckSound;
        GameManager.Instance.OnTenSecondsSound += GameManager_OnTenSecondsSound;
        PieceManager.Instance.OnPromotionSound += PieceManager_OnPromotionSound;
    }

    private void GameManager_OnGameStartSound(object sender, System.EventArgs e) {
        PlaySound(gameStart, Camera.main.transform.position);
    }

    private void PieceManager_OnGameEndSound(object sender, System.EventArgs e) {
        PlaySound(gameEnd, Camera.main.transform.position);
    }

    private void TeamPromotion_OnGameEndSound(object sender, System.EventArgs e) {
        PlaySound(gameEnd, Camera.main.transform.position);
    }

    private void PieceManager_OnMoveSelfSound(object sender, System.EventArgs e) {
        PlaySound(moveSelf, Camera.main.transform.position);
    }

    private void PieceManager_OnMoveOpponentSound(object sender, System.EventArgs e) {
        PlaySound(moveOpponent, Camera.main.transform.position);
    }

    private void PieceManager_OnCaptureSound(object sender, System.EventArgs e) {
        PlaySound(capture, Camera.main.transform.position);
    }

    private void PieceManager_OnCastleSound(object sender, System.EventArgs e) {
        PlaySound(castle, Camera.main.transform.position);
    }

    private void PieceManager_OnMoveCheckSound(object sender, System.EventArgs e) {
        PlaySound(moveCheck, Camera.main.transform.position);
    }

    private void TeamPromotion_OnMoveCheckSound(object sender, System.EventArgs e) {
        PlaySound(moveCheck, Camera.main.transform.position);
    }

    private void GameManager_OnTenSecondsSound(object sender, System.EventArgs e) {
        PlaySound(tenSeconds, Camera.main.transform.position);
    }

    private void PieceManager_OnPromotionSound(object sender, System.EventArgs e) {
        PlaySound(promote, Camera.main.transform.position);
    }

    public void PlayCountdownSound() {
        PlaySound(countdown, Camera.main.transform.position);
    }

    private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f) {
        AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
    }

    public static void InitVolume(float initVolume) {
        volume = initVolume;
    }

    public static void ChangeVolume(float newVolume) {
        volume = newVolume / 10f;

        PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECT_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public static float GetVolume() {
        return volume;
    }
}
