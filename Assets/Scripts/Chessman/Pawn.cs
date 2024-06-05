using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Chessman
{
    private bool moved = false;
    public override List<Vector2Int> GetAvailableMoves(ref Chessman[,] board, int width) {
        List<Vector2Int> r = new List<Vector2Int>();
        int direction = ((team == ChessmanTeam.White) ? 1 : -1);
        if (board[currentColumn, currentRow + direction] == null) {
            r.Add(new Vector2Int(currentColumn, currentRow + direction));
        }
        if (board[currentColumn, currentRow + direction * 2] == null && !moved)
        {
            r.Add(new Vector2Int(currentColumn, currentRow + direction * 2));
        }
        if (currentColumn != width - 1) {
            
        }
        return r;
    }
}
