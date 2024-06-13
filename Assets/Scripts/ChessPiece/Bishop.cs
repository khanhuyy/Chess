using System.Collections.Generic;
using UnityEngine;

namespace ChessPiece
{
    public class Bishop : ChessPiece
    {
        public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int width) {
            List<Vector2Int> result = new List<Vector2Int>();
            // North East
            for (int col = currentColumn + 1, row = currentRow + 1; col < width && row < width; col++, row++) {
                if (board[col, row] == null) {
                    result.Add(new Vector2Int(col, row));
                } else {
                    if (board[col, row].team != team) {
                        result.Add(new Vector2Int(col, row));
                    }
                    break;
                }
            }
            // South East
            for (int col = currentColumn + 1, row = currentRow - 1; col < width && row >= 0; col++, row--) {
                if (board[col, row] == null) {
                    result.Add(new Vector2Int(col, row));
                } else {
                    if (board[col, row].team != team) {
                        result.Add(new Vector2Int(col, row));
                    }
                    break;
                }
            }
            // South West
            for (int col = currentColumn - 1, row = currentRow - 1; col >= 0 && row >= 0; col--, row--) {
                if (board[col, row] == null) {
                    result.Add(new Vector2Int(col, row));
                } else {
                    if (board[col, row].team != team) {
                        result.Add(new Vector2Int(col, row));
                    }
                    break;
                }
            }
            // North West
            for (int col = currentColumn - 1, row = currentRow + 1; col >= 0 && row < width; col--, row++) {
                if (board[col, row] == null) {
                    result.Add(new Vector2Int(col, row));
                } else {
                    if (board[col, row].team != team) {
                        result.Add(new Vector2Int(col, row));
                    }
                    break;
                }
            }
            return result;
        }
    }
}
