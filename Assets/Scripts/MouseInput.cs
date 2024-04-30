using System.Collections.Generic;
using UnityEngine;

public class MouseInput : MonoBehaviour
{
    private static Camera currentCamera;
    private static Vector2Int currentHover;

    private static RaycastHit info;
    private static Ray ray;

    private static float distance;

    public static void UpdateInput(ref List<Vector2Int> validMoves, ref Piece[,] pieces, ref Piece currentPiece, bool isWhiteTurn, int tileCount)
    {
        UpdateCamera();

        ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            // Get indexes of hit tile
            Vector2Int hitPosition = Tile.LookupTileIndex(info.transform.gameObject, tileCount);

            // If we're hovering a tile after not hovering any tiles
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                Tile.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If we were already hovering a tile, change the previous one
            if (currentHover != hitPosition)
            {
                Tile.tiles[currentHover.x, currentHover.y].layer = IsValidMove(ref validMoves, currentHover) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                Tile.tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // Is left mouse button is clicked
            if (Input.GetMouseButtonDown(0))
            {
                // Releasing piece
                if (currentPiece != null)
                {
                    if (IsValidMove(ref validMoves, new Vector2Int(hitPosition.x, hitPosition.y)))
                    {
                        Chessboard.MovePiece(currentPiece, hitPosition.x, hitPosition.y);
                    }
                    else
                    {
                        currentPiece.SetPosition(Tile.GetTileCenter(currentPiece.currentX, currentPiece.currentY));
                        currentPiece = null;
                        Tile.RemoveHighlights(ref validMoves);
                    }
                }
                // Selecting piece
                else if (pieces[hitPosition.x, hitPosition.y] != null)
                {
                    // Is it our turn?
                    if ((pieces[hitPosition.x, hitPosition.y].team == TeamColor.White && isWhiteTurn) ||
                        (pieces[hitPosition.x, hitPosition.y].team == TeamColor.Black && !isWhiteTurn))
                    {
                        currentPiece = pieces[hitPosition.x, hitPosition.y];

                        // Get a list of valid moves
                        validMoves = currentPiece.GetValidMoves(ref pieces, tileCount, Chessboard.lastMove);

                        // Remove moves that put us in check
                        Chessboard.PreventMove();

                        Tile.HighlightMoves(ref validMoves);
                    }
                }
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                Tile.tiles[currentHover.x, currentHover.y].layer = IsValidMove(ref validMoves, currentHover) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if (currentPiece != null && Input.GetMouseButtonDown(0))
            {
                currentPiece.SetPosition(Tile.GetTileCenter(currentPiece.currentX, currentPiece.currentY));
                currentPiece = null;
                Tile.RemoveHighlights(ref validMoves);
            }
        }

        // Selected piece animation
        if (currentPiece != null)
        {
            Plane horizontalPlane = new(Vector3.up, Vector3.up * Tile.yOffset);
            distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
                currentPiece.SetPosition(ray.GetPoint(distance) + Vector3.up * 0.2f);
        }
    }

    private static void UpdateCamera()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }
    }

    public static bool IsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;

        return false;
    }
}
