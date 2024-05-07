using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameInput : NetworkBehaviour {
    public static GameInput Instance { get; private set; }

    private Camera currentCamera;
    private Vector2Int currentHover;

    private RaycastHit info;
    private Ray ray;
    private Vector2Int hitPosition;

    private float distance;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        if (!TeamPromotion.Instance.isPromotionUIActive && !GameManager.Instance.gameOverUIActive && !Chessboard.opponentDisconnectUIActive) {
            UpdateCamera();
            UpdateInput(ref Chessboard.validMoves, ref PieceManager.Instance.pieces, ref PieceManager.Instance.currentPiece, GameManager.Instance.isWhiteTurn, TileManager.TILE_COUNT);
            UpdatePieceAnimation(ref PieceManager.Instance.currentPiece);
        }
    }

    private void UpdateCamera() {
        if (!currentCamera) {
            currentCamera = Camera.main;
            return;
        }
    }

    public void UpdateInput(ref List<Vector2Int> validMoves, ref Piece[,] pieces, ref Piece currentPiece, bool isWhiteTurn, int tileCount) {
        ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight"))) {
            // Get indexes of hit tile
            hitPosition = TileManager.Instance.LookupTileIndex(info.transform.gameObject, tileCount);

            // If we're hovering a tile after not hovering any tiles
            if (currentHover == -Vector2Int.one) {
                currentHover = hitPosition;
                TileManager.Instance.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If we were already hovering a tile, change the previous one
            if (currentHover != hitPosition) {
                TileManager.Instance.tiles[currentHover.x, currentHover.y].layer = ContainsMove(ref validMoves, currentHover) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                TileManager.Instance.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // Is left mouse button is clicked
            if (Input.GetMouseButtonDown(0)) {
                // Releasing piece
                if (currentPiece != null) {
                    if (ContainsMove(ref validMoves, new Vector2Int(hitPosition.x, hitPosition.y))) {
                        PieceManager.Instance.MovePiece(new Vector2Int(currentPiece.currentX, currentPiece.currentY), new Vector2Int(hitPosition.x, hitPosition.y));
                    } else {
                        currentPiece.SetPosition(TileManager.Instance.GetTileCenter(currentPiece.currentX, currentPiece.currentY));
                        currentPiece = null;
                        TileManager.Instance.RemoveHighlights(ref validMoves);
                    }
                }
                // Selecting piece
                else if (pieces[hitPosition.x, hitPosition.y] != null) {
                    // Is it our turn?
                    if (IsMyTurn(pieces, isWhiteTurn)) {
                        currentPiece = pieces[hitPosition.x, hitPosition.y];

                        // Get a list of valid moves
                        validMoves = currentPiece.GetValidMoves(ref pieces, tileCount, Chessboard.lastMove);

                        // Remove moves that put us in check
                        Chessboard.PreventMove();

                        TileManager.Instance.HighlightMoves(ref validMoves);
                    }
                }
            }
        } else {
            if (currentHover != -Vector2Int.one) {
                TileManager.Instance.tiles[currentHover.x, currentHover.y].layer = ContainsMove(ref validMoves, currentHover) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if (currentPiece != null && Input.GetMouseButtonDown(0)) {
                currentPiece.SetPosition(TileManager.Instance.GetTileCenter(currentPiece.currentX, currentPiece.currentY));
                currentPiece = null;
                TileManager.Instance.RemoveHighlights(ref validMoves);
            }
        }
    }

    private void UpdatePieceAnimation(ref Piece currentPiece) {
        if (currentPiece != null) {
            Plane horizontalPlane = new(Vector3.up, Vector3.up * 0.1f);
            distance = 0.0f;

            if (horizontalPlane.Raycast(ray, out distance))
                currentPiece.SetPosition(ray.GetPoint(distance) + Vector3.up * 0.2f);
        }
    }

    public bool ContainsMove(ref List<Vector2Int> moves, Vector2Int pos) {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;

        return false;
    }

    private bool IsMyTurn(Piece[,] pieces, bool isWhiteTurn) {
        if (GameMultiplayer.playMultiplayer && GameMultiplayer.Instance.playerDataNetworkList.Count == 2) {
            return (pieces[hitPosition.x, hitPosition.y].team == TeamColor.White && isWhiteTurn && GameMultiplayer.Instance.GetPlayerData().colorId == 0) ||
                   (pieces[hitPosition.x, hitPosition.y].team == TeamColor.Black && !isWhiteTurn && GameMultiplayer.Instance.GetPlayerData().colorId == 1);
        } else {
            return (pieces[hitPosition.x, hitPosition.y].team == TeamColor.White && isWhiteTurn) ||
                   (pieces[hitPosition.x, hitPosition.y].team == TeamColor.Black && !isWhiteTurn);
        }
    }
}
