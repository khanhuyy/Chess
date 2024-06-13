using System.Collections.Generic;
using UnityEngine;

namespace ChessPiece
{
    public class Queen : ChessPiece
    {
        public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int width) {
            List<Vector2Int> result = new List<Vector2Int>();
            // North East
            result.AddRange(GetAvailableBishopMoves(ref board, width));
            result.AddRange(GetAvailableRookMoves(ref board, width));
            return result;
        }

        private List<Vector2Int> GetAvailableBishopMoves(ref ChessPiece[,] board, int width) {
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

        private List<Vector2Int> GetAvailableRookMoves(ref ChessPiece[,] board, int width) {
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
}
