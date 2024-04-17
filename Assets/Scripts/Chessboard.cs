using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    [Header("Art Stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathScale = 0.7f;
    [SerializeField] private float deathSpacing = 0.2f;

    [Header("Prefabs and Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    // Logic
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private Piece[,] pieces;
    private Piece currentPiece;

    private List<Piece> deadWhites = new List<Piece>();
    private List<Piece> deadBlacks = new List<Piece>();

    public void Awake() 
    {
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        SpawnAllPieces();
        PositionAllPieces();
    }

    private void Update()
    {
        if (!currentCamera) 
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover")))
        {
            //get indexes of hit tile
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");

                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //Is left mouse button clicked
            if (Input.GetMouseButtonDown(0)) 
            {
                if (currentPiece != null) 
                {
                    MovePiece(currentPiece, currentHover.x, currentHover.y);
                }
                else if (pieces[currentHover.x, currentHover.y] != null) 
                {
                    currentPiece = pieces[currentHover.x, currentHover.y];
                }
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
        }
    }

    //Generate the board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;
        

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject($"X: {x}, Y: {y}");
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x+1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y+1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");

        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    //Operation
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);

        return -Vector2Int.one; //u slucaju greske
    }

    //Positioning
    private void PositionAllPieces() 
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (pieces[x, y] != null)
                    PositionSinglePiece(x, y, false);
    }

    private void PositionSinglePiece(int x, int y, bool animate = true)
    {
        pieces[x, y].currentX = x;
        pieces[x, y].currentY = y;
        pieces[x, y].transform.position = GetTileCenter(x, y);
    }

    private Vector3 GetTileCenter(int x, int y) 
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) + new Vector3(tileSize/2, 0, tileSize/2) - bounds;
    }

    private void MovePiece(Piece piece, int x, int y) 
    {
        if (pieces[x, y] != null)
        {
            Piece target = pieces[x, y];
            if (target.team == piece.team)
                return;

            if (target.type != PieceType.King) 
            {
                target.transform.localScale = Vector3.one * deathScale;
                if (target.team == TeamColor.White) 
                {
                    target.transform.position = GetTileCenter(8, -1) + Vector3.forward * deadWhites.Count * deathSpacing;
                    deadWhites.Add(target);
                }
                else
                {
                    target.transform.position = GetTileCenter(-1, 8) + Vector3.back * deadBlacks.Count * deathSpacing;
                    deadBlacks.Add(target);
                }
            }
        }

        pieces[piece.currentX, piece.currentY] = null;
        pieces[x, y] = piece;
        PositionSinglePiece(x, y);

        currentPiece = null;
    }

    //Spawning
    private void SpawnAllPieces()
    {
        pieces = new Piece[TILE_COUNT_X, TILE_COUNT_Y];

        //White
        pieces[0, 0] = SpawnSinglePiece(PieceType.Rook, TeamColor.White);
        pieces[1, 0] = SpawnSinglePiece(PieceType.Knight, TeamColor.White);
        pieces[2, 0] = SpawnSinglePiece(PieceType.Bishop, TeamColor.White);
        pieces[3, 0] = SpawnSinglePiece(PieceType.Queen, TeamColor.White);
        pieces[4, 0] = SpawnSinglePiece(PieceType.King, TeamColor.White);
        pieces[5, 0] = SpawnSinglePiece(PieceType.Bishop, TeamColor.White);
        pieces[6, 0] = SpawnSinglePiece(PieceType.Knight, TeamColor.White);
        pieces[7, 0] = SpawnSinglePiece(PieceType.Rook, TeamColor.White);

        for(int i = 0; i < TILE_COUNT_X; i++)
            pieces[i, 1] = SpawnSinglePiece(PieceType.Pawn, TeamColor.White);

        //Black
        pieces[0, 7] = SpawnSinglePiece(PieceType.Rook, TeamColor.Black);
        pieces[1, 7] = SpawnSinglePiece(PieceType.Knight, TeamColor.Black);
        pieces[2, 7] = SpawnSinglePiece(PieceType.Bishop, TeamColor.Black);
        pieces[3, 7] = SpawnSinglePiece(PieceType.King, TeamColor.Black);
        pieces[4, 7] = SpawnSinglePiece(PieceType.Queen, TeamColor.Black);
        pieces[5, 7] = SpawnSinglePiece(PieceType.Bishop, TeamColor.Black);
        pieces[6, 7] = SpawnSinglePiece(PieceType.Knight, TeamColor.Black);
        pieces[7, 7] = SpawnSinglePiece(PieceType.Rook, TeamColor.Black);

        for (int i = 0; i < TILE_COUNT_X; i++)
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
}


