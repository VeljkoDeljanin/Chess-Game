using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    [Header("Art Stuff")]
    private static float deathScale = 0.6f;
    private static float deathSpacing = 0.15f;


    [Header("Prefabs and Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    // Logic
    private const int TILE_COUNT = 8;
    
    private static Piece[,] pieces;
    private static Piece currentPiece;
    private static List<Vector2Int> validMoves = new List<Vector2Int>();
    private static List<Piece> deadWhites = new List<Piece>();
    private static List<Piece> deadBlacks = new List<Piece>();
    private static bool isWhiteTurn;

    public void Awake() 
    {
        isWhiteTurn = true;

        Tile.GenerateAllTiles(TILE_COUNT, transform);

        SpawnAllPieces();
        PositionAllPieces();
    }

    private void Update()
    {
        MouseInput.UpdateInput(ref validMoves, ref pieces, ref currentPiece, isWhiteTurn, TILE_COUNT);
    }

    // Spawning the pieces
    private void SpawnAllPieces()
    {
        pieces = new Piece[TILE_COUNT, TILE_COUNT];

        // White
        pieces[0, 0] = SpawnSinglePiece(PieceType.Rook, TeamColor.White);
        pieces[1, 0] = SpawnSinglePiece(PieceType.Knight, TeamColor.White);
        pieces[2, 0] = SpawnSinglePiece(PieceType.Bishop, TeamColor.White);
        pieces[3, 0] = SpawnSinglePiece(PieceType.Queen, TeamColor.White);
        pieces[4, 0] = SpawnSinglePiece(PieceType.King, TeamColor.White);
        pieces[5, 0] = SpawnSinglePiece(PieceType.Bishop, TeamColor.White);
        pieces[6, 0] = SpawnSinglePiece(PieceType.Knight, TeamColor.White);
        pieces[7, 0] = SpawnSinglePiece(PieceType.Rook, TeamColor.White);
        for (int i = 0; i < TILE_COUNT; i++)
            pieces[i, 1] = SpawnSinglePiece(PieceType.Pawn, TeamColor.White);

        // Black
        pieces[0, 7] = SpawnSinglePiece(PieceType.Rook, TeamColor.Black);
        pieces[1, 7] = SpawnSinglePiece(PieceType.Knight, TeamColor.Black);
        pieces[2, 7] = SpawnSinglePiece(PieceType.Bishop, TeamColor.Black);
        pieces[3, 7] = SpawnSinglePiece(PieceType.Queen, TeamColor.Black);
        pieces[4, 7] = SpawnSinglePiece(PieceType.King, TeamColor.Black);
        pieces[5, 7] = SpawnSinglePiece(PieceType.Bishop, TeamColor.Black);
        pieces[6, 7] = SpawnSinglePiece(PieceType.Knight, TeamColor.Black);
        pieces[7, 7] = SpawnSinglePiece(PieceType.Rook, TeamColor.Black);
        for (int i = 0; i < TILE_COUNT; i++)
            pieces[i, 6] = SpawnSinglePiece(PieceType.Pawn, TeamColor.Black);
    }
    private Piece SpawnSinglePiece(PieceType type, TeamColor team)
    {
        Piece piece = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Piece>();

        piece.type = type;
        piece.team = team;
        piece.GetComponent<MeshRenderer>().material = teamMaterials[(int)team - 1];

        return piece;
    }

    // Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT; x++)
            for (int y = 0; y < TILE_COUNT; y++)
                if (pieces[x, y] != null)
                    PositionSinglePiece(x, y, false);
    }
    private static void PositionSinglePiece(int x, int y, bool animate = true)
    {
        pieces[x, y].currentX = x;
        pieces[x, y].currentY = y;
        pieces[x, y].SetPosition(Tile.GetTileCenter(x, y), animate);
    }
    
    public static void MovePiece(Piece piece, int x, int y)
    {
        // Is there another piece on the target position?
        if (pieces[x, y] != null)
        {
            Piece target = pieces[x, y];

            // TODO: King check logic
            target.SetScale(Vector3.one * deathScale);
            if (target.team == TeamColor.White)
            {
                target.SetPosition(Tile.GetTileCenter(8, -1) + Vector3.forward * deadWhites.Count * deathSpacing);
                deadWhites.Add(target);
            }
            else
            {
                target.SetPosition(Tile.GetTileCenter(-1, 8) + Vector3.back * deadBlacks.Count * deathSpacing);
                deadBlacks.Add(target);
            }
        }

        pieces[x, y] = piece;
        pieces[piece.currentX, piece.currentY] = null;

        PositionSinglePiece(x, y);

        isWhiteTurn = !isWhiteTurn;

        if (currentPiece != null)
            currentPiece = null;
        Tile.RemoveHighlights(ref validMoves); 
    }

}
