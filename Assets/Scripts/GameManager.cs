using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject[] teamCameras;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        ChangeCamera(GameMultiplayer.Instance.GetPlayerData().colorId);
    }

    private void ChangeCamera(int cameraIndex) {
        foreach (GameObject camera in teamCameras)
            camera.SetActive(false);

        teamCameras[cameraIndex].SetActive(true);
    }
}
