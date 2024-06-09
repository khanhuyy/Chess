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
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();
    private ChessmanTeam turn;
    private const int WIDTH = 8;
    private Vector2Int INVALID_HOVER = -Vector2Int.one;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private Vector3 centerChessmanOffset;
    private SpecialMove specialMove;

    private ChessmanTeam GetNextTurnTeam() {
        return turn == ChessmanTeam.White ? ChessmanTeam.Black : ChessmanTeam.White;
    }
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
                if (ContainsValidMove(ref availableMoves, currentHover)) {
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("HighlightTile");
                    tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = highlightMaterial;
                } else {
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                    tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = tileMaterial;
                }
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                tiles[hitPosition.x, hitPosition.y].GetComponent<MeshRenderer>().material = hoverMaterial;
            }

            // picked chessman
            if(Input.GetMouseButtonDown(0)) {
                if(chessmans[hitPosition.x, hitPosition.y] != null) {
                    if (chessmans[hitPosition.x, hitPosition.y].team == turn) {
                        currentlyPicked = chessmans[hitPosition.x, hitPosition.y];
                        availableMoves = currentlyPicked.GetAvailableMoves(ref chessmans, WIDTH);
                        specialMove = currentlyPicked.GetSpecialMoves(ref chessmans, ref moveList, ref availableMoves);
                        PreventCheck();
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

    private bool MoveTo(Chessman currentChessman, int col, int row) {
        if(!ContainsValidMove(ref availableMoves, new Vector2Int(col, row))) {
            return false;
        }
        
        Vector2Int previousPosotion = new Vector2Int(currentChessman.currentColumn, currentChessman.currentRow);
        if (chessmans[col, row] != null) {
            Chessman opponentChessman = chessmans[col, row];
            if (currentChessman.team == opponentChessman.team) {
                return false;
            } else {
                if (opponentChessman.team == ChessmanTeam.White) {
                    deadBlacks.Add(opponentChessman);
                    opponentChessman.SetLocalScale(Vector3.one * deathSize);
                    opponentChessman.SetPosition(
                        new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + (Vector3.forward * deathChessmanSpaceBetween) * deadWhites.Count);
                }
                else {
                    deadWhites.Add(opponentChessman);
                    opponentChessman.SetLocalScale(Vector3.one * deathSize);
                    opponentChessman.SetPosition(
                        new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + (Vector3.back * deathChessmanSpaceBetween) * deadBlacks.Count);
                }
                // not happened
                if (opponentChessman.type == ChessmanType.King) {
                    Victory(opponentChessman.team);
                }
            }
        }
        chessmans[col, row] = currentChessman;
        chessmans[previousPosotion.x, previousPosotion.y] = null;
        PositionSingleChessman(col, row, false, true);
        moveList.Add(new Vector2Int[] { previousPosotion, new Vector2Int(col, row) });
        SolveSpecialMove();
        if (CheckMate()) {
            Victory(turn);
        }
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
        chessmans[3, 7] = SpawnSingleChessman(ChessmanType.King, ChessmanTeam.Black);
        chessmans[4, 7] = SpawnSingleChessman(ChessmanType.Queen, ChessmanTeam.Black);
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

    private void SolveSpecialMove() {
        if (specialMove == SpecialMove.EnPassant) {
            var newMove = moveList[moveList.Count - 1];
            Chessman currentPawn = chessmans[newMove[1].x, newMove[1].y];
            var previousMove = moveList[moveList.Count - 2];
            Chessman opponentPawn = chessmans[previousMove[1].x, previousMove[1].y];
            if (currentPawn.currentColumn == opponentPawn.currentColumn) {
                if (opponentPawn.team == ChessmanTeam.White) {

                    deadWhites.Add(opponentPawn);
                    opponentPawn.SetLocalScale(Vector3.one * deathSize);
                    opponentPawn.SetPosition(
                        new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + (Vector3.forward * deathChessmanSpaceBetween) * deadWhites.Count);
                } else {
                    deadBlacks.Add(opponentPawn);
                    opponentPawn.SetLocalScale(Vector3.one * deathSize);
                    opponentPawn.SetPosition(
                        new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + (Vector3.back * deathChessmanSpaceBetween) * deadBlacks.Count);
                }
            }
            chessmans[opponentPawn.currentColumn, opponentPawn.currentRow] = null;
        }

        if (specialMove == SpecialMove.Promotion) {

            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            Chessman currentPawn = chessmans[lastMove[1].x, lastMove[1].y];
            if (currentPawn.type == ChessmanType.Pawn) {
                Debug.Log(currentPawn.initRow);
                // Debug.Log(currentPawn.curre);
                if (Mathf.Abs(currentPawn.currentRow - currentPawn.initRow) == 6) {
                    Chessman newQueen = SpawnSingleChessman(ChessmanType.Queen, turn);
                    Destroy(chessmans[lastMove[1].x, lastMove[1].y].gameObject);
                    chessmans[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSingleChessman(lastMove[1].x, lastMove[1].y, true);
                }
            }
        }

        if (specialMove == SpecialMove.Castling) {
            Vector2Int[] kingLastMove = moveList[moveList.Count - 1];
            // far
            if (kingLastMove[1].x - kingLastMove[0].x == 2) {
                Chessman rook = chessmans[WIDTH - 1, kingLastMove[1].y];
                chessmans[kingLastMove[1].x - 1, kingLastMove[1].y] = rook;
                PositionSingleChessman(kingLastMove[1].x - 1, kingLastMove[1].y);
                chessmans[WIDTH - 1, kingLastMove[1].y] = null;
            } else if (kingLastMove[0].x - kingLastMove[1].x == 2) { // near
                Chessman rook = chessmans[0, kingLastMove[1].y];
                chessmans[kingLastMove[1].x + 1, kingLastMove[1].y] = rook;
                PositionSingleChessman(kingLastMove[1].x + 1, kingLastMove[1].y);
                chessmans[0, kingLastMove[1].y] = null;
            }
        }
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

    private void PreventCheck() {
        Chessman targetKing = null;
        bool findTheKing = false;
        for (int col = 0; col < WIDTH; col++) {
            for (int row = 0; row < WIDTH; row++)
                if (chessmans[col, row] != null)
                    if (chessmans[col, row].type == ChessmanType.King)
                        if (chessmans[col, row].team == turn) {
                            targetKing = chessmans[col, row];
                            findTheKing = true;
                        }
            if (findTheKing) {
                break;
            }
        }
        SimulateMoveForSingleChessman(currentlyPicked, ref availableMoves, targetKing);
    }

    private void SimulateMoveForSingleChessman(Chessman cm, ref List<Vector2Int> moves, Chessman targetKing) {
        // Save the current values to reset after the function call
        int actualCol = cm.currentColumn;
        int actualRow = cm.currentRow;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        // Going throught all moves, simulate them and check if we're in check
        for (int i = 0; i < moves.Count; i++) {
            int simCol = moves[i].x;
            int simRow = moves[i].y;

            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentColumn, targetKing.currentRow);
            if (cm.type == ChessmanType.King) {
                kingPositionThisSim = new Vector2Int(simCol, simRow);
            }
            Chessman[,] simulation = new Chessman[WIDTH, WIDTH];
            List<Chessman> simAttackingChessman = new List<Chessman>();
            for (int col = 0; col < WIDTH; col++) {
                for (int row = 0; row < WIDTH; row++) {
                    simulation[col, row] = chessmans[col, row];
                    if (simulation[col, row] != null && simulation[col, row].team != cm.team) 
                        simAttackingChessman.Add(simulation[col, row]);
                }
            }
            // simulate that move were check the king
            simulation[actualCol, actualRow] = null;
            cm.currentColumn = simCol;
            cm.currentRow = simRow;
            simulation[simCol, simRow] = cm;
        
            // Did one of the chessman got taken down during simulation
            var deadChessman = simAttackingChessman.Find(c => c.currentColumn == simCol && c.currentRow == simRow);
            if (deadChessman != null)
                simAttackingChessman.Remove(deadChessman);

            // Get all the simulated attacking pieces moves
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int a= 0; a < simAttackingChessman.Count; a++) {
                var chessmanMoves = simAttackingChessman[a].GetAvailableMoves(ref simulation, WIDTH);
                for (int b= 0; b < chessmanMoves.Count; b++) {
                    simMoves.Add(chessmanMoves[b]);
                }
            }

            // Is the king in trouble? if so, remove the move
            if (ContainsValidMove(ref simMoves, kingPositionThisSim)) {
                movesToRemove.Add(moves[i]);
            }
        
            // Restore the actual chessman data
            cm.currentColumn = actualCol;
            cm.currentRow = actualRow;
        }
        
        // Remove from the current available move list
        for (int i = 0; i < movesToRemove.Count; i++) {
            moves.Remove(movesToRemove[i]);
        }
    }

    private bool CheckMate() {
        var lastMove = moveList[moveList.Count - 1];
        ChessmanTeam defendingTeam = GetNextTurnTeam();

        List<Chessman> attackingChessmans = new List<Chessman>();
        List<Chessman> defendingChessmans = new List<Chessman>();
        Chessman defendingKing = null;
        for (int col = 0; col < WIDTH; col++) {
            for (int row = 0; row < WIDTH; row++) {
                if (chessmans[col, row] != null) {
                    if (chessmans[col, row].team == defendingTeam) {
                        defendingChessmans.Add(chessmans[col, row]);
                        if (chessmans[col, row].type == ChessmanType.King) {
                            defendingKing = chessmans[col, row];
                        }
                    } else {
                        attackingChessmans.Add(chessmans[col, row]);
                    }
                }
            }
        }
                        
        // Is the king was attacked at the moment
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingChessmans.Count; i++) {
            var chessmanMoves = attackingChessmans[i].GetAvailableMoves(ref chessmans, WIDTH);
            for (int chessmanMovesIndex = 0; chessmanMovesIndex < chessmanMoves.Count; chessmanMovesIndex++) {
                currentAvailableMoves.Add(chessmanMoves[chessmanMovesIndex]);
            }
        }

        // Current team in check at the moment
        if (ContainsValidMove(ref currentAvailableMoves, new Vector2Int(defendingKing.currentColumn, defendingKing.currentRow))) {
            for (int i = 0; i < defendingChessmans.Count; i++) {
                List<Vector2Int> defendingMoves = defendingChessmans[i].GetAvailableMoves(ref chessmans, WIDTH);
                // remove the move can not rescue the king
                SimulateMoveForSingleChessman(defendingChessmans[i], ref defendingMoves, defendingKing);
                if (defendingMoves.Count != 0)
                    return false;
            }
            return true;
        }
        return false;
    }

    private void Victory(ChessmanTeam team) {
        Debug.Log(string.Format("{0} Victory", team));
    }

    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos) {
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
