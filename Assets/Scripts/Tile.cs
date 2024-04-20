using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private static Material tileMaterial;
    private static float tileSize = 0.3f;
    public static float yOffset = 0.185f;

    public static GameObject[,] tiles;
    private static Vector3 boardCenter = Vector3.zero;
    private static Vector3 bounds;

    // Generate tiles
    public static void GenerateAllTiles(int tileCount, Transform transform)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCount / 2) * tileSize, 0, (tileCount / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCount, tileCount];
        for (int x = 0; x < tileCount; x++)
            for (int y = 0; y < tileCount; y++)
                tiles[x, y] = GenerateSingleTile(x, y, transform);
    }
    private static GameObject GenerateSingleTile(int x, int y, Transform transform)
    {
        GameObject tileObject = new GameObject($"X: {x}, Y: {y}");
        tileObject.transform.parent = transform;

        tileMaterial = Resources.Load("Materials/TransparentTiles", typeof(Material)) as Material;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");

        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    // Highlight Tiles
    public static void HighlightMoves(ref List<Vector2Int> validMoves)
    {
        for (int i = 0; i < validMoves.Count; i++)
            tiles[validMoves[i].x, validMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
    }
    public static void RemoveHighlights(ref List<Vector2Int> validMoves)
    {
        for (int i = 0; i < validMoves.Count; i++)
            tiles[validMoves[i].x, validMoves[i].y].layer = LayerMask.NameToLayer("Tile");

        validMoves.Clear();
    }

    // Utility
    public static Vector2Int LookupTileIndex(GameObject hitInfo, int tileCount)
    {
        for (int x = 0; x < tileCount; x++)
            for (int y = 0; y < tileCount; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);

        return -Vector2Int.one; // Error
    }
    public static Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }
}
