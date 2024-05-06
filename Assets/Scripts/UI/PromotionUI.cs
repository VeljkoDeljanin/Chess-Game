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
        TeamPromotion.Instance.OnTeamPromotion += TeamPromotion_OnTeamPromotion;

        Hide();
    }

    private void AddButtonListeners(Button[] buttons) {
        buttons[0].onClick.AddListener(() => {
            TeamPromotion.Instance.ProcessPromotion(PieceType.Queen);
            TeamPromotion.Instance.SetPromotionUIActive(false);
            Hide();
        });
        buttons[1].onClick.AddListener(() => {
            TeamPromotion.Instance.ProcessPromotion(PieceType.Rook);
            TeamPromotion.Instance.SetPromotionUIActive(false);
            Hide();
        });
        buttons[2].onClick.AddListener(() => {
            TeamPromotion.Instance.ProcessPromotion(PieceType.Knight);
            TeamPromotion.Instance.SetPromotionUIActive(false);
            Hide();
        });
        buttons[3].onClick.AddListener(() => {
            TeamPromotion.Instance.ProcessPromotion(PieceType.Bishop);
            TeamPromotion.Instance.SetPromotionUIActive(false);
            Hide();
        });
    }

    private void TeamPromotion_OnTeamPromotion(object sender, TeamPromotion.OnTeamPromotionEventArgs e) {
        Show(e.colorId);
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
