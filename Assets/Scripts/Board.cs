using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Art stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.1f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.3f;
    [SerializeField] private float deathChessmanSpaceBetween = 0.3f;
    [SerializeField] private float dragOffset = 1f;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    

    private Chessman[,] chessmans;
    private Chessman currentlyPicked;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<Chessman> deadWhites = new List<Chessman>();
    private List<Chessman> deadBlacks = new List<Chessman>();
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
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "HighlightTile")))
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
            else
            {
                // First time
                if (ContainsValidMove(ref availableMoves, currentHover))
                {
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("HighlightTile");
                    tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = highlightMaterial;
                }
                else
                {
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                    tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = tileMaterial;
                }
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                tiles[hitPosition.x, hitPosition.y].GetComponent<MeshRenderer>().material = hoverMaterial;
            }

            if(Input.GetMouseButtonDown(0)) {
                if(chessmans[hitPosition.x, hitPosition.y] != null) {
                    if (chessmans[hitPosition.x, hitPosition.y].team == turn) {
                        currentlyPicked = chessmans[hitPosition.x, hitPosition.y];
                        availableMoves = currentlyPicked.GetAvailableMoves(ref chessmans, WIDTH);
                        HighlightTiles();
                    }
                }
            }

            if (currentlyPicked != null && Input.GetMouseButtonUp(0)) {
                Vector2Int previousPosition = new Vector2Int(currentlyPicked.currentColumn, currentlyPicked.currentRow);
                bool isValidMove = MoveTo(currentlyPicked, hitPosition.x, hitPosition.y);
                if (!isValidMove) {
                    currentlyPicked.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                }
                currentlyPicked = null;
                RemoveHighlightTiles();
            }
        }
        else
        {
            if (currentHover != INVALID_HOVER)
            {
                if (ContainsValidMove(ref availableMoves, currentHover)) {
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("HighlightTile");
                    tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = highlightMaterial;
                } else {
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                    tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = tileMaterial;
                }
                currentHover = INVALID_HOVER;
            }
            if (currentlyPicked != null && Input.GetMouseButtonUp(0))
            {
                currentlyPicked.SetPosition(GetTileCenter(currentlyPicked.currentColumn, currentlyPicked.currentRow));
                currentlyPicked = null;
                RemoveHighlightTiles();
            }
        }

        if (currentlyPicked) {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance)) {
                currentlyPicked.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }
        }
    }

    private Vector3 GetTileCenter(int col, int row)
    {
        return new Vector3(col * tileSize, yOffset, row * tileSize) - bounds + centerChessmanOffset;
    }

    private bool MoveTo(Chessman srcChessman, int col, int row) {
        if(!ContainsValidMove(ref availableMoves, new Vector2(col, row))) {
            return false;
        }
        
        Vector2Int previousPosotion = new Vector2Int(srcChessman.currentRow, srcChessman.currentColumn);
        if (chessmans[col, row] != null) {
            Chessman desChessman = chessmans[col, row];
            if (srcChessman.team == desChessman.team) {
                return false;
            } else {
                if (desChessman.team == ChessmanTeam.White) {
                    deadBlacks.Add(desChessman);
                    desChessman.SetLocalScale(Vector3.one * deathSize);
                    desChessman.SetPosition(
                        new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + (Vector3.forward * deathChessmanSpaceBetween) * deadWhites.Count);
                }
                else {
                    deadWhites.Add(desChessman);
                    desChessman.SetLocalScale(Vector3.one * deathSize);
                    desChessman.SetPosition(
                        new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + (Vector3.back * deathChessmanSpaceBetween) * deadBlacks.Count);
                }
                if (desChessman.type == ChessmanType.King) {
                    CheckMate(desChessman.team);
                }
            }
        }
        chessmans[col, row] = srcChessman;
        chessmans[previousPosotion.x, previousPosotion.y] = null;
        PositionSingleChessman(col, row, false, true);
        turn = (turn == ChessmanTeam.White) ? ChessmanTeam.Black : ChessmanTeam.White;
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
                tiles[col, row] = GenerateSingleTile(tileSize, col, row);
    }

    private GameObject GenerateSingleTile(float tileSize, int col, int row)
    {
        GameObject tileObject = new GameObject(string.Format("Column:{0}, Row:{1}", col, row));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(col * tileSize, yOffset, row * tileSize) - bounds;
        vertices[1] = new Vector3(col * tileSize, yOffset, (row + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((col + 1) * tileSize, yOffset, row * tileSize) - bounds;
        vertices[3] = new Vector3((col + 1) * tileSize, yOffset, (row + 1) * tileSize) - bounds;

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
        chessmans[1, 0] = SpawnSingleChessman(ChessmanType.Knight, ChessmanTeam.White);
        chessmans[2, 0] = SpawnSingleChessman(ChessmanType.Bishop, ChessmanTeam.White);
        chessmans[3, 0] = SpawnSingleChessman(ChessmanType.King, ChessmanTeam.White);
        chessmans[4, 0] = SpawnSingleChessman(ChessmanType.Queen, ChessmanTeam.White);
        chessmans[5, 0] = SpawnSingleChessman(ChessmanType.Bishop, ChessmanTeam.White);
        chessmans[6, 0] = SpawnSingleChessman(ChessmanType.Knight, ChessmanTeam.White);
        chessmans[7, 0] = SpawnSingleChessman(ChessmanType.Rook, ChessmanTeam.White);
        for (int i = 0; i < WIDTH; i++) {
            chessmans[i, 1] = SpawnSingleChessman(ChessmanType.Pawn, ChessmanTeam.White);
        }

        chessmans[0, 7] = SpawnSingleChessman(ChessmanType.Rook, ChessmanTeam.Black);
        chessmans[1, 7] = SpawnSingleChessman(ChessmanType.Knight, ChessmanTeam.Black);
        chessmans[2, 7] = SpawnSingleChessman(ChessmanType.Bishop, ChessmanTeam.Black);
        chessmans[3, 7] = SpawnSingleChessman(ChessmanType.Queen, ChessmanTeam.Black);
        chessmans[4, 7] = SpawnSingleChessman(ChessmanType.King, ChessmanTeam.Black);
        chessmans[5, 7] = SpawnSingleChessman(ChessmanType.Bishop, ChessmanTeam.Black);
        chessmans[6, 7] = SpawnSingleChessman(ChessmanType.Knight, ChessmanTeam.Black);
        chessmans[7, 7] = SpawnSingleChessman(ChessmanType.Rook, ChessmanTeam.Black);
        for (int i = 0; i < WIDTH; i++) {
            chessmans[i, 6] = SpawnSingleChessman(ChessmanType.Pawn, ChessmanTeam.Black);
        }
    }

    private Chessman SpawnSingleChessman(ChessmanType type, ChessmanTeam team) {
        Chessman chessman = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Chessman>();
        if (chessman != null) {
            chessman.gameObject.name = string.Format("{0} {1}", team, type);
            chessman.type = type;
            chessman.team = team;
            chessman.GetComponent<MeshRenderer>().material = teamMaterials[(int)team];
        }

        return chessman;
    }

    private void PositionAllChessmans() {
        for (int row = 0; row < WIDTH; row++)
            for (int col = 0; col < WIDTH; col++)
                if (chessmans[col, row] != null)
                    PositionSingleChessman(col, row, true);
    }

    private void PositionSingleChessman(int col, int row, bool force = false, bool isMove = false) {
        chessmans[col, row].currentRow = row;
        chessmans[col, row].currentColumn = col;
        chessmans[col, row].SetPosition(GetTileCenter(col, row), force, isMove);
    }

    private void HighlightTiles() {
        for (int i = 0; i < availableMoves.Count; i++) {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("HighlightTile");
            tiles[availableMoves[i].x, availableMoves[i].y].GetComponent<MeshRenderer>().material = highlightMaterial;
        }
    }

    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
            tiles[availableMoves[i].x, availableMoves[i].y].GetComponent<MeshRenderer>().material = tileMaterial;
        }
        availableMoves.Clear();
    }

    private void CheckMate(ChessmanTeam team) {
        Debug.Log(string.Format("{0} Victory", team));
    }

    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos) {
        for (int i =0; i < moves.Count; i++) {
            if (moves[i].x == pos.x && moves[i].y == pos.y) {
                return true;
            }
        }
        return false;
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
