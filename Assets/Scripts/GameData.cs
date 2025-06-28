using UnityEngine;
public enum TileType
{
    red, green, blue, yellow,
    garo, sero, daegak, bomb,
    temp
}

public class Position
{
    public int x;
    public int y;
    public Position(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
}

public enum GameState
{
    Idle,
    Swapping,
    Matching,
    Refilling,
    GameOver,
}

public enum BoostType
{
    ResourceBoost,
    UnlimitedMove,
    DamageBoost,
    BreakOne,
    ClearCurrentArea,
    ChangeToColor,
    BossBoost
}

public class BoostItem
{
    public BoostType Type;
    public bool IsAvailable;
    public int Count;
}