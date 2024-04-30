using System;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    public override List<Vector2Int> GetValidMoves(ref Piece[,] board, int tileCount, Tuple<Vector2Int, Vector2Int> lastMove)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        int[] dx = new int[8] { 1, 1, 1, -1, -1, -1, 0, 0};
        int[] dy = new int[8] { 1, 0, -1, 1, 0, -1, 1, -1};

        int x, y;
        for (int i = 0; i < 8; i++)
        {
            x = currentX + dx[i];
            y = currentY + dy[i];

            if (x >= 0 && y >= 0 && x < tileCount && y < tileCount)
                if (board[x, y] == null || board[x, y].team != team)
                    moves.Add(new Vector2Int(x, y));
        }

        //Castling
        if (!moved)
        {
            x = currentX;
            y = currentY;

            Piece leftRook = null;
            Piece rightRook = null;
            if(team == TeamColor.White)
            {
                if (board[0,0] != null)
                    leftRook = board[0,0];

                if (board[7,0] != null)
                    rightRook = board[7,0];
            }
            else
            {
                if (board[0, 7] != null)
                    leftRook = board[0, 7];

                if (board[7, 7] != null)
                    rightRook = board[7, 7];
            }
            //Left castle
            if(leftRook != null && !leftRook.moved)
            {
                if (board[x-1,y] == null && board[x-2,y] == null && board[x-3,y] == null)
                {
                    moves.Add(new Vector2Int(x-2, y));
                }
            }
            //Right castle
            if(rightRook != null && !rightRook.moved)
            {
                if (board[x + 1, y] == null && board[x + 2, y] == null)
                {
                    moves.Add(new Vector2Int(x + 2, y));
                }
            }
        }

        return moves;
    }
}
