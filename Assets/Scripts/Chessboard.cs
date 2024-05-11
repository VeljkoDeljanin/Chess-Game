using System;
using System.Collections.Generic;
using UnityEngine;

public class Chessboard : MonoBehaviour {
    // Logic
    public static List<Vector2Int> validMoves = new List<Vector2Int>();

    public static Tuple<Vector2Int, Vector2Int> lastMove = new Tuple<Vector2Int, Vector2Int>(new Vector2Int(0, 0), new Vector2Int(0, 0));
    public static Tuple<Vector2Int, Vector2Int> lastSimulation = new Tuple<Vector2Int, Vector2Int>(new Vector2Int(0, 0), new Vector2Int(0, 0));
    public static bool enPassant;

    private void Awake()  {
        enPassant = false;
    }

    // Prevent check
    public static void PreventMove() {
        // Getting the king we are checking
        Piece ourKing = null;
        for (int i = 0; i < TileManager.TILE_COUNT; i++)
            for (int j = 0; j < TileManager.TILE_COUNT; j++)
                if (PieceManager.Instance.pieces[i, j] != null)
                    if (PieceManager.Instance.pieces[i, j].type == PieceType.King && PieceManager.Instance.pieces[i, j].team == PieceManager.Instance.currentPiece.team)
                        ourKing = PieceManager.Instance.pieces[i, j];
            
        int originalX = PieceManager.Instance.currentPiece.currentX;
        int originalY = PieceManager.Instance.currentPiece.currentY;

        List<Vector2Int> movesToRemove = new();

        // Simulating all valid moves for selected piece
        for(int i = 0; i < validMoves.Count; i++) {
            int x = validMoves[i].x;
            int y = validMoves[i].y;

            if(SimulateMove(ourKing, PieceManager.Instance.currentPiece, x, y))
                movesToRemove.Add(new Vector2Int(x, y));

            PieceManager.Instance.currentPiece.currentX = originalX;
            PieceManager.Instance.currentPiece.currentY = originalY;
        }

        for (int i = 0; i < movesToRemove.Count; i++)
            validMoves.Remove(movesToRemove[i]);
    }
    private static bool SimulateMove(Piece ourKing, Piece currentPiece, int x, int y) {
        Vector2Int kingPos = new(ourKing.currentX, ourKing.currentY);

        if(currentPiece.type == PieceType.King) {
            kingPos.x = x;
            kingPos.y = y;
        }

        // Copying board
        Piece[,] board = new Piece[TileManager.TILE_COUNT, TileManager.TILE_COUNT];
        for (int i = 0; i < TileManager.TILE_COUNT; i++)
            for (int j = 0; j < TileManager.TILE_COUNT; j++)
                board[i, j] = PieceManager.Instance.pieces[i, j];

        // Making a move
        if (currentPiece.type == PieceType.Pawn && Mathf.Abs(currentPiece.currentX - x) == 1 && Mathf.Abs(currentPiece.currentY - y) == 1 && board[x, y] == null)
            enPassant = true;
        int y2 = y;

        if(enPassant)
            y2 += ((currentPiece.team == TeamColor.White) ? -1 : 1);

        bool castling = false;
        if (currentPiece.type == PieceType.King && Mathf.Abs(x - currentPiece.currentX) == 2)
            castling = true;

        lastSimulation = new Tuple<Vector2Int, Vector2Int>(new(currentPiece.currentX, currentPiece.currentY), new(x, y));
        board[x, y] = currentPiece;
        board[currentPiece.currentX, currentPiece.currentY] = null;
        if (enPassant)
            board[x, y2] = null;

        currentPiece.currentX = x;
        currentPiece.currentY = y;

        enPassant = false;

        // Is king in check?
        for (int i = 0; i < TileManager.TILE_COUNT; i++) {
            for (int j = 0; j < TileManager.TILE_COUNT; j++) {
                if (board[i, j] != null && board[i, j].team != ourKing.team) {
                    List<Vector2Int> enemyMoves = board[i, j].GetValidMoves(ref board, TileManager.TILE_COUNT, lastSimulation);
                    if (GameInput.Instance.ContainsMove(ref enemyMoves, kingPos))
                        return true;

                    if (castling && (GameInput.Instance.ContainsMove(ref enemyMoves, new(4, currentPiece.currentY)) ||
                        GameInput.Instance.ContainsMove(ref enemyMoves, new((currentPiece.currentX > 4) ? 5 : 3, currentPiece.currentY)))) {
                        return true;
                    }

                }
            }
        }

        return false;
    }
}
