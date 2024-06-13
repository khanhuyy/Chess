using System.Collections.Generic;
using UnityEngine;

namespace ChessPiece
{
    public class Pawn : ChessPiece
    {
        // public int direction;
        // private const int totalCaptureMovedTile = 6;

        private int GetDirection() {
            return (team == ChessPieceTeam.White) ? 1 : -1;
        }

        public override void SetPosition(Vector3 position, bool force = false, bool isReallyMove = false) {
            SetDesiredPosition(position);
            if (isReallyMove) {
                if (!moved) {
                    moved = true;
                }
            }
            if (!force) return;
            transform.position = GetDesiredPosition();
            initRow = currentRow;
            initColumn = currentColumn;
        }

        public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int width) {
            List<Vector2Int> result = new List<Vector2Int>();
            if (currentRow + GetDirection() < width && !board[currentColumn, currentRow + GetDirection()]) {
                result.Add(new Vector2Int(currentColumn, currentRow + GetDirection()));
                if (!moved && !board[currentColumn, currentRow + GetDirection() * 2])
                {
                    result.Add(new Vector2Int(currentColumn, currentRow + GetDirection() * 2));
                }
            }
        

            // Kill kill kill
            if (currentRow + GetDirection() < width) { // Not happen when at the end of board, pawn will be promotion    
                if (currentColumn != width - 1) {
                    if (board[currentColumn + 1, currentRow + GetDirection()] && board[currentColumn + 1, currentRow + GetDirection()].team != team)
                        result.Add(new Vector2Int(currentColumn + 1, currentRow + GetDirection()));
                }
                if (currentColumn != 0) {
                    if (board[currentColumn - 1, currentRow + GetDirection()] && board[currentColumn - 1, currentRow + GetDirection()].team != team)
                        result.Add(new Vector2Int(currentColumn - 1, currentRow + GetDirection()));
                }
            }

            return result;
        }

        public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves) {
            // Promotion
            // todo refactor
            // if ( totalMovedTile == 5 ) {
            //     return SpecialMove.Promotion;
            // }
            if (Mathf.Abs(currentRow - initRow) == 5) {
                return SpecialMove.Promotion;
            }

            // En Passant
            if (moveList.Count > 0) {
                Vector2Int[] lastMove = moveList[^1];
                if (board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn) {
                    if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2) {
                        if (board[lastMove[1].x, lastMove[1].y].team != team) {
                            if (lastMove[1].y == currentRow) {
                                if (lastMove[1].x == currentColumn - 1) {
                                    availableMoves.Add(new Vector2Int(currentColumn - 1, currentRow + GetDirection()));
                                    return SpecialMove.EnPassant;
                                }
                                if (lastMove[1].x == currentColumn + 1) {
                                    availableMoves.Add(new Vector2Int(currentColumn + 1, currentRow + GetDirection()));
                                    return SpecialMove.EnPassant;
                                }
                            }
                        }
                    }
                }
            }        

            return SpecialMove.None;
        }
    }
}
