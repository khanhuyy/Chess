using System.Collections.Generic;
using UnityEngine;

namespace ChessPiece
{
    public class Knight : ChessPiece
    {
        public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int width) {
            List<Vector2Int> result = new List<Vector2Int>();
        
            // 1 hour direction
            int predictMoveColumn = currentColumn + 1;
            int predictMoveRow = currentRow + 2;
            if (predictMoveColumn < width && predictMoveRow < width)
                if (board[predictMoveColumn, predictMoveRow] == null || board[predictMoveColumn, predictMoveRow].team != team)
                    result.Add(new Vector2Int(predictMoveColumn, predictMoveRow));

            // 2 hour direction
            predictMoveColumn = currentColumn + 2;
            predictMoveRow = currentRow + 1;
            if (predictMoveColumn < width && predictMoveRow < width)
                if (board[predictMoveColumn, predictMoveRow] == null || board[predictMoveColumn, predictMoveRow].team != team)
                    result.Add(new Vector2Int(predictMoveColumn, predictMoveRow));

            // 4 hour direction
            predictMoveColumn = currentColumn + 2;
            predictMoveRow = currentRow - 1;
            if (predictMoveColumn < width && predictMoveRow > 0)
                if (board[predictMoveColumn, predictMoveRow] == null || board[predictMoveColumn, predictMoveRow].team != team)
                    result.Add(new Vector2Int(predictMoveColumn, predictMoveRow));

            // 5 hour direction
            predictMoveColumn = currentColumn + 1;
            predictMoveRow = currentRow - 2;
            if (predictMoveColumn < width && predictMoveRow > 0)
                if (board[predictMoveColumn, predictMoveRow] == null || board[predictMoveColumn, predictMoveRow].team != team)
                    result.Add(new Vector2Int(predictMoveColumn, predictMoveRow));

            // 7 hour direction
            predictMoveColumn = currentColumn - 1;
            predictMoveRow = currentRow - 2;
            if (predictMoveColumn > 0 && predictMoveRow > 0)
                if (board[predictMoveColumn, predictMoveRow] == null || board[predictMoveColumn, predictMoveRow].team != team)
                    result.Add(new Vector2Int(predictMoveColumn, predictMoveRow));

            // 8 hour direction
            predictMoveColumn = currentColumn - 2;
            predictMoveRow = currentRow - 1;
            if (predictMoveColumn > 0 && predictMoveRow > 0)
                if (board[predictMoveColumn, predictMoveRow] == null || board[predictMoveColumn, predictMoveRow].team != team)
                    result.Add(new Vector2Int(predictMoveColumn, predictMoveRow));

            // 10 hour direction
            predictMoveColumn = currentColumn - 2;
            predictMoveRow = currentRow + 1;
            if (predictMoveColumn > 0 && predictMoveRow < width)
                if (board[predictMoveColumn, predictMoveRow] == null || board[predictMoveColumn, predictMoveRow].team != team)
                    result.Add(new Vector2Int(predictMoveColumn, predictMoveRow));

            // 11 hour direction
            predictMoveColumn = currentColumn - 1;
            predictMoveRow = currentRow + 2;
            if (predictMoveColumn > 0 && predictMoveRow < width)
                if (board[predictMoveColumn, predictMoveRow] == null || board[predictMoveColumn, predictMoveRow].team != team)
                    result.Add(new Vector2Int(predictMoveColumn, predictMoveRow));

            return result;
        }
    }
}
