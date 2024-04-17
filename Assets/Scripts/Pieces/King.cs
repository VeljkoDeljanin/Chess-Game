using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    public override List<Vector2Int> GetValidMoves(ref Piece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        int[] dx = new int[8] { 1, 1, 1, -1, -1, -1, 0, 0};
        int[] dy = new int[8] { 1, 0, -1, 1, 0, -1, 1, -1};

        int x, y;
        for (int i = 0; i < 8; i++)
        {
            x = currentX + dx[i];
            y = currentY + dy[i];

            if (x >= 0 && y >= 0 && x < tileCountX && y < tileCountY)
                if (board[x, y] == null || board[x, y].team != team)
                    moves.Add(new Vector2Int(x, y));
        }

        return moves;
    }
}
