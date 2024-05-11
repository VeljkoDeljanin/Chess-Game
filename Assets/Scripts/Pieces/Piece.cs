using System;
using System.Collections.Generic;
using UnityEngine;

public enum PieceType {
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

public enum TeamColor {
    None = 0,
    White = 1,
    Black = 2
}

public class Piece : MonoBehaviour {
    public PieceType type;
    public TeamColor team;
    public int currentX, currentY;
    public bool moved = false;

    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;

    private void Start() {
        transform.rotation = Quaternion.Euler((team == TeamColor.Black) ? new Vector3(0, 180, 0) : Vector3.zero);
    }

    private void Update() {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual List<Vector2Int> GetValidMoves(ref Piece[,] board, int tileCount, Tuple<Vector2Int, Vector2Int> lastMove) {
        return null;
    }

    public void SetPosition(Vector3 position, bool animate = true) {
        desiredPosition = position;
        if (!animate) {
            transform.position = desiredPosition;
        }
    }
    public void SetScale(Vector3 scale, bool animate = true) {
        desiredScale = scale;
        if (!animate) {
            transform.localScale = desiredScale;
        }
    }
}
