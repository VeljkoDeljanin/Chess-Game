using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Bishop : Piece
{
    public override List<Vector2Int> GetValidMoves(ref Piece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves  = new List<Vector2Int>();

        // Forward right
        for (int x = currentX + 1, y = currentY + 1; x < tileCountX && y < tileCountY; x++, y++)
        {
            if (board[x, y] == null)
                moves.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team)
                    moves.Add(new Vector2Int(x, y));

                break;
            }
        }

        // Forward left
        for (int x = currentX - 1, y = currentY + 1; x >= 0 && y < tileCountY; x--, y++)
        {
            if (board[x, y] == null)
                moves.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team)
                    moves.Add(new Vector2Int(x, y));

                break;
            }
        }

        // Backward right
        for (int x = currentX + 1, y = currentY - 1; x < tileCountX && y >= 0; x++, y--)
        {
            if (board[x, y] == null)
                moves.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team)
                    moves.Add(new Vector2Int(x, y));

                break;
            }
        }

        // Backward left
        for (int x = currentX - 1, y = currentY - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (board[x, y] == null)
                moves.Add(new Vector2Int(x, y));
            else
            {
                if (board[x, y].team != team)
                    moves.Add(new Vector2Int(x, y));

                break;
            }
        }

        return moves;
    }
}
