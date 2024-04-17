using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{
    public override List<Vector2Int> GetValidMoves(ref Piece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves  = new List<Vector2Int>();

        //Forward-Right
        for (int i = currentX + 1, j = currentY + 1; i < tileCountX && j < tileCountY; i++, j++)
        {
            if (board[i, j] == null)
                moves.Add(new Vector2Int(i, j));
            else
            {
                if (board[i, j].team != team)
                    moves.Add(new Vector2Int(i, j));
                break;
            }
        }

        //Forward-Left
        for (int i = currentX - 1, j = currentY + 1; i >= 0 && j <= tileCountY; i--, j++)
        {
            if (board[i, j] == null)
                moves.Add(new Vector2Int(i, j));
            else
            {
                if (board[i, j].team != team)
                    moves.Add(new Vector2Int(i, j));
                break;
            }
        }

        //Backward-Right
        for (int i = currentX + 1, j = currentY - 1; i < tileCountX && j >= 0; i++, j--)
        {
            if (board[i, j] == null)
                moves.Add(new Vector2Int(i, j));
            else
            {
                if (board[i, j].team != team)
                    moves.Add(new Vector2Int(i, j));
                break;
            }
        }

        //Backward-Left
        for (int i = currentX - 1, j = currentY - 1; i >= 0 && j >= 0; i--, j--)
        {
            if (board[i, j] == null)
                moves.Add(new Vector2Int(i, j));
            else
            {
                if (board[i, j].team != team)
                    moves.Add(new Vector2Int(i, j));
                break;
            }
        }

        return moves;
    }
}
