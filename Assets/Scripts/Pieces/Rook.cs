using System;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece {
    public override List<Vector2Int> GetValidMoves(ref Piece[,] board, int tileCount, Tuple<Vector2Int, Vector2Int> lastMove) {
        List<Vector2Int> moves = new List<Vector2Int> ();

        // Forward
        for(int i = currentY + 1; i < tileCount; i++) {
            if (board[currentX, i] == null) {
                moves.Add(new Vector2Int(currentX, i));
            } else {
                if (board[currentX, i].team != team)
                    moves.Add(new Vector2Int(currentX, i));

                break;
            }
        }

        // Backward
        for (int i = currentY - 1; i >= 0; i--) {
            if (board[currentX, i] == null) {
                moves.Add(new Vector2Int(currentX, i));
            } else {
                if (board[currentX, i].team != team)
                    moves.Add(new Vector2Int(currentX, i));

                break;
            }
        }

        // Right
        for (int i = currentX + 1; i < tileCount; i++) {
            if (board[i, currentY] == null) {
                moves.Add(new Vector2Int(i, currentY));
            } else {
                if (board[i, currentY].team != team)
                    moves.Add(new Vector2Int(i, currentY));

                break;
            }
        }

        // Left
        for (int i = currentX - 1; i >= 0; i--) {
            if (board[i, currentY] == null) {
                moves.Add(new Vector2Int(i, currentY));
            } else {
                if (board[i, currentY].team != team)
                    moves.Add(new Vector2Int(i, currentY));

                break;
            }
        }


        return moves;
    }
}
