using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private Material hoverMaterial;
    public GameObject chessmanPrefab;

    private const int HEIGHT = 8;
    private const int WIDTH = 8;
    private Vector2Int INVALID_HOVER = -Vector2Int.one;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;

    private void Awake()
    {
        GenerateTiles(1, WIDTH, HEIGHT);
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
            // Get the indexes if the tile hitted
            Vector2Int hitPostion = LoockupTileIndex(info.transform.gameObject);
            if (currentHover == INVALID_HOVER)
            {
                // First time
                currentHover = hitPostion;
                tiles[hitPostion.x, hitPostion.y].layer = LayerMask.NameToLayer("Hover");
                tiles[hitPostion.x, hitPostion.y].GetComponent<MeshRenderer>().material = hoverMaterial;
            }

            // If we were already hovering a til, change the previous one
            // if (currentHover == hitPostion)
            else
            {
                // First time
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = tileMaterial;
                currentHover = hitPostion;
                tiles[hitPostion.x, hitPostion.y].layer = LayerMask.NameToLayer("Hover");
                tiles[hitPostion.x, hitPostion.y].GetComponent<MeshRenderer>().material = hoverMaterial;
            }
        }
        else
        {
            if (currentHover != INVALID_HOVER)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = tileMaterial;
                currentHover = INVALID_HOVER;
            }
        }

    }

    private void GenerateTiles(float tileSize, int totalTileX, int totalTileY)
    {
        tiles = new GameObject[totalTileX, totalTileY];
        for (int x = 0; x < totalTileX; x++)
            for (int y = 0; y < totalTileY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }

    private GameObject GenerateSingleTile(float tileSize, int posX, int posY)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", posX, posY));
        tileObject.transform.parent = transform;
        // tileObject.gameObject.transform.position = new Vector3(posX, 0, posY);

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(posX * tileSize, 0, posY * tileSize);
        vertices[1] = new Vector3(posX * tileSize, 0, (posY + 1) * tileSize);
        vertices[2] = new Vector3((posX + 1) * tileSize, 0, posY * tileSize);
        vertices[3] = new Vector3((posX + 1) * tileSize, 0, (posY + 1) * tileSize);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    private Vector2Int LoockupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < WIDTH; x++)
            for (int y = 0; y < HEIGHT; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);
        return -Vector2Int.one; // Invalid
    }
}
