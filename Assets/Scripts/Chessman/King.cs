using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Chessman
{
    public override List<Vector2Int> GetAvailableMoves(ref Chessman[,] board, int width) {
        List<Vector2Int> result = new List<Vector2Int>();
        for (int row = currentRow - 1; row <= currentRow + 1; row++) {
            if (row < 0 || row >= width) {
                continue;
            }
            for (int col = currentColumn - 1; col <= currentColumn + 1; col ++) {
                if (col < 0 || col >= width) {
                    continue;
                }
                if (row == currentRow && col == currentColumn) {
                    continue;
                }
                if (board[col, row] == null || board[col, row].team != team) {
                    result.Add(new Vector2Int(col, row));
                }
            }
        }
        return result;
    }

    // Castling
    public override SpecialMove GetSpecialMoves(ref Chessman[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves) {
        // SPECIAL MOVE!!!: CASTLING
        // Near
        int width = board.GetLength(0);
        if (!moved && !board[0, currentRow].moved) { // the nearest rook is column 7
            bool emptyRoad = true;
            for (int col = 1; col < currentColumn; col++) {
                if (board[col, currentRow] != null) {
                    emptyRoad = false;
                }
            }
            if (emptyRoad) {
                availableMoves.Add(new Vector2Int(currentColumn - 2, currentRow));
                return SpecialMove.Castling;
            }
        }
        // Far
        if (!moved && !board[width - 1, currentRow].moved) { // the furthest rook is column 7 = width -1
            bool emptyRoad = true;
            for (int col = currentColumn + 1; col < width - 1; col++) {
                if (board[col, currentRow] != null) {
                    emptyRoad = false;
                }
            }
            if (emptyRoad) {
                availableMoves.Add(new Vector2Int(currentColumn + 2, currentRow));
                return SpecialMove.Castling;
            }
        }     

        return SpecialMove.None;
    }
}
