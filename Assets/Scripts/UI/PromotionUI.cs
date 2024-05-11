using UnityEngine;
using UnityEngine.UI;

public class PromotionUI : MonoBehaviour {

    [SerializeField] private Button[] whiteTeamButtons;
    [SerializeField] private Button[] blackTeamButtons;
    [SerializeField] private GameObject[] teamPromotions;

    private void Awake() {
        AddButtonListeners(whiteTeamButtons);
        AddButtonListeners(blackTeamButtons);
    }

    private void Start() {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        Hide();
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e) {
        if (GameManager.Instance.IsPromotionActive() && (GameManager.Instance.GetPromotionColorId() == GameMultiplayer.Instance.GetPlayerData().colorId || !GameMultiplayer.playMultiplayer)) {
            Show(GameManager.Instance.GetPromotionColorId());
        } else {
            Hide();
        }
    }

    private void AddButtonListeners(Button[] buttons) {
        buttons[0].onClick.AddListener(() => {
            TeamPromotion.Instance.ProcessPromotion(PieceType.Queen);
        });
        buttons[1].onClick.AddListener(() => {
            TeamPromotion.Instance.ProcessPromotion(PieceType.Rook);
        });
        buttons[2].onClick.AddListener(() => {
            TeamPromotion.Instance.ProcessPromotion(PieceType.Knight);
        });
        buttons[3].onClick.AddListener(() => {
            TeamPromotion.Instance.ProcessPromotion(PieceType.Bishop);
        });
    }

    private void Show(int colorId) {
        gameObject.SetActive(true);
        teamPromotions[colorId].SetActive(true);
    }

    private void Hide() {
        foreach (GameObject promotion in teamPromotions)
            promotion.SetActive(false);
        
        gameObject.SetActive(false);
    }
}
