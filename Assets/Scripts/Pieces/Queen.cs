using System.Collections.Generic;
using UnityEngine;

public class Queen : Piece
{
    public override List<Vector2Int> GetValidMoves(ref Piece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

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

        //Forward
        for (int i = currentY + 1; i < tileCountY; i++)
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
        for (int i = currentX + 1; i < tileCountX; i++)
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
