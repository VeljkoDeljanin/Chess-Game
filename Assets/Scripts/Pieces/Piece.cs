using UnityEngine;

public enum PieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

public enum TeamColor
{
    None = 0,
    White = 1,
    Black = 2
}

public class Piece : MonoBehaviour
{
    public PieceType type;
    public TeamColor team;
    public int currentX, currentY;

    private Vector3 desiredPosition;
    private Vector3 desiredScale;


}
