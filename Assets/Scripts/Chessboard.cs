using System;
using System.Collections.Generic;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    [Header("Art Stuff")]
    [SerializeField] private GameObject _victoryScreen;
    [SerializeField] private GameObject _promotionMenu;

    [Header("Prefabs and Materials")]
    [SerializeField] private GameObject[] _prefabs;
    [SerializeField] private Material[] _teamMaterials;

    private static GameObject victoryScreen; 
    private static GameObject promotionMenu;
    private static GameObject[] prefabs;
    private static Material[] teamMaterials;
    private static Transform transform2;

    // Logic
    private const int TILE_COUNT = 8;
    private static Piece[,] pieces;
    private static Piece currentPiece;
    private static List<Vector2Int> validMoves = new();
    private static List<Piece> deadWhites = new();
    private static List<Piece> deadBlacks = new();
    private static float deathScale = 0.6f;
    private static float deathSpacing = 0.15f;
    private static bool isWhiteTurn;
    public static Tuple<Vector2Int, Vector2Int> lastMove = new(new(0, 0), new(0, 0));    
    public static bool enPassant = false;
    public static PieceType promotionType = 0;
    private static bool promotionMenuActive = false;

    public void Awake() 
    {
        victoryScreen = _victoryScreen;
        promotionMenu = _promotionMenu;
        prefabs = _prefabs;
        teamMaterials = _teamMaterials;
        transform2 = transform;

        Tile.GenerateAllTiles(TILE_COUNT, transform);

        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;
    }
    private void Update()
    {
        if(!promotionMenuActive)
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
    private static Piece SpawnSinglePiece(PieceType type, TeamColor team)
    {
        Piece piece = Instantiate(prefabs[(int)type - 1], transform2).GetComponent<Piece>();

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

        int y2 = y;
        if (enPassant)
            y2 += ((piece.team == TeamColor.White) ? -1 : 1);
        // Is there another piece on the target position?
        if (pieces[x, y2] != null)
        {

            Piece target = pieces[x, y2];
            print(target);

            // TODO: King check logic

            target.SetScale(Vector3.one * deathScale);
            if (target.team == TeamColor.White)
            {
                if (target.type == PieceType.King)
                    Checkmate(TeamColor.Black);

                target.SetPosition(Tile.GetTileCenter(8, -1) + Vector3.forward * deadWhites.Count * deathSpacing);
                deadWhites.Add(target);
            }
            else
            {
                if (target.type == PieceType.King)
                    Checkmate(TeamColor.White);

                target.SetPosition(Tile.GetTileCenter(-1, 8) + Vector3.back * deadBlacks.Count * deathSpacing);
                deadBlacks.Add(target);
            }
        }

        lastMove = new Tuple<Vector2Int, Vector2Int>(new(piece.currentX, piece.currentY), new(x, y));
        pieces[x, y] = piece;
        pieces[piece.currentX, piece.currentY] = null;

        PositionSinglePiece(x, y);

        ActivatePromotionMenu();


        isWhiteTurn = !isWhiteTurn;
        enPassant = false;

        if (currentPiece != null)
            currentPiece = null;
        Tile.RemoveHighlights(ref validMoves); 
    }

    // Checkmate
    private static void Checkmate(TeamColor winningTeam)
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild((int)winningTeam - 1).gameObject.SetActive(true);
    }
    public void OnResetButton()
    {
        // UI
        victoryScreen.transform.GetChild((int)TeamColor.White - 1).gameObject.SetActive(false);
        victoryScreen.transform.GetChild((int)TeamColor.Black - 1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);

        // Fields reset
        currentPiece = null;
        validMoves.Clear();

        // Clean up
        for (int x = 0; x < TILE_COUNT; x++)
        {
            for (int y = 0; y < TILE_COUNT; y++)
            {
                if (pieces[x, y] != null)
                    Destroy(pieces[x, y].gameObject);

                pieces[x, y] = null;
            }
        }

        for (int i = 0; i < deadWhites.Count; i++)
            Destroy(deadWhites[i].gameObject);
        for (int i = 0; i < deadBlacks.Count; i++)
            Destroy(deadBlacks[i].gameObject);

        deadWhites.Clear();
        deadBlacks.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;
    }
    public void OnExitButton()
    {
        Application.Quit();
    }

    // Promotion
    private static void ActivatePromotionMenu()
    {
        if (pieces[lastMove.Item2.x, lastMove.Item2.y].type == PieceType.Pawn)
            if (lastMove.Item2.y == 7 || lastMove.Item2.y == 0)
            {
                promotionMenuActive = true;
                promotionMenu.SetActive(true);
                promotionMenu.transform.GetChild((int)pieces[lastMove.Item2.x, lastMove.Item2.y].team - 1).gameObject.SetActive(true);
            }
        promotionType = PieceType.None;
    }
    private static void ProcessPromotion()
    {
        Destroy(pieces[lastMove.Item2.x, lastMove.Item2.y].gameObject);
        Piece newPiece = SpawnSinglePiece(promotionType, (lastMove.Item2.y == 7) ? TeamColor.White : TeamColor.Black);
        pieces[lastMove.Item2.x, lastMove.Item2.y] = newPiece;

        PositionSinglePiece(lastMove.Item2.x, lastMove.Item2.y, false);
        promotionMenuActive = false;
        promotionMenu.transform.GetChild((int)TeamColor.White - 1).gameObject.SetActive(false);
        promotionMenu.transform.GetChild((int)TeamColor.Black - 1).gameObject.SetActive(false);
        promotionMenu.SetActive(false);
    }
    public void OnQueenButton()
    {
        promotionType = PieceType.Queen;
        ProcessPromotion();
    }
    public void OnRookButton()
    {
        promotionType = PieceType.Rook;
        ProcessPromotion();
    }
    public void OnBishopButton()
    {
        promotionType = PieceType.Bishop;
        ProcessPromotion();
    }
    public void OnKnightButton()
    {
        promotionType = PieceType.Knight;
        ProcessPromotion();
    }
}
