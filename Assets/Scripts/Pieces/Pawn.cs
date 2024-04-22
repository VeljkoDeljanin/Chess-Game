using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override List<Vector2Int> GetValidMoves(ref Piece[,] board, int tileCount)
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
        if (currentX != tileCount - 1)
            if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team)
                moves.Add(new Vector2Int(currentX + 1, currentY + direction));

        if (currentX != 0)
            if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team)
                moves.Add(new Vector2Int(currentX - 1, currentY + direction));

        //En passant
        int x1 = Chessboard.lastMove.Item1.x;
        int x2 = Chessboard.lastMove.Item2.x;
        int y1 = Chessboard.lastMove.Item1.y;
        int y2 = Chessboard.lastMove.Item2.y;

        if (currentY == y2 && board[x2, y2].type == PieceType.Pawn &&
            board[x2, y2].team != team && Mathf.Abs(y1 - y2) == 2)
            if (x2 == currentX - 1 || x2 == currentX + 1)
               moves.Add(new Vector2Int(x2, currentY + direction));
 


        return moves;
    }
}
