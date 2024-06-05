using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChessmanType {
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6,
}

public enum ChessmanTeam {
    White = 0,
    Black = 1,
}

public class Chessman : MonoBehaviour
{
    public int currentRow;
    public int currentColumn;
    public ChessmanTeam team;
    public ChessmanType type;

    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;

    private void Update() {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref Chessman[,] board, int width) {
        List<Vector2Int> r = new List<Vector2Int>();

        r.Add(new Vector2Int(3, 3));
        r.Add(new Vector2Int(3, 4));
        r.Add(new Vector2Int(4, 3));
        r.Add(new Vector2Int(4, 4));
        return r;
    }


    public virtual void SetPosition(Vector3 position, bool force = false) {
        desiredPosition = position;
        if (force) {
            transform.position = desiredPosition;
        }
    }

    public virtual void SetLocalScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
        {
            transform.localScale = desiredScale;
        }
    }
}
