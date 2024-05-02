using System;
using Unity.Netcode;
using UnityEngine;

public class PieceManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;
    [SerializeField] private Transform chessboardTransform;

    public static PieceManager Instance { get; private set; }

    public Piece[,] pieces;
    public Piece currentPiece;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        SpawnAllPieces();
        PositionAllPieces();
    }

    // Spawning the pieces
    public void SpawnAllPieces()
    {
        pieces = new Piece[TileManager.TILE_COUNT, TileManager.TILE_COUNT];

        // White pieces
        pieces[0, 0] = SpawnSinglePiece(PieceType.Rook, TeamColor.White);
        pieces[1, 0] = SpawnSinglePiece(PieceType.Knight, TeamColor.White);
        pieces[2, 0] = SpawnSinglePiece(PieceType.Bishop, TeamColor.White);
        pieces[3, 0] = SpawnSinglePiece(PieceType.Queen, TeamColor.White);
        pieces[4, 0] = SpawnSinglePiece(PieceType.King, TeamColor.White);
        pieces[5, 0] = SpawnSinglePiece(PieceType.Bishop, TeamColor.White);
        pieces[6, 0] = SpawnSinglePiece(PieceType.Knight, TeamColor.White);
        pieces[7, 0] = SpawnSinglePiece(PieceType.Rook, TeamColor.White);
        for (int i = 0; i < TileManager.TILE_COUNT; i++)
            pieces[i, 1] = SpawnSinglePiece(PieceType.Pawn, TeamColor.White);

        // Black pieces
        pieces[0, 7] = SpawnSinglePiece(PieceType.Rook, TeamColor.Black);
        pieces[1, 7] = SpawnSinglePiece(PieceType.Knight, TeamColor.Black);
        pieces[2, 7] = SpawnSinglePiece(PieceType.Bishop, TeamColor.Black);
        pieces[3, 7] = SpawnSinglePiece(PieceType.Queen, TeamColor.Black);
        pieces[4, 7] = SpawnSinglePiece(PieceType.King, TeamColor.Black);
        pieces[5, 7] = SpawnSinglePiece(PieceType.Bishop, TeamColor.Black);
        pieces[6, 7] = SpawnSinglePiece(PieceType.Knight, TeamColor.Black);
        pieces[7, 7] = SpawnSinglePiece(PieceType.Rook, TeamColor.Black);
        for (int i = 0; i < TileManager.TILE_COUNT; i++)
            pieces[i, 6] = SpawnSinglePiece(PieceType.Pawn, TeamColor.Black);
    }
    public Piece SpawnSinglePiece(PieceType type, TeamColor team)
    {
        Transform pieceTransform = Instantiate(prefabs[(int)type - 1], chessboardTransform).GetComponent<Transform>();
        Piece piece = pieceTransform.GetComponent<Piece>();

        piece.type = type;
        piece.team = team;
        piece.GetComponent<MeshRenderer>().material = teamMaterials[(int)team - 1];

        return piece;
    }

    // Positioning
    public void PositionAllPieces()
    {
        for (int x = 0; x < TileManager.TILE_COUNT; x++)
            for (int y = 0; y < TileManager.TILE_COUNT; y++)
                if (pieces[x, y] != null)
                    PositionSinglePiece(x, y, false);
    }
    public void PositionSinglePiece(int x, int y, bool animate = true)
    {
        pieces[x, y].currentX = x;
        pieces[x, y].currentY = y;
        pieces[x, y].SetPosition(TileManager.Instance.GetTileCenter(x, y), animate);
    }

    // Move piece synchronization
    [ServerRpc(RequireOwnership = false)]
    public void MovePieceServerRpc(Vector2Int oldPosition, Vector2Int newPosition)
    {
        MovePieceClientRpc(oldPosition, newPosition);
    }
    [ClientRpc]
    private void MovePieceClientRpc(Vector2Int oldPosition, Vector2Int newPosition)
    {
        Piece piece = pieces[oldPosition.x, oldPosition.y];
        int x = newPosition.x, y = newPosition.y;

        if (piece.type == PieceType.Pawn && Mathf.Abs(piece.currentX - x) == 1 && Mathf.Abs(piece.currentY - y) == 1 && pieces[x, y] == null)
            Chessboard.enPassant = true;
        int y2 = y;
        if (Chessboard.enPassant)
            y2 += ((piece.team == TeamColor.White) ? -1 : 1);

        // Is there another piece on the target position?
        if (pieces[x, y2] != null)
        {
            Piece target = pieces[x, y2];

            target.SetScale(Vector3.one * Chessboard.deathScale);
            if (target.team == TeamColor.White)
            {
                target.SetPosition(TileManager.Instance.GetTileCenter(8, -1) + Chessboard.deadWhites.Count * Chessboard.deathSpacing * Vector3.forward);
                Chessboard.deadWhites.Add(target);
            }
            else
            {
                target.SetPosition(TileManager.Instance.GetTileCenter(-1, 8) + Chessboard.deadBlacks.Count * Chessboard.deathSpacing * Vector3.back);
                Chessboard.deadBlacks.Add(target);
            }
        }

        // Castling
        if (piece.type == PieceType.King && Mathf.Abs(x - piece.currentX) == 2)
        {
            Piece rook = pieces[(piece.currentX > x) ? 0 : 7, piece.currentY];

            pieces[(piece.currentX > x) ? 3 : 5, piece.currentY] = rook;
            PositionSinglePiece((piece.currentX > x) ? 3 : 5, piece.currentY);
            pieces[(piece.currentX > x) ? 0 : 7, piece.currentY] = null;
        }

        Chessboard.lastMove = new Tuple<Vector2Int, Vector2Int>(new(piece.currentX, piece.currentY), new(x, y));
        pieces[x, y] = piece;
        pieces[piece.currentX, piece.currentY] = null;
        if (Chessboard.enPassant)
            pieces[x, y2] = null;

        PositionSinglePiece(x, y);

        Chessboard.ActivatePromotionMenu();

        if (piece.type == PieceType.Rook || piece.type == PieceType.King)
            piece.moved = true;

        Chessboard.isWhiteTurn = !Chessboard.isWhiteTurn;
        Chessboard.enPassant = false;

        currentPiece = null;

        TileManager.Instance.RemoveHighlights(ref Chessboard.validMoves);

        Chessboard.CheckForCheckmate(piece.team == TeamColor.White ? TeamColor.Black : TeamColor.White);

        currentPiece = null;
    }
}
