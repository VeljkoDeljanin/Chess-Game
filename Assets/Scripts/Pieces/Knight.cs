using System;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{
    public override List<Vector2Int> GetValidMoves(ref Piece[,] board, int tileCount, Tuple<Vector2Int, Vector2Int> lastMove)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        int[] dx = new int[8] {2, 2, -2, -2, 1, -1, 1, -1};
        int[] dy = new int[8] {1, -1, 1, -1, 2, 2, -2, -2};

        int x, y;
        for(int i = 0; i < 8; i++)
        {
            x = currentX + dx[i];
            y = currentY + dy[i];

            if (x >= 0 && y >= 0 && x < tileCount && y < tileCount)
                if (board[x, y] == null || board[x, y].team != team)
                    moves.Add(new Vector2Int(x, y));
        }

        return moves;
    }
}
