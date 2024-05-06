using System;
using Unity.Netcode;
using UnityEngine;

public class TeamPromotion : NetworkBehaviour {
    public static TeamPromotion Instance { get; private set; }

    public event EventHandler<OnTeamPromotionEventArgs> OnTeamPromotion;
    public class OnTeamPromotionEventArgs : EventArgs {
        public int colorId;
    }

    private void Awake() {
        Instance = this;
    }

    public void CheckForPromotion() {
        Tuple<Vector2Int, Vector2Int> lastMove = Chessboard.lastMove;
        if (PieceManager.Instance.pieces[lastMove.Item2.x, lastMove.Item2.y].type == PieceType.Pawn) {
            if (lastMove.Item2.y == 7 || lastMove.Item2.y == 0) {
                int colorId = (int)PieceManager.Instance.pieces[lastMove.Item2.x, lastMove.Item2.y].team - 1;
                if (colorId == GameMultiplayer.Instance.GetPlayerData().colorId || !GameMultiplayer.playMultiplayer) {
                    SetPromotionUIActive(true);
                    OnTeamPromotion?.Invoke(this, new OnTeamPromotionEventArgs { colorId = colorId });
                }
            }
        }
    }

    public void SetPromotionUIActive(bool promotionUIActive) {
        SetPromotionUIActiveServerRpc(promotionUIActive);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPromotionUIActiveServerRpc(bool promotionUIActive) {
        SetPromotionUIActiveClientRpc(promotionUIActive);
    }

    [ClientRpc]
    public void SetPromotionUIActiveClientRpc(bool promotionUIActive) {
        Chessboard.promotionUIActive = promotionUIActive;
    }

    public void ProcessPromotion(PieceType promotionType) {
        ProcessPromotionServerRpc(promotionType);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ProcessPromotionServerRpc(PieceType promotionType) {
        ProcessPromotionClientRpc(promotionType);
    }

    [ClientRpc]
    private void ProcessPromotionClientRpc(PieceType promotionType) {
        Tuple<Vector2Int, Vector2Int> lastMove = Chessboard.lastMove;
        Destroy(PieceManager.Instance.pieces[lastMove.Item2.x, lastMove.Item2.y].gameObject);
        Piece newPiece = PieceManager.Instance.SpawnSinglePiece(promotionType, (lastMove.Item2.y == 7) ? TeamColor.White : TeamColor.Black);
        PieceManager.Instance.pieces[lastMove.Item2.x, lastMove.Item2.y] = newPiece;

        PieceManager.Instance.PositionSinglePiece(lastMove.Item2.x, lastMove.Item2.y, false);

        Chessboard.CheckForCheckmate((lastMove.Item2.y == 7) ? TeamColor.Black : TeamColor.White);
    }
}
