using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Chessman
{
    public override List<Vector2Int> GetAvailableMoves(ref Chessman[,] board, int width) {
        List<Vector2Int> result = new List<Vector2Int>();
        // Down
        for (int row = currentRow - 1; row >= 0; row--) {
            if (board[currentColumn, row] == null) {
                result.Add(new Vector2Int(currentColumn, row));
            } else {
                if (board[currentColumn, row].team != team) {
                    result.Add(new Vector2Int(currentColumn, row));
                }
                break;
            }
        }
        // Up
        for (int row = currentRow + 1; row < width; row++) {
            if (board[currentColumn, row] == null) {
                result.Add(new Vector2Int(currentColumn, row));
            } else {
                if (board[currentColumn, row].team != team) {
                    result.Add(new Vector2Int(currentColumn, row));
                    Debug.Log("add");
                }
                break;
            }
        }
        // Left
        for (int col = currentColumn - 1; col >= 0; col--) {
            if (board[col, currentRow] == null) {
                result.Add(new Vector2Int(col, currentRow));
            } else {
                if (board[col, currentRow].team != team) {
                    result.Add(new Vector2Int(col, currentRow));
                }
                break;
            }
        }
        // Down
        for (int col = currentColumn + 1; col < width; col++) {
            if (board[col, currentRow] == null) {
                result.Add(new Vector2Int(col, currentRow));
            } else {
                if (board[col, currentRow].team != team) {
                    result.Add(new Vector2Int(col, currentRow));
                }
                break;
            }
        }
        return result;
    }
}
