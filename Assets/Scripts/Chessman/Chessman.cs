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
    private Vector3 desiredScale;


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
