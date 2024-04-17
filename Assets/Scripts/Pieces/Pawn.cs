using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override List<Vector2Int> GetValidMoves(ref Piece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        int direction = team == TeamColor.White ? 1 : -1;

        //One forward
        if (board[currentX, currentY + direction] == null) 
            moves.Add(new Vector2Int(currentX, currentY + direction));

        //Two forward
        if (board[currentX, currentY + direction] == null)
        {
            if (team == TeamColor.White && currentY == 1 && board[currentX, currentY + 2 * direction] == null)
                moves.Add(new Vector2Int(currentX, currentY + 2*direction));

            if (team == TeamColor.Black && currentY == 6 && board[currentX, currentY + 2 * direction] == null)
                moves.Add(new Vector2Int(currentX, currentY + 2 * direction));
        }

        //Eating
        if (currentX != tileCountX - 1)
            if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
                moves.Add(new Vector2Int(currentX + 1, currentY + direction));

        if (currentX != 0)
            if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
                moves.Add(new Vector2Int(currentX - 1, currentY + direction));


        return moves;
    }
}
