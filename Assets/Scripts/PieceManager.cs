using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PieceManager : NetworkBehaviour {

    private const float DEATH_SCALE = 0.6f;
    private const float DEATH_SPACING = 0.15f;

    public static PieceManager Instance { get; private set; }

    public event EventHandler OnMoveSelfSound;
    public event EventHandler OnMoveOpponentSound;
    public event EventHandler OnGameEndSound;
    public event EventHandler OnCaptureSound;
    public event EventHandler OnCastleSound;
    public event EventHandler OnMoveCheckSound;
    public event EventHandler OnPromotionSound;

    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;
    [SerializeField] private Transform chessboardTransform;

    public Piece[,] pieces;
    public Piece currentPiece;
    private List<Piece> deadWhites = new List<Piece>();
    private List<Piece> deadBlacks = new List<Piece>();

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SpawnAllPieces();
        PositionAllPieces();
    }

    // Spawning the pieces
    public void SpawnAllPieces() {
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

    public Piece SpawnSinglePiece(PieceType type, TeamColor team) {
        Transform pieceTransform = Instantiate(prefabs[(int)type - 1], chessboardTransform).GetComponent<Transform>();
        Piece piece = pieceTransform.GetComponent<Piece>();

        piece.type = type;
        piece.team = team;
        piece.GetComponentInChildren<MeshRenderer>().material = teamMaterials[(int)team - 1];

        return piece;
    }

    // Positioning
    public void PositionAllPieces() {
        for (int x = 0; x < TileManager.TILE_COUNT; x++)
            for (int y = 0; y < TileManager.TILE_COUNT; y++)
                if (pieces[x, y] != null)
                    PositionSinglePiece(x, y, false);
    }

    public void PositionSinglePiece(int x, int y, bool animate = true) {
        pieces[x, y].currentX = x;
        pieces[x, y].currentY = y;
        pieces[x, y].SetPosition(TileManager.Instance.GetTileCenter(x, y), animate);
    }

    // Move piece synchronization
    public void MovePiece(Vector2Int oldPosition, Vector2Int newPosition) {
        MovePieceServerRpc(oldPosition, newPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void MovePieceServerRpc(Vector2Int oldPosition, Vector2Int newPosition, ServerRpcParams serverRpcParams = default) {
        MovePieceClientRpc(oldPosition, newPosition, serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void MovePieceClientRpc(Vector2Int oldPosition, Vector2Int newPosition, ulong senderClientId) {
        bool soundPlayed = false;
        bool pieceEaten = false;
        
        Piece piece = pieces[oldPosition.x, oldPosition.y];
        int x = newPosition.x, y = newPosition.y;

        if (piece.type == PieceType.Pawn && Mathf.Abs(piece.currentX - x) == 1 && Mathf.Abs(piece.currentY - y) == 1 && pieces[x, y] == null)
            Chessboard.enPassant = true;

        int y2 = y;
        if (Chessboard.enPassant)
            y2 += ((piece.team == TeamColor.White) ? -1 : 1);

        // Is there another piece on the target position?
        if (pieces[x, y2] != null) {
            Piece target = pieces[x, y2];

            target.SetScale(Vector3.one * DEATH_SCALE);
            if (target.team == TeamColor.White) {
                target.SetPosition(TileManager.Instance.GetTileCenter(9, -1) + deadWhites.Count * DEATH_SPACING * Vector3.forward);
                deadWhites.Add(target);
            } else {
                target.SetPosition(TileManager.Instance.GetTileCenter(-2, 8) + deadBlacks.Count * DEATH_SPACING * Vector3.back);
                deadBlacks.Add(target);
            }

            pieceEaten = true;
        }

        // Castling
        if (piece.type == PieceType.King && Mathf.Abs(x - piece.currentX) == 2) {
            Piece rook = pieces[(piece.currentX > x) ? 0 : 7, piece.currentY];

            pieces[(piece.currentX > x) ? 3 : 5, piece.currentY] = rook;
            PositionSinglePiece((piece.currentX > x) ? 3 : 5, piece.currentY);
            pieces[(piece.currentX > x) ? 0 : 7, piece.currentY] = null;

            soundPlayed = true;
            OnCastleSound?.Invoke(this, EventArgs.Empty);
        }

        Chessboard.lastMove = new Tuple<Vector2Int, Vector2Int>(new(piece.currentX, piece.currentY), new(x, y));
        pieces[x, y] = piece;
        pieces[piece.currentX, piece.currentY] = null;
        if (Chessboard.enPassant)
            pieces[x, y2] = null;

        PositionSinglePiece(x, y);

        if (TeamPromotion.Instance.CheckForPromotion()) {
            soundPlayed = true;
            OnPromotionSound?.Invoke(this, EventArgs.Empty);
        }

        if (piece.type == PieceType.Rook || piece.type == PieceType.King) {
            piece.moved = true;
        }

        GameManager.Instance.isWhiteTurn = !GameManager.Instance.isWhiteTurn;
        Chessboard.enPassant = false;

        currentPiece = null;

        TileManager.Instance.RemoveHighlights(ref Chessboard.validMoves);

        int checkForCheckmateResult = GameManager.Instance.CheckForCheckmate(piece.team == TeamColor.White ? TeamColor.Black : TeamColor.White);

        currentPiece = null;

        if (!soundPlayed) {
            if (checkForCheckmateResult == 1) {
                OnGameEndSound?.Invoke(this, EventArgs.Empty);
            } else if (checkForCheckmateResult == 2) {
                OnMoveCheckSound?.Invoke(this, EventArgs.Empty);
            } else if (pieceEaten) {
                OnCaptureSound?.Invoke(this, EventArgs.Empty);
            } else if (senderClientId == NetworkManager.Singleton.LocalClientId) {
                OnMoveSelfSound?.Invoke(this, EventArgs.Empty);
            } else {
                OnMoveOpponentSound?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
