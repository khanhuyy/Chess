using System.Collections.Generic;
using ChessPiece;
using Net;
using Net.Message;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private Transform rematchIndicator;
    [SerializeField] private Button rematchButton;
    private GameObject currentKeyboardTile;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip captureSound;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip theQueenSound;
    [SerializeField] private AudioClip theRookSound;
    [SerializeField] private AudioClip theKnightSound;
    [SerializeField] private AudioClip theBishopSound;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    private ChessPiece.ChessPiece[,] chessPieces;
    private ChessPiece.ChessPiece currentlyPicked;
    private List<Vector2Int> availableMoves;
    private List<ChessPiece.ChessPiece> deadWhites;
    private List<ChessPiece.ChessPiece> deadBlacks;
    private List<Vector2Int[]> moveList;
    private ChessPieceTeam turn;
    private const int Width = 8;
    private readonly Vector2Int invalidHover = -Vector2Int.one;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private Vector3 centerChessmanOffset;
    private SpecialMove specialMove;

    [Header("Online Game Stuff")]
    private int playerCount;
    private ChessPieceTeam onlineTurn;
    private bool isLocalGame = true;
    private bool[] playerConfirmRematch = new bool[2];

    private void Start()
    {
        turn = ChessPieceTeam.White;
        GenerateTiles(Width / 2);
        currentKeyboardTile = tiles[0, 0];
        SpawnAllChessPieces();
        PositionAllChessPieces();
        RegisterEvents();
        if (!currentCamera)
        {
            currentCamera = Camera.main;
        }
    }

    private void Update()
    {
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var info, 100, LayerMask.GetMask("Tile", "Hover", "HighlightTile")))
        {
            // Get the indexes if the tile hit
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);
            if (currentHover == invalidHover)
            {
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

            // picked chessman
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    if (chessPieces[hitPosition.x, hitPosition.y].team == turn && onlineTurn == turn)
                    {
                        currentlyPicked = chessPieces[hitPosition.x, hitPosition.y];
                        availableMoves = currentlyPicked.GetAvailableMoves(ref chessPieces, Width);
                        specialMove = currentlyPicked.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);
                        PreventCheck();
                        HighlightTiles();
                    }
                }
            }

            if (currentlyPicked != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyPicked.currentColumn, currentlyPicked.currentRow);
                if (ContainsValidMove(ref availableMoves, new Vector2Int(hitPosition.x, hitPosition.y)))
                {
                    MoveTo(previousPosition.x, previousPosition.y, hitPosition.x, hitPosition.y);
                    // Online impl todo split method
                    NetMakeMove mm = new NetMakeMove
                    {
                        OriginalColumn = previousPosition.x,
                        OriginalRow = previousPosition.y,
                        DestinationColumn = hitPosition.x,
                        DestinationRow = hitPosition.y,
                        Team = onlineTurn
                    };
                    Client.Instance.SendToServer(mm);
                    // RemoveHighlightTiles();
                }
                else
                {
                    currentlyPicked.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                    if (currentlyPicked)
                        currentlyPicked = null;
                    RemoveHighlightTiles();
                }
                // if (currentlyPicked)
                //     currentlyPicked = null;
                // RemoveHighlightTiles();

            }
        }
        else
        {
            if (currentHover != invalidHover)
            {
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
                currentHover = invalidHover;
            }
            if (currentlyPicked != null && Input.GetMouseButtonUp(0))
            {
                currentlyPicked.SetPosition(GetTileCenter(currentlyPicked.currentColumn, currentlyPicked.currentRow));
                currentlyPicked = null;
                RemoveHighlightTiles();
            }
        }

        if (currentlyPicked)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            if (horizontalPlane.Raycast(ray, out var distance))
            {
                currentlyPicked.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }
        }
    }

    #region "Helper"
    private Vector3 GetTileCenter(int col, int row)
    {
        return new Vector3(col * tileSize, yOffset, row * tileSize) - bounds + centerChessmanOffset;
    }

    private ChessPieceTeam GetNextTurnTeam()
    {
        return turn == ChessPieceTeam.White ? ChessPieceTeam.Black : ChessPieceTeam.White;
    }

    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Width; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);
        return invalidHover;
    }
    #endregion

    #region "Sub constructor"
    private void GenerateTiles(int boundWidth)
    {
        yOffset += transform.position.y;
        bounds = new Vector3(boundWidth * tileSize, 0, boundWidth * tileSize) + boardCenter;
        Debug.Log(bounds);
        centerChessmanOffset = new Vector3(0.5f, 0, 0.5f);
        tiles = new GameObject[Width, Width];
        for (int row = 0; row < Width; row++)
            for (int col = 0; col < Width; col++)
                tiles[col, row] = GenerateSingleTile(col, row);
    }

    private GameObject GenerateSingleTile(int col, int row)
    {
        GameObject tileObject = new GameObject($"Column:{col}, Row:{row}")
        {
            transform =
            {
                parent = transform
            }
        };

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;
        
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(col * tileSize, yOffset, row * tileSize) - bounds;
        vertices[1] = new Vector3(col * tileSize, yOffset, (row + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((col + 1) * tileSize, yOffset, row * tileSize) - bounds;
        vertices[3] = new Vector3((col + 1) * tileSize, yOffset, (row + 1) * tileSize) - bounds;

        int[] tris = { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    private void SpawnAllChessPieces()
    {
        chessPieces = new ChessPiece.ChessPiece[Width, Width];
        chessPieces[0, 0] = SpawnSingleChessman(ChessPieceType.Rook, ChessPieceTeam.White);
        chessPieces[1, 0] = SpawnSingleChessman(ChessPieceType.Knight, ChessPieceTeam.White);
        chessPieces[2, 0] = SpawnSingleChessman(ChessPieceType.Bishop, ChessPieceTeam.White);
        chessPieces[3, 0] = SpawnSingleChessman(ChessPieceType.King, ChessPieceTeam.White);
        chessPieces[4, 0] = SpawnSingleChessman(ChessPieceType.Queen, ChessPieceTeam.White);
        chessPieces[5, 0] = SpawnSingleChessman(ChessPieceType.Bishop, ChessPieceTeam.White);
        chessPieces[6, 0] = SpawnSingleChessman(ChessPieceType.Knight, ChessPieceTeam.White);
        chessPieces[7, 0] = SpawnSingleChessman(ChessPieceType.Rook, ChessPieceTeam.White);
        for (int i = 0; i < Width; i++)
        {
            chessPieces[i, 1] = SpawnSingleChessman(ChessPieceType.Pawn, ChessPieceTeam.White);
        }

        chessPieces[0, 7] = SpawnSingleChessman(ChessPieceType.Rook, ChessPieceTeam.Black);
        chessPieces[1, 7] = SpawnSingleChessman(ChessPieceType.Knight, ChessPieceTeam.Black);
        chessPieces[2, 7] = SpawnSingleChessman(ChessPieceType.Bishop, ChessPieceTeam.Black);
        chessPieces[3, 7] = SpawnSingleChessman(ChessPieceType.King, ChessPieceTeam.Black);
        chessPieces[4, 7] = SpawnSingleChessman(ChessPieceType.Queen, ChessPieceTeam.Black);
        chessPieces[5, 7] = SpawnSingleChessman(ChessPieceType.Bishop, ChessPieceTeam.Black);
        chessPieces[6, 7] = SpawnSingleChessman(ChessPieceType.Knight, ChessPieceTeam.Black);
        chessPieces[7, 7] = SpawnSingleChessman(ChessPieceType.Rook, ChessPieceTeam.Black);
        for (int i = 0; i < Width; i++)
        {
            chessPieces[i, 6] = SpawnSingleChessman(ChessPieceType.Pawn, ChessPieceTeam.Black);
        }
    }

    private ChessPiece.ChessPiece SpawnSingleChessman(ChessPieceType type, ChessPieceTeam team)
    {
        Instantiate(prefabs[(int)type - 1], transform).TryGetComponent(out ChessPiece.ChessPiece chessPiece);
        if (chessPiece)
        {
            chessPiece.gameObject.name = $"{team} {type}";
            chessPiece.type = type;
            chessPiece.team = team;
            if (chessPiece.TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.material = teamMaterials[(int)team];
            }
        }

        return chessPiece;
    }
    #endregion

    #region "Move Solving"
    private void PositionAllChessPieces()
    {
        for (int row = 0; row < Width; row++)
            for (int col = 0; col < Width; col++)
                if (chessPieces[col, row] != null)
                    PositionSingleChessman(col, row, true);
    }

    private void PositionSingleChessman(int col, int row, bool force = false, bool isMove = false)
    {
        chessPieces[col, row].currentRow = row;
        chessPieces[col, row].currentColumn = col;
        chessPieces[col, row].SetPosition(GetTileCenter(col, row), force, isMove);
    }

    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
    {
        foreach (var move in moves)
        {
            if (move.x == pos.x && move.y == pos.y)
            {
                return true;
            }
        }

        return false;
    }

    private void SolveSpecialMove()
    {
        if (specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[^1];
            ChessPiece.ChessPiece currentPawn = chessPieces[newMove[1].x, newMove[1].y];
            var previousMove = moveList[^2];
            ChessPiece.ChessPiece opponentPawn = chessPieces[previousMove[1].x, previousMove[1].y];
            if (currentPawn.currentColumn == opponentPawn.currentColumn)
            {
                switch (opponentPawn.team)
                {
                    case ChessPieceTeam.White:
                        deadWhites.Add(opponentPawn);
                        opponentPawn.SetLocalScale(Vector3.one * deathSize);
                        opponentPawn.SetPosition(
                            new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + Vector3.forward * (deathChessmanSpaceBetween * deadWhites.Count));
                        break;
                    default:
                        deadBlacks.Add(opponentPawn);
                        opponentPawn.SetLocalScale(Vector3.one * deathSize);
                        opponentPawn.SetPosition(
                            new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + Vector3.back * (deathChessmanSpaceBetween * deadBlacks.Count));
                        break;
                }
            }
            chessPieces[opponentPawn.currentColumn, opponentPawn.currentRow] = null;
        }

        if (specialMove == SpecialMove.Promotion)
        {

            Vector2Int[] lastMove = moveList[^1];
            ChessPiece.ChessPiece currentPawn = chessPieces[lastMove[1].x, lastMove[1].y];
            if (currentPawn.type == ChessPieceType.Pawn)
            {
                Debug.Log(currentPawn.initRow);
                if (Mathf.Abs(currentPawn.currentRow - currentPawn.initRow) == 6)
                {
                    ChessPiece.ChessPiece newQueen = SpawnSingleChessman(ChessPieceType.Queen, turn);
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSingleChessman(lastMove[1].x, lastMove[1].y, true);
                }
            }
        }

        if (specialMove == SpecialMove.Castling)
        {
            Vector2Int[] kingLastMove = moveList[^1];
            // far
            if (kingLastMove[1].x - kingLastMove[0].x == 2)
            {
                ChessPiece.ChessPiece rook = chessPieces[Width - 1, kingLastMove[1].y];
                chessPieces[kingLastMove[1].x - 1, kingLastMove[1].y] = rook;
                PositionSingleChessman(kingLastMove[1].x - 1, kingLastMove[1].y);
                chessPieces[Width - 1, kingLastMove[1].y] = null;
            }
            else if (kingLastMove[0].x - kingLastMove[1].x == 2)
            { // near
                ChessPiece.ChessPiece rook = chessPieces[0, kingLastMove[1].y];
                chessPieces[kingLastMove[1].x + 1, kingLastMove[1].y] = rook;
                PositionSingleChessman(kingLastMove[1].x + 1, kingLastMove[1].y);
                chessPieces[0, kingLastMove[1].y] = null;
            }
        }
    }

    private void MoveTo(int originalColumn, int originalRow, int destinationColumn, int destinationRow)
    {

        ChessPiece.ChessPiece currentChessPiece = chessPieces[originalColumn, originalRow];
        Vector2Int previousPosition = new Vector2Int(originalColumn, originalRow);
        if (chessPieces[destinationColumn, destinationRow])
        {
            ChessPiece.ChessPiece opponentChessPiece = chessPieces[destinationColumn, destinationRow];
            if (currentChessPiece.team == opponentChessPiece.team)
            {
                return;
            }
            else
            {
                if (opponentChessPiece.team == ChessPieceTeam.White)
                {
                    deadBlacks.Add(opponentChessPiece);
                    opponentChessPiece.SetLocalScale(Vector3.one * deathSize);
                    opponentChessPiece.SetPosition(
                        new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + Vector3.forward * (deathChessmanSpaceBetween * deadWhites.Count));
                }
                else
                {
                    deadWhites.Add(opponentChessPiece);
                    opponentChessPiece.SetLocalScale(Vector3.one * deathSize);
                    opponentChessPiece.SetPosition(
                        new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds
                        + new Vector3(tileSize / 2, 0, tileSize / 2)
                        + Vector3.back * (deathChessmanSpaceBetween * deadBlacks.Count));
                }
                // not happened
                if (opponentChessPiece.type == ChessPieceType.King)
                {
                    Victory(opponentChessPiece.team);
                }
                audioSource.clip = captureSound; // todo move to sound manager
                audioSource.Play(0);
            }
        } else {
            audioSource.clip = moveSound;
            audioSource.Play(0);
        }
        chessPieces[destinationColumn, destinationRow] = currentChessPiece;
        chessPieces[previousPosition.x, previousPosition.y] = null;
        PositionSingleChessman(destinationColumn, destinationRow, false, true);

        moveList.Add(new [] { previousPosition, new (destinationColumn, destinationRow) });
        SolveSpecialMove();
        if (CheckMate())
        {
            Victory(turn);
        }
        if (currentlyPicked)
            currentlyPicked = null;
        RemoveHighlightTiles();
        turn = (turn == ChessPieceTeam.White) ? ChessPieceTeam.Black : ChessPieceTeam.White;
        if (isLocalGame)
        {
            onlineTurn = (onlineTurn == ChessPieceTeam.White) ? ChessPieceTeam.Black : ChessPieceTeam.White;
            GameUI.Instance.ChangeCamera((turn == ChessPieceTeam.White) ? CameraAngle.WhiteTeam : CameraAngle.BlackTeam);
        }
    }
    #endregion

    #region "Highlight moves"
    private void HighlightTiles()
    {
        foreach (var move in availableMoves)
        {
            tiles[move.x, move.y].layer = LayerMask.NameToLayer("HighlightTile");
            tiles[move.x, move.y].GetComponent<MeshRenderer>().material = highlightMaterial;
        }
    }

    private void RemoveHighlightTiles()
    {
        foreach (var availableMove in availableMoves)
        {
            tiles[availableMove.x, availableMove.y].layer = LayerMask.NameToLayer("Tile");
            tiles[availableMove.x, availableMove.y].GetComponent<MeshRenderer>().material = tileMaterial;
        }

        availableMoves.Clear();
    }
    #endregion

    #region "Check solving"
    private void PreventCheck()
    {
        ChessPiece.ChessPiece targetKing = null;
        bool findTheKing = false;
        for (int col = 0; col < Width; col++)
        {
            for (int row = 0; row < Width; row++)
                if (chessPieces[col, row] != null)
                    if (chessPieces[col, row].type == ChessPieceType.King)
                        if (chessPieces[col, row].team == turn)
                        {
                            targetKing = chessPieces[col, row];
                            findTheKing = true;
                        }
            if (findTheKing)
            {
                break;
            }
        }
        SimulateMoveForSingleChessman(currentlyPicked, ref availableMoves, targetKing);
    }

    private void SimulateMoveForSingleChessman(ChessPiece.ChessPiece cm, ref List<Vector2Int> moves, ChessPiece.ChessPiece targetKing)
    {
        // Save the current values to reset after the function call
        int actualCol = cm.currentColumn;
        int actualRow = cm.currentRow;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        // Going through all moves, simulate them and check if we're in check
        for (int i = 0; i < moves.Count; i++)
        {
            int simCol = moves[i].x;
            int simRow = moves[i].y;

            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentColumn, targetKing.currentRow);
            if (cm.type == ChessPieceType.King)
            {
                kingPositionThisSim = new Vector2Int(simCol, simRow);
            }
            ChessPiece.ChessPiece[,] simulation = new ChessPiece.ChessPiece[Width, Width];
            List<ChessPiece.ChessPiece> simAttackingChessPieces = new List<ChessPiece.ChessPiece>();
            for (int col = 0; col < Width; col++)
            {
                for (int row = 0; row < Width; row++)
                {
                    simulation[col, row] = chessPieces[col, row];
                    if (simulation[col, row] && simulation[col, row].team != cm.team)
                        simAttackingChessPieces.Add(simulation[col, row]);
                }
            }
            // simulate that move were check the king
            simulation[actualCol, actualRow] = null;
            cm.currentColumn = simCol;
            cm.currentRow = simRow;
            simulation[simCol, simRow] = cm;

            // Did one of the chessman got taken down during simulation
            var deadChessman = simAttackingChessPieces.Find(c => c.currentColumn == simCol && c.currentRow == simRow);
            if (deadChessman)
                simAttackingChessPieces.Remove(deadChessman);

            // Get all the simulated attacking pieces moves
            List<Vector2Int> simMoves = new List<Vector2Int>();
            foreach (var simPiece in simAttackingChessPieces)
            {
                var chessmanMoves = simPiece.GetAvailableMoves(ref simulation, Width);
                foreach (var move in chessmanMoves)
                {
                    simMoves.Add(move);
                }
            }

            // Is the king in trouble? if so, remove the move
            if (ContainsValidMove(ref simMoves, kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            // Restore the actual chessman data
            cm.currentColumn = actualCol;
            cm.currentRow = actualRow;
        }

        // Remove from the current available move list
        foreach (var move in movesToRemove)
        {
            moves.Remove(move);
        }
    }

    private bool CheckMate()
    {
        ChessPieceTeam defendingTeam = GetNextTurnTeam();

        List<ChessPiece.ChessPiece> attackingChessPieces = new List<ChessPiece.ChessPiece>();
        List<ChessPiece.ChessPiece> defendingChessPieces = new List<ChessPiece.ChessPiece>();
        ChessPiece.ChessPiece defendingKing = null;
        for (int col = 0; col < Width; col++)
        {
            for (int row = 0; row < Width; row++)
            {
                if (chessPieces[col, row])
                {
                    if (chessPieces[col, row].team == defendingTeam)
                    {
                        defendingChessPieces.Add(chessPieces[col, row]);
                        if (chessPieces[col, row].type == ChessPieceType.King)
                        {
                            defendingKing = chessPieces[col, row];
                        }
                    }
                    else
                    {
                        attackingChessPieces.Add(chessPieces[col, row]);
                    }
                }
            }
        }
        // could not happen
        if (!defendingKing) 
            return false;

        // Is the king was attacked at the moment
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        foreach (var attackingChessPiece in attackingChessPieces)
        {
            var chessmanMoves = attackingChessPiece.GetAvailableMoves(ref chessPieces, Width);
            foreach (var move in chessmanMoves)
            {
                currentAvailableMoves.Add(move);
            }
        }

        // Current team in check at the moment
        if (ContainsValidMove(ref currentAvailableMoves, new Vector2Int(defendingKing.currentColumn, defendingKing.currentRow)))
        {
            foreach (var piece in defendingChessPieces)
            {
                List<Vector2Int> defendingMoves = piece.GetAvailableMoves(ref chessPieces, Width);
                // remove the move can not rescue the king
                SimulateMoveForSingleChessman(piece, ref defendingMoves, defendingKing);
                if (defendingMoves.Count != 0)
                    return false;
            }

            return true;
        }
        return false;
    }
    #endregion

    #region "Victory UI solving"
    private void Victory(ChessPieceTeam team)
    {
        Debug.Log($"{team} Victory");
        victoryScreen.SetActive(true);
        if (team == ChessPieceTeam.White) 
        {
            victoryScreen.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            victoryScreen.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    public void OnRematchButton()
    {
        if (isLocalGame)
        {
            NetRematch whiteRm = new NetRematch
            {
                Team = ChessPieceTeam.White, // 0 for white
                WantRematch = 1
            };
            Client.Instance.SendToServer(whiteRm);

            NetRematch blackRm = new NetRematch
            {
                Team = ChessPieceTeam.Black, // 1 for black
                WantRematch = 1
            };
            Client.Instance.SendToServer(blackRm);
        }
        else
        {
            NetRematch rm = new NetRematch
            {
                Team = onlineTurn,
                WantRematch = 1
            };
            Client.Instance.SendToServer(rm);
        }
    }

    private void GameReset() {
        // ui
        rematchButton.interactable = true;
        rematchIndicator.transform.GetChild(0).gameObject.SetActive(false);
        rematchIndicator.transform.GetChild(1).gameObject.SetActive(false);

        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.SetActive(false);

        currentlyPicked = null;
        availableMoves.Clear();
        moveList.Clear();
        playerConfirmRematch[0] = playerConfirmRematch[1] = false;

        // clean up
        for (int column = 0; column < Width; column++)
        {
            for (int row = 0; row < Width; row++)
            {
                if (chessPieces[column, row] != null)
                {
                    Destroy(chessPieces[column, row].gameObject);
                }
                chessPieces[column, row] = null;
            }
        }

        foreach (var deadPiece in deadWhites)
        {
            Destroy(deadPiece.gameObject);
        }

        foreach (var chessPiece in deadBlacks)
        {
            Destroy(chessPiece.gameObject);
        }

        deadWhites.Clear();
        deadBlacks.Clear();

        SpawnAllChessPieces();
        PositionAllChessPieces();
        playerCount = 0;
        turn = ChessPieceTeam.White;
        if (isLocalGame) {
            onlineTurn = ChessPieceTeam.White;
        }
    }

    public void OnMenuButton()
    {
        NetRematch rm = new NetRematch
        {
            Team = onlineTurn,
            WantRematch = 0
        };
        Client.Instance.SendToServer(rm);
        Application.Quit();
        GameReset();
        GameUI.Instance.OnLeaveFromInGameMenu();
        Invoke(nameof(ShutdownRelay), 1.0f);
        // Reset some values
        playerCount = 0;
        onlineTurn = ChessPieceTeam.Black;

    }
    #endregion

    #region "Online solving"
    private void RegisterEvents()
    {
        NetUtility.SWelcome += OnWelcomeServer;
        NetUtility.SMakeMove += OnMakeMoveServer;
        NetUtility.SRematch += OnRematchServer;

        NetUtility.CWelcome += OnWelcomeClient;
        NetUtility.CStartGame += OnStartGameClient;
        NetUtility.CMakeMove += OnMakeMoveClient;
        NetUtility.CRematch += OnRematchClient;

        GameUI.Instance.SetLocalGame += OnSetLocalGame;
    }

    private void UnRegisterEvents()
    {
        NetUtility.SWelcome -= OnWelcomeServer;
        NetUtility.SMakeMove -= OnMakeMoveServer;
        NetUtility.SRematch -= OnRematchServer;

        NetUtility.CWelcome -= OnWelcomeClient;
        NetUtility.CStartGame -= OnStartGameClient;
        NetUtility.CMakeMove -= OnMakeMoveClient;
        NetUtility.CRematch -= OnRematchClient;

        GameUI.Instance.SetLocalGame -= OnSetLocalGame;
    }
    
    // Server
    private void OnWelcomeServer(NetMessage msg, NetworkConnection nc)
    {
        // Client has connected, assign a team and return the message back to server
        var nw = msg as NetWelcome;

        // Assign a team
        ++playerCount;
        nw!.AssignedTeam = playerCount == 1 ? ChessPieceTeam.White : ChessPieceTeam.Black;

        // Return back to the client
        Server.Instance.SendToClient(nc, nw);

        if (playerCount == 2)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }

    private static void OnMakeMoveServer(NetMessage msg, NetworkConnection nc)
    {
        // var mm = msg as NetMakeMove;
        Server.Instance.Broadcast(msg);
    }

    private static void OnRematchServer(NetMessage msg, NetworkConnection nc)
    {
        Server.Instance.Broadcast(msg);
    }

    private void OnWelcomeClient(NetMessage msg)
    {
        // Receive the connection message
        var nw = msg as NetWelcome;

        // assign team
        // turn = nw.AssignedTeam;
        onlineTurn = nw!.AssignedTeam;
        
        if (isLocalGame && onlineTurn == ChessPieceTeam.White)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }

    private void OnStartGameClient(NetMessage msg)
    {
        GameUI.Instance.ChangeCamera((onlineTurn == ChessPieceTeam.White) ? CameraAngle.WhiteTeam : CameraAngle.BlackTeam);
    }

    private void OnMakeMoveClient(NetMessage msg)
    {
        NetMakeMove mm = msg as NetMakeMove;
        // string chessmanName = chessPieces[mm.destinationColumn, mm.destinationRow].gameObject.name;
        // ?? 
        if (mm == null || mm.Team == onlineTurn) return;
        ChessPiece.ChessPiece target = chessPieces[mm.OriginalColumn, mm.OriginalRow];
        availableMoves = target.GetAvailableMoves(ref chessPieces, Width);
        specialMove = target.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);
        MoveTo(mm.OriginalColumn, mm.OriginalRow, mm.DestinationColumn, mm.DestinationRow);
    }

    private void OnRematchClient(NetMessage msg)
    {
        NetRematch rm = msg as NetRematch;
        if (rm == null) return;
        playerConfirmRematch[(int)rm.Team] = rm.WantRematch == 1;
        // ?
        if (rm.Team != onlineTurn)
        {
            rematchIndicator.transform.GetChild((rm.WantRematch == 1) ? 0 : 1).gameObject.SetActive(true);
            if(rm.WantRematch < 1) // not equal 1
            {
                rematchButton.interactable = false;
            }
        }
        if (playerConfirmRematch[0] && playerConfirmRematch[1])
            GameReset();
    }

    private void ShutdownRelay() {
        Client.Instance.Shutdown();
        Server.Instance.Shutdown();
    }

    private void OnSetLocalGame(bool local)
    {
        playerCount = 0;
        onlineTurn = ChessPieceTeam.Black;
        isLocalGame = local;
    }
    #endregion
}
