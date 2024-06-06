using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Chessman
{
    public override List<Vector2Int> GetAvailableMoves(ref Chessman[,] board, int width) {
        List<Vector2Int> result = new List<Vector2Int>();
        int direction = ((team == ChessmanTeam.White) ? 1 : -1);
        if (board[currentColumn, currentRow + direction] == null) {
            result.Add(new Vector2Int(currentColumn, currentRow + direction));
        }
        if (board[currentColumn, currentRow + direction * 2] == null && !moved)
        {
            result.Add(new Vector2Int(currentColumn, currentRow + direction * 2));
        }

        // Kill kill kill
        if (currentColumn != width - 1) {
            if (board[currentColumn + 1, currentRow + direction] != null && board[currentColumn + 1, currentRow + direction].team != team)
                result.Add(new Vector2Int(currentColumn + 1, currentRow + direction));
        }
        if (currentColumn != 0) {
            if (board[currentColumn - 1, currentRow + direction] != null && board[currentColumn - 1, currentRow + direction].team != team)
                result.Add(new Vector2Int(currentColumn - 1, currentRow + direction));
        }

        // promo

        return result;
    }
}
