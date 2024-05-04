using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Chessboard : MonoBehaviour
{
    [Header("Art Stuff")]
    [SerializeField] private GameObject _victoryScreen;
    [SerializeField] private GameObject _promotionMenu;
    [SerializeField] private TextMeshProUGUI _text;

    private static GameObject victoryScreen;
    private static GameObject promotionMenu;
    private static TextMeshProUGUI text;

    // Logic
    public static List<Vector2Int> validMoves = new();
    public static bool isWhiteTurn = true;

    public static List<Piece> deadWhites = new();
    public static List<Piece> deadBlacks = new();
    public static float deathScale = 0.6f;
    public static float deathSpacing = 0.15f;

    public static Tuple<Vector2Int, Vector2Int> lastMove = new(new(0, 0), new(0, 0));
    public static Tuple<Vector2Int, Vector2Int> lastSimulation = new(new(0, 0), new(0, 0));
    public static bool enPassant = false;
    public static PieceType promotionType = PieceType.None;
    public static bool promotionUIActive = false;
    public static bool gameOverUIActive = false;
    public static bool opponentDisconnectUIActive = false;

    private void Awake() 
    {
        victoryScreen = _victoryScreen;
        promotionMenu = _promotionMenu;
        text = _text;
    }

    // Checkmate
    public static void CheckForCheckmate(TeamColor team)
    {
        // Getting the king we are checking
        Piece ourKing = null;
        for (int i = 0; i < TileManager.TILE_COUNT; i++)
            for (int j = 0; j < TileManager.TILE_COUNT; j++)
                if (PieceManager.Instance.pieces[i, j] != null)
                    if (PieceManager.Instance.pieces[i, j].type == PieceType.King && PieceManager.Instance.pieces[i, j].team == team)
                        ourKing = PieceManager.Instance.pieces[i, j];

        // Is king in check?
        bool kingChecked = false;
        for (int i = 0; i < TileManager.TILE_COUNT; i++)
            for (int j = 0; j < TileManager.TILE_COUNT; j++)
                if (PieceManager.Instance.pieces[i, j] != null && PieceManager.Instance.pieces[i, j].team != ourKing.team)
                {
                    List<Vector2Int> enemyMoves = PieceManager.Instance.pieces[i, j].GetValidMoves(ref PieceManager.Instance.pieces, TileManager.TILE_COUNT, lastMove);
                    if (GameInput.Instance.ContainsMove(ref enemyMoves, new Vector2Int(ourKing.currentX, ourKing.currentY)))
                    {
                        kingChecked = true;
                        break;
                    }
                }

        // Do we have any moves left?
        int movesLeft = 0;
        for (int i = 0; i < TileManager.TILE_COUNT; i++)
            for (int j = 0; j < TileManager.TILE_COUNT; j++)
                if (PieceManager.Instance.pieces[i, j] != null && PieceManager.Instance.pieces[i, j].team == ourKing.team)
                {
                    PieceManager.Instance.currentPiece = PieceManager.Instance.pieces[i, j];
                    validMoves = PieceManager.Instance.pieces[i, j].GetValidMoves(ref PieceManager.Instance.pieces, TileManager.TILE_COUNT, lastMove);
                    PreventMove();
                    if (validMoves.Count > 0)
                        movesLeft++;
                }

        validMoves.Clear();
        PieceManager.Instance.currentPiece = null;

        if (movesLeft == 0)
        {
            if (kingChecked)
                Checkmate(team == TeamColor.White ? TeamColor.Black : TeamColor.White);
            else
                Stalemate();
        }
    }
    private static void Checkmate(TeamColor winningTeam)
    {
        gameOverUIActive = true;
        victoryScreen.SetActive(true);
        if (winningTeam == TeamColor.White)
            text.text = "White team wins!";
        else
            text.text = "Black team wins!";
    }
    private static void Stalemate()
    {
        gameOverUIActive = true;
        victoryScreen.SetActive(true);
        text.text = "Draw!";
    }
    public void OnResetButton()
    {
        // UI
        gameOverUIActive = false;
        victoryScreen.SetActive(false);

        // Fields reset
        PieceManager.Instance.currentPiece = null;
        validMoves.Clear();

        // Clean up
        for (int x = 0; x < TileManager.TILE_COUNT; x++)
        {
            for (int y = 0; y < TileManager.TILE_COUNT; y++)
            {
                if (PieceManager.Instance.pieces[x, y] != null)
                    Destroy(PieceManager.Instance.pieces[x, y].gameObject);

                PieceManager.Instance.pieces[x, y] = null;
            }
        }

        for (int i = 0; i < deadWhites.Count; i++)
            Destroy(deadWhites[i].gameObject);
        for (int i = 0; i < deadBlacks.Count; i++)
            Destroy(deadBlacks[i].gameObject);

        deadWhites.Clear();
        deadBlacks.Clear();

        PieceManager.Instance.SpawnAllPieces();
        PieceManager.Instance.PositionAllPieces();
        isWhiteTurn = true;
    }
    public void OnExitButton()
    {
        Application.Quit();
    }

    // Promotion
    public static void ActivatePromotionMenu()
    {
        if (PieceManager.Instance.pieces[lastMove.Item2.x, lastMove.Item2.y].type == PieceType.Pawn)
            if (lastMove.Item2.y == 7 || lastMove.Item2.y == 0)
            {
                promotionUIActive = true;
                promotionMenu.SetActive(true);
                promotionMenu.transform.GetChild((int)PieceManager.Instance.pieces[lastMove.Item2.x, lastMove.Item2.y].team + 1).gameObject.SetActive(true);
            }
        promotionType = PieceType.None;
    }
    private static void ProcessPromotion()
    {
        Destroy(PieceManager.Instance.pieces[lastMove.Item2.x, lastMove.Item2.y].gameObject);
        Piece newPiece = PieceManager.Instance.SpawnSinglePiece(promotionType, (lastMove.Item2.y == 7) ? TeamColor.White : TeamColor.Black);
        PieceManager.Instance.pieces[lastMove.Item2.x, lastMove.Item2.y] = newPiece;

        PieceManager.Instance.PositionSinglePiece(lastMove.Item2.x, lastMove.Item2.y, false);
        promotionUIActive = false;
        promotionMenu.transform.GetChild((int)TeamColor.White + 1).gameObject.SetActive(false);
        promotionMenu.transform.GetChild((int)TeamColor.Black + 1).gameObject.SetActive(false);
        promotionMenu.SetActive(false);

        CheckForCheckmate((lastMove.Item2.y == 7) ? TeamColor.Black : TeamColor.White);
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

    // Prevent check
    public static void PreventMove()
    {
        // Getting the king we are checking
        Piece ourKing = null;
        for (int i = 0; i < TileManager.TILE_COUNT; i++)
            for (int j = 0; j < TileManager.TILE_COUNT; j++)
                if (PieceManager.Instance.pieces[i, j] != null)
                    if (PieceManager.Instance.pieces[i, j].type == PieceType.King && PieceManager.Instance.pieces[i, j].team == PieceManager.Instance.currentPiece.team)
                        ourKing = PieceManager.Instance.pieces[i, j];
            
        int originalX = PieceManager.Instance.currentPiece.currentX;
        int originalY = PieceManager.Instance.currentPiece.currentY;

        List<Vector2Int> movesToRemove = new();

        // Simulating all valid moves for selected piece
        for(int i = 0; i < validMoves.Count; i++)
        {
            int x = validMoves[i].x;
            int y = validMoves[i].y;

            if(SimulateMove(ourKing, PieceManager.Instance.currentPiece, x, y))
                movesToRemove.Add(new Vector2Int(x, y));

            PieceManager.Instance.currentPiece.currentX = originalX;
            PieceManager.Instance.currentPiece.currentY = originalY;
        }

        for (int i = 0; i < movesToRemove.Count; i++)
            validMoves.Remove(movesToRemove[i]);
    }
    private static bool SimulateMove(Piece ourKing, Piece currentPiece, int x, int y)
    {
        Vector2Int kingPos = new(ourKing.currentX, ourKing.currentY);

        if(currentPiece.type == PieceType.King)
        {
            kingPos.x = x;
            kingPos.y = y;
        }

        // Copying board
        Piece[,] board = new Piece[TileManager.TILE_COUNT, TileManager.TILE_COUNT];
        for (int i = 0; i < TileManager.TILE_COUNT; i++)
            for (int j = 0; j < TileManager.TILE_COUNT; j++)
                board[i, j] = PieceManager.Instance.pieces[i, j];

        // Making a move
        if (currentPiece.type == PieceType.Pawn && Mathf.Abs(currentPiece.currentX - x) == 1 && Mathf.Abs(currentPiece.currentY - y) == 1 && board[x, y] == null)
            enPassant = true;
        int y2 = y;

        if(enPassant)
            y2 += ((currentPiece.team == TeamColor.White) ? -1 : 1);

        bool castling = false;
        if (currentPiece.type == PieceType.King && Mathf.Abs(x - currentPiece.currentX) == 2)
            castling = true;

        lastSimulation = new Tuple<Vector2Int, Vector2Int>(new(currentPiece.currentX, currentPiece.currentY), new(x, y));
        board[x, y] = currentPiece;
        board[currentPiece.currentX, currentPiece.currentY] = null;
        if (enPassant)
            board[x, y2] = null;

        currentPiece.currentX = x;
        currentPiece.currentY = y;

        enPassant = false;

        // Is king in check?
        for(int i = 0; i < TileManager.TILE_COUNT; i++)
            for(int j = 0; j < TileManager.TILE_COUNT; j++)
                if (board[i, j] != null && board[i, j].team != ourKing.team)
                {
                    List<Vector2Int> enemyMoves = board[i, j].GetValidMoves(ref board, TileManager.TILE_COUNT, lastSimulation);
                    if (GameInput.Instance.ContainsMove(ref enemyMoves, kingPos))
                        return true;

                    if (castling && (GameInput.Instance.ContainsMove(ref enemyMoves, new(4, currentPiece.currentY)) ||
                        GameInput.Instance.ContainsMove(ref enemyMoves, new((currentPiece.currentX > 4) ? 5 : 3, currentPiece.currentY)))){
                        return true;
                    }

                }

        return false;
    }
}
