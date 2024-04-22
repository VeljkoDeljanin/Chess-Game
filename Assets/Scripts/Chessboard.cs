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
        if(piece.type == PieceType.Pawn && Math.Abs(piece.currentX - x) == 1 && Math.Abs(piece.currentY - y) == 1 && pieces[x, y] == null)
            enPassant = true;
        int y2 = y;
        if (enPassant)
            y2 += ((piece.team == TeamColor.White) ? -1 : 1);
        // Is there another piece on the target position?
        if (pieces[x, y2] != null)
        {

            Piece target = pieces[x, y2];
            print(target);

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

        lastMove = new Tuple<Vector2Int, Vector2Int>(new(piece.currentX, piece.currentY), new(x, y));
        pieces[x, y] = piece;
        pieces[piece.currentX, piece.currentY] = null;
        if (enPassant)
            pieces[x, y2] = null;

        PositionSinglePiece(x, y);

        ActivatePromotionMenu();

        isWhiteTurn = !isWhiteTurn;
        enPassant = false;

        if (currentPiece != null)
            currentPiece = null;

        Tile.RemoveHighlights(ref validMoves);

        if (IsCheckmate(piece.team == TeamColor.White ? TeamColor.Black : TeamColor.White))
            Checkmate(piece.team);

        currentPiece = null;
    }

    // Checkmate
    private static bool IsCheckmate(TeamColor team)
    {
        //Getting the king we are checking
        Piece ourKing = null;
        for (int i = 0; i < TILE_COUNT; i++)
            for (int j = 0; j < TILE_COUNT; j++)
                if (pieces[i, j] != null)
                    if (pieces[i, j].type == PieceType.King && pieces[i, j].team == team)
                        ourKing = pieces[i, j];

        bool kingChecked = false;
        //Asking if king is checked
        for (int i = 0; i < TILE_COUNT; i++)
            for (int j = 0; j < TILE_COUNT; j++)
                if (pieces[i, j] != null && pieces[i, j].team != ourKing.team)
                {
                    List<Vector2Int> enemyMoves = pieces[i, j].GetValidMoves(ref pieces, TILE_COUNT);
                    if (MouseInput.IsValidMove(ref enemyMoves, new Vector2Int(ourKing.currentX, ourKing.currentY)))
                    {
                        kingChecked = true;
                        break;
                    }
                }

        int movesLeft = 0;
        //Asking if we have any moves left
        for (int i = 0; i < TILE_COUNT; i++)
            for (int j = 0; j < TILE_COUNT; j++)
                if (pieces[i, j] != null && pieces[i, j].team == ourKing.team)
                {
                    currentPiece = pieces[i, j];
                    validMoves = pieces[i, j].GetValidMoves(ref pieces, TILE_COUNT);
                    PreventMove();
                    if (validMoves.Count > 0)
                        movesLeft++;
                }

        return (movesLeft == 0 && kingChecked);
    }
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

    public static void PreventMove()
    {
        //Getting the king we are checking
        Piece ourKing = null;
        for (int i = 0; i < TILE_COUNT; i++)
            for (int j = 0; j < TILE_COUNT; j++)
                if (pieces[i, j] != null)
                    if (pieces[i, j].type == PieceType.King && pieces[i, j].team == currentPiece.team)
                        ourKing = pieces[i, j];
            

        int originalX = currentPiece.currentX;
        int originalY = currentPiece.currentY;

        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        //Simulating all valid moves for currentPiece
        for(int i = 0; i < validMoves.Count; i++)
        {
            int x = validMoves[i].x;
            int y = validMoves[i].y;
            if(SimulateMove(ourKing, currentPiece, x, y))
            {
                movesToRemove.Add(new Vector2Int(x, y));
            }
            currentPiece.currentX = originalX;
            currentPiece.currentY = originalY;
        }

        for (int i = 0; i < movesToRemove.Count; i++)
            validMoves.Remove(movesToRemove[i]);

    }

    private static bool SimulateMove(Piece ourKing, Piece currentPiece, int x, int y)
    {
        Vector2Int kingPos = new Vector2Int(ourKing.currentX, ourKing.currentY);

        if(currentPiece.type == PieceType.King)
        {
            kingPos.x = x;
            kingPos.y = y;
        }

        //Copying board
        Piece[,] board = new Piece[TILE_COUNT, TILE_COUNT];
        for (int i = 0; i < TILE_COUNT; i++)
            for (int j = 0; j < TILE_COUNT; j++)
                board[i, j] = pieces[i, j];

        //Making a move
        if (currentPiece.type == PieceType.Pawn && Math.Abs(currentPiece.currentX - x) == 1 && Math.Abs(currentPiece.currentY - y) == 1 && board[x, y] == null)
            enPassant = true;
        int y2 = y;

        if(enPassant)
            y2 += ((currentPiece.team == TeamColor.White) ? -1 : 1);
        enPassant = false;

        board[currentPiece.currentX, currentPiece.currentY] = null;
        currentPiece.currentX = x;
        currentPiece.currentY = y;
       
        board[x, y2] = null;
        board[x, y] = currentPiece;


        //Asking if king is checked
        for(int i = 0; i < TILE_COUNT; i++)
            for(int j = 0; j < TILE_COUNT; j++)
                if (board[i, j] != null && board[i, j].team != ourKing.team)
                {
                    List<Vector2Int> enemyMoves = board[i, j].GetValidMoves(ref board, TILE_COUNT);
                    if (MouseInput.IsValidMove(ref enemyMoves, kingPos))
                        return true;
                }

        return false;
    }
}
