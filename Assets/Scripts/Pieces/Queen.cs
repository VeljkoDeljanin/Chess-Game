using System.Collections.Generic;
using UnityEngine;

public class Queen : Piece
{
    public override List<Vector2Int> GetValidMoves(ref Piece[,] board, int tileCount)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // Forward right
        for (int x = currentX + 1, y = currentY + 1; x < tileCount && y < tileCount; x++, y++)
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
        for (int x = currentX - 1, y = currentY + 1; x >= 0 && y < tileCount; x--, y++)
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
        for (int x = currentX + 1, y = currentY - 1; x < tileCount && y >= 0; x++, y--)
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

        //Forward
        for (int i = currentY + 1; i < tileCount; i++)
        {
            if (board[currentX, i] == null)
                moves.Add(new Vector2Int(currentX, i));
            else
            {
                if (board[currentX, i].team != team)
                    moves.Add(new Vector2Int(currentX, i));
                break;
            }
        }

        //Backward
        for (int i = currentY - 1; i >= 0; i--)
        {
            if (board[currentX, i] == null)
                moves.Add(new Vector2Int(currentX, i));
            else
            {
                if (board[currentX, i].team != team)
                    moves.Add(new Vector2Int(currentX, i));
                break;
            }
        }

        //Right
        for (int i = currentX + 1; i < tileCount; i++)
        {
            if (board[i, currentY] == null)
                moves.Add(new Vector2Int(i, currentY));
            else
            {
                if (board[i, currentY].team != team)
                    moves.Add(new Vector2Int(i, currentY));
                break;
            }
        }

        //Left
        for (int i = currentX - 1; i >= 0; i--)
        {
            if (board[i, currentY] == null)
                moves.Add(new Vector2Int(i, currentY));
            else
            {
                if (board[i, currentY].team != team)
                    moves.Add(new Vector2Int(i, currentY));
                break;
            }
        }

        return moves;
    }
}
