using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.1f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    

    public Chessman[,] chessmans;
    public Chessman currentlyPicked;
    private ChessmanTeam turn;
    private const int WIDTH = 8;
    private Vector2Int INVALID_HOVER = -Vector2Int.one;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private Vector3 centerChessmanOffset;

    private void Awake()
    {
        turn = ChessmanTeam.White;
        GenerateTiles(tileSize, WIDTH);
        SpawnAllChessmans();
        PositionAllChessmans();
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
            Vector2Int hitPosition = LoockupTileIndex(info.transform.gameObject);
            if (currentHover == INVALID_HOVER) {
                // First time
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                tiles[hitPosition.x, hitPosition.y].GetComponent<MeshRenderer>().material = hoverMaterial;
            }
            // If we were already hovering a til, change the previous one
            // if (currentHover == hitPosition)
            else
            {
                // First time
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = tileMaterial;
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                tiles[hitPosition.x, hitPosition.y].GetComponent<MeshRenderer>().material = hoverMaterial;
            }

            if(Input.GetMouseButtonDown(0)) {
                if(chessmans[hitPosition.x, hitPosition.y] != null) {
                    
                    if (true) {
                        currentlyPicked = chessmans[hitPosition.x, hitPosition.y];
                    }
                }
            }

            if (currentlyPicked != null && Input.GetMouseButtonUp(0)) {
                Vector2Int previousPosition = new Vector2Int(currentlyPicked.currentColumn, currentlyPicked.currentRow);
                bool isValidMove = MoveTo(currentlyPicked, hitPosition.x, hitPosition.y);
                if (!isValidMove) {
                    currentlyPicked.transform.position = GetTileCenter(previousPosition.x, previousPosition.y);
                    currentlyPicked = null;
                }
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

    private bool MoveTo(Chessman chessman, int col, int row) {
        Vector2Int previousPosotion = new Vector2Int(chessman.currentRow, chessman.currentColumn);
        chessmans[row, col] = chessman;
        chessmans[previousPosotion.x, previousPosotion.y] = null;
        PositionSingleChessman(row, col);
        return true;
    }

    private void GenerateTiles(float tileSize, int width)
    {
        yOffset += transform.position.y;
        bounds = new Vector3(width / 2 * tileSize, 0, width / 2 * tileSize) + boardCenter;
        centerChessmanOffset = new Vector3(0.5f, 0, 0.5f);
        tiles = new GameObject[width, width];
        for (int row = 0; row < width; row++)
            for (int col = 0; col < width; col++)
                tiles[row, col] = GenerateSingleTile(tileSize, row, col);
    }

    private GameObject GenerateSingleTile(float tileSize, int row, int column)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", column, row));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(column * tileSize, yOffset, row * tileSize) - bounds;
        vertices[1] = new Vector3(column * tileSize, yOffset, (row + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((column + 1) * tileSize, yOffset, row * tileSize) - bounds;
        vertices[3] = new Vector3((column + 1) * tileSize, yOffset, (row + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    private void SpawnAllChessmans() {
        chessmans = new Chessman[WIDTH, WIDTH];
        chessmans[0, 0] = SpawnSingleChessman(ChessmanType.Rook, ChessmanTeam.White);
        chessmans[0, 1] = SpawnSingleChessman(ChessmanType.Knight, ChessmanTeam.White);
        chessmans[0, 2] = SpawnSingleChessman(ChessmanType.Bishop, ChessmanTeam.White);
        chessmans[0, 3] = SpawnSingleChessman(ChessmanType.King, ChessmanTeam.White);
        chessmans[0, 4] = SpawnSingleChessman(ChessmanType.Queen, ChessmanTeam.White);
        chessmans[0, 5] = SpawnSingleChessman(ChessmanType.Bishop, ChessmanTeam.White);
        chessmans[0, 6] = SpawnSingleChessman(ChessmanType.Knight, ChessmanTeam.White);
        chessmans[0, 7] = SpawnSingleChessman(ChessmanType.Rook, ChessmanTeam.White);
        for (int i = 0; i < WIDTH; i++) {
            chessmans[1, i] = SpawnSingleChessman(ChessmanType.Pawn, ChessmanTeam.White);
        }

        chessmans[7, 0] = SpawnSingleChessman(ChessmanType.Rook, ChessmanTeam.Black);
        chessmans[7, 1] = SpawnSingleChessman(ChessmanType.Knight, ChessmanTeam.Black);
        chessmans[7, 2] = SpawnSingleChessman(ChessmanType.Bishop, ChessmanTeam.Black);
        chessmans[7, 3] = SpawnSingleChessman(ChessmanType.Queen, ChessmanTeam.Black);
        chessmans[7, 4] = SpawnSingleChessman(ChessmanType.King, ChessmanTeam.Black);
        chessmans[7, 5] = SpawnSingleChessman(ChessmanType.Bishop, ChessmanTeam.Black);
        chessmans[7, 6] = SpawnSingleChessman(ChessmanType.Knight, ChessmanTeam.Black);
        chessmans[7, 7] = SpawnSingleChessman(ChessmanType.Rook, ChessmanTeam.Black);
        for (int i = 0; i < WIDTH; i++) {
            chessmans[6, i] = SpawnSingleChessman(ChessmanType.Pawn, ChessmanTeam.Black);
        }
    }

    private Chessman SpawnSingleChessman(ChessmanType type, ChessmanTeam team) {
        Chessman chessman = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Chessman>();
        if (chessman != null) {
            chessman.type = type;
            chessman.team = team;
            chessman.GetComponent<MeshRenderer>().material = teamMaterials[(int)team];
        }

        return chessman;
    }

    private void PositionAllChessmans() {
        for (int row = 0; row < WIDTH; row++)
            for (int col = 0; col < WIDTH; col++)
                if (chessmans[row, col] != null)
                    PositionSingleChessman(row, col, true);
    }

    private void PositionSingleChessman(int row, int col, bool force = false) {
        chessmans[row, col].currentRow = row;
        chessmans[row, col].currentColumn = col;
        chessmans[row, col].transform.position = new Vector3(col * tileSize, yOffset, row * tileSize) - bounds + centerChessmanOffset;
    }

    private Vector3 GetTileCenter(int x, int y) {
        return new Vector3(1.5f, 0, 1.5f);
    }

    private Vector2Int LoockupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < WIDTH; x++)
            for (int y = 0; y < WIDTH; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);
        return -Vector2Int.one; // Invalid
    }
}
