using System;
using Unity.Netcode;
using UnityEngine;

public class TeamPromotion : NetworkBehaviour {

    public static TeamPromotion Instance { get; private set; }

    public event EventHandler OnGameEndSound;
    public event EventHandler OnMoveCheckSound;

    private void Awake() {
        Instance = this;
    }

    public bool CheckForPromotion() {
        Tuple<Vector2Int, Vector2Int> lastMove = PieceManager.Instance.lastMove;
        if (PieceManager.Instance.pieces[lastMove.Item2.x, lastMove.Item2.y].type == PieceType.Pawn) {
            if (lastMove.Item2.y == 7 || lastMove.Item2.y == 0) {

                int colorId = (int)PieceManager.Instance.pieces[lastMove.Item2.x, lastMove.Item2.y].team - 1;
                if (colorId == GameMultiplayer.Instance.GetPlayerData().colorId || !GameMultiplayer.playMultiplayer) {
                    GameManager.Instance.SetTeamPromotion(colorId);
                    return true;
                }
            }
        }

        return false;
    }

    public void ProcessPromotion(PieceType promotionType) {
        ProcessPromotionServerRpc(promotionType);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ProcessPromotionServerRpc(PieceType promotionType) {
        ProcessPromotionClientRpc(promotionType);

        GameManager.Instance.TeamPromotionOver();
    }

    [ClientRpc]
    private void ProcessPromotionClientRpc(PieceType promotionType) {
        Tuple<Vector2Int, Vector2Int> lastMove = PieceManager.Instance.lastMove;
        Destroy(PieceManager.Instance.pieces[lastMove.Item2.x, lastMove.Item2.y].gameObject);
        Piece newPiece = PieceManager.Instance.SpawnSinglePiece(promotionType, (lastMove.Item2.y == 7) ? TeamColor.White : TeamColor.Black);
        PieceManager.Instance.pieces[lastMove.Item2.x, lastMove.Item2.y] = newPiece;

        PieceManager.Instance.PositionSinglePiece(lastMove.Item2.x, lastMove.Item2.y, false);

        int checkForCheckmateResult = GameManager.Instance.CheckForCheckmate((lastMove.Item2.y == 7) ? TeamColor.Black : TeamColor.White);
        if (checkForCheckmateResult == 1) {
            OnGameEndSound?.Invoke(this, EventArgs.Empty);
        } else if (checkForCheckmateResult == 2) {
            OnMoveCheckSound?.Invoke(this, EventArgs.Empty);
        }
    }
}
