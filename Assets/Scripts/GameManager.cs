using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnGameOverEventArgs> OnGameOver;
    public class OnGameOverEventArgs : EventArgs {
        public string resultText;
    }

    [SerializeField] private GameObject[] teamCameras;

    public bool isWhiteTurn;
    public bool gameOverUIActive;

    private void Awake() {
        Instance = this;

        isWhiteTurn = true;
        gameOverUIActive = false;
    }

    private void Start() {
        ChangeCamera(GameMultiplayer.Instance.GetPlayerData().colorId);
    }

    private void ChangeCamera(int cameraIndex) {
        foreach (GameObject camera in teamCameras)
            camera.SetActive(false);

        teamCameras[cameraIndex].SetActive(true);
    }

    public void CheckForCheckmate(TeamColor team) {
        // Getting the king we are checking
        Piece ourKing = null;
        for (int i = 0; i < TileManager.TILE_COUNT; i++)
            for (int j = 0; j < TileManager.TILE_COUNT; j++)
                if (PieceManager.Instance.pieces[i, j] != null)
                    if (PieceManager.Instance.pieces[i, j].type == PieceType.King && PieceManager.Instance.pieces[i, j].team == team)
                        ourKing = PieceManager.Instance.pieces[i, j];

        // Is king in check?
        bool kingChecked = false;
        for (int i = 0; i < TileManager.TILE_COUNT; i++) {
            for (int j = 0; j < TileManager.TILE_COUNT; j++) {
                if (PieceManager.Instance.pieces[i, j] != null && PieceManager.Instance.pieces[i, j].team != ourKing.team) {
                    List<Vector2Int> enemyMoves = PieceManager.Instance.pieces[i, j].GetValidMoves(ref PieceManager.Instance.pieces, TileManager.TILE_COUNT, Chessboard.lastMove);

                    if (GameInput.Instance.ContainsMove(ref enemyMoves, new Vector2Int(ourKing.currentX, ourKing.currentY))) {
                        kingChecked = true;
                        break;
                    }
                }
            }
        }

        // Do we have any moves left?
        int movesLeft = 0;
        for (int i = 0; i < TileManager.TILE_COUNT; i++) {
            for (int j = 0; j < TileManager.TILE_COUNT; j++) {
                if (PieceManager.Instance.pieces[i, j] != null && PieceManager.Instance.pieces[i, j].team == ourKing.team) {
                    PieceManager.Instance.currentPiece = PieceManager.Instance.pieces[i, j];
                    Chessboard.validMoves = PieceManager.Instance.pieces[i, j].GetValidMoves(ref PieceManager.Instance.pieces, TileManager.TILE_COUNT, Chessboard.lastMove);
                    Chessboard.PreventMove();
                    if (Chessboard.validMoves.Count > 0)
                        movesLeft++;
                }
            }
        }

        Chessboard.validMoves.Clear();
        PieceManager.Instance.currentPiece = null;

        if (movesLeft == 0) {
            if (kingChecked)
                Checkmate(team == TeamColor.White ? TeamColor.Black : TeamColor.White);
            else
                Stalemate();
        }
    }

    private void Checkmate(TeamColor winningTeam) {
        gameOverUIActive = true;

        if (winningTeam == TeamColor.White)
            OnGameOver?.Invoke(this, new OnGameOverEventArgs { resultText = "White team wins!" });
        else
            OnGameOver?.Invoke(this, new OnGameOverEventArgs { resultText = "Black team wins!" });
    }

    private void Stalemate() {
        gameOverUIActive = true;
        OnGameOver?.Invoke(this, new OnGameOverEventArgs { resultText = "Draw!" });
    }
}
