using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CheckMatches
{
    private static CheckMatches instance;
    public static CheckMatches Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CheckMatches();
            }
            return instance;
        }
    }

    // 매치 타입 상수
    private const int MIN_MATCH_COUNT = 3;
    private const int SQUARE_MATCH_COUNT = 4;
    private const int L_SHAPE_MATCH_COUNT = 3;
    private const int T_SHAPE_MATCH_COUNT = 3;

    private Tile[,] field;
    private float offsetX;
    private float offsetY;
    private const int AREA_X = 4;
    private const int AREA_Y = 4;

    private CheckMatches() { }

    public bool Check(Tile[,] field, int startX, int startY, int endX, int endY, Position? originalPosA = null, Position? originalPosB = null)
    {
        bool needRefill = false;

        // 매치 우선순위에 따라 체크
        needRefill |= CheckLineMatches(startX, endX, startY, endY, 5, field);  // 5개 매치
        //needRefill |= CheckLShapeMatches(startX, endX, startY, endY, field);   // L자 매치
        needRefill |= CheckLineMatches(startX, endX, startY, endY, 4, field);  // 4개 매치
        needRefill |= CheckSquareMatches(startX, endX, startY, endY, field);   // 2x2 정사각형
        needRefill |= CheckLineMatches(startX, endX, startY, endY, 3, field);  // 3개 매치

        return needRefill;
    }

    private bool CheckLineMatches(int startX, int endX, int startY, int endY, int minMatchCount, Tile[,] field)
    {
        bool foundMatch = false;

        // 가로 매치 확인
        foundMatch |= CheckHorizontalMatches(startX, endX, startY, endY, minMatchCount, field);

        // 세로 매치 확인
        foundMatch |= CheckVerticalMatches(startX, endX, startY, endY, minMatchCount, field);

        return foundMatch;
    }

    private bool CheckHorizontalMatches(int startX, int endX, int startY, int endY, int minMatchCount, Tile[,] field)
    {
        bool foundMatch = false;
        for (int y = startY; y <= endY; y++)
        {
            int matchCount = 1;
            for (int x = startX + 1; x <= endX; x++)
            {
                if (IsValidMatch(field[x, y], field[x - 1, y]))
                {
                    matchCount++;
                }
                else
                {
                    if (matchCount >= minMatchCount)
                    {
                        foundMatch |= AddMatchedTiles(matchCount, x - 1, y, true, field);
                        Debug.Log($"Matched Line-{minMatchCount} : {x - matchCount},{y} to {x - 1},{y}");
                    }
                    matchCount = 1;
                }
            }
            if (matchCount >= minMatchCount)
            {
                foundMatch |= AddMatchedTiles(matchCount, endX, y, true, field);
                Debug.Log($"Matched Line-{minMatchCount} : {endX - matchCount + 1},{y} to {endX},{y}");
            }
        }
        return foundMatch;
    }

    private bool CheckVerticalMatches(int startX, int endX, int startY, int endY, int minMatchCount, Tile[,] field)
    {
        bool foundMatch = false;
        for (int x = startX; x <= endX; x++)
        {
            int matchCount = 1;
            for (int y = startY + 1; y <= endY; y++)
            {
                if (IsValidMatch(field[x, y], field[x, y - 1]))
                {
                    matchCount++;
                }
                else
                {
                    if (matchCount >= minMatchCount)
                    {
                        foundMatch |= AddMatchedTiles(matchCount, x, y - 1, false, field);
                        Debug.Log($"Matched Line-{minMatchCount} : {x},{y - matchCount} to {x},{y - 1}");
                    }
                    matchCount = 1;
                }
            }
            if (matchCount >= minMatchCount)
            {
                foundMatch |= AddMatchedTiles(matchCount, x, endY, false, field);
                Debug.Log($"Matched Line-{minMatchCount} : {x},{endY - matchCount + 1} to {x},{endY}");
            }
        }
        return foundMatch;
    }

    private bool IsValidMatch(Tile tile1, Tile tile2)
    {
        return tile1 != null && tile2 != null &&
               tile1.type == tile2.type &&
               !tile1.isMatched && !tile2.isMatched;
    }

    private bool AddMatchedTiles(int matchCount, int x, int y, bool isHorizontal, Tile[,] field)
    {
        if (matchCount >= MIN_MATCH_COUNT)
        {
            for (int i = 0; i < matchCount; i++)
            {
                Position pos = isHorizontal ?
                    new Position(x - i, y) :
                    new Position(x, y - i);
                field[pos.x, pos.y].isMatched = true;
            }
            return true;
        }
        return false;
    }

    private bool CheckSquareMatches(int startX, int endX, int startY, int endY, Tile[,] field)
    {
        bool foundMatch = false;
        for (int x = startX; x <= endX - 1; x++)
        {
            for (int y = startY; y <= endY - 1; y++)
            {
                if (IsValidSquareMatch(x, y, field))
                {
                    MarkSquareAsMatched(x, y, field);
                    foundMatch = true;
                    Debug.Log($"Matched Square : {x},{y} to {x + 1},{y + 1}");
                }
            }
        }
        return foundMatch;
    }

    private bool IsValidSquareMatch(int x, int y, Tile[,] field)
    {
        if (field[x, y] == null || field[x + 1, y] == null ||
            field[x, y + 1] == null || field[x + 1, y + 1] == null)
            return false;

        TileType type = field[x, y].type;
        return field[x + 1, y].type == type &&
               field[x, y + 1].type == type &&
               field[x + 1, y + 1].type == type &&
               !field[x, y].isMatched &&
               !field[x + 1, y].isMatched &&
               !field[x, y + 1].isMatched &&
               !field[x + 1, y + 1].isMatched;
    }

    private void MarkSquareAsMatched(int x, int y, Tile[,] field)
    {
        field[x, y].isMatched = true;
        field[x + 1, y].isMatched = true;
        field[x, y + 1].isMatched = true;
        field[x + 1, y + 1].isMatched = true;
    }
}