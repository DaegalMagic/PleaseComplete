using UnityEngine;
using System.Collections;

public class TileManager : MonoBehaviour
{
    public GameObject tilePrefab;
    private Tile[,] field = new Tile[ConstValue.SIZE, ConstValue.SIZE];

    public void InitializeField()
    {
        for (int x = 0; x < ConstValue.SIZE; x++)
        {
            for (int y = 0; y < ConstValue.SIZE; y++)
            {
                Vector3 position = GetWorldPosition(x, y);
                GameObject obj = Instantiate(tilePrefab, position, Quaternion.identity);
                Tile tile = obj.GetComponent<Tile>();
                tile.type = (TileType)Random.Range(0, 4);
                tile.position = new Position(x, y);
                field[x, y] = tile;
            }
        }
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        float worldX = (x - (ConstValue.SIZE - 1) / 2f) * ConstValue.TILE_DISTANCE;
        float worldY = (y - (ConstValue.SIZE - 1) / 2f) * ConstValue.TILE_DISTANCE;
        return new Vector3(worldX, worldY, 0);
    }

    public Vector3 GetWorldPosition(Position pos)
    {
        return GetWorldPosition(pos.x, pos.y);
    }

    public Tile GetTileAt(int x, int y)
    {
        if (IsInBounds(x, y))
            return field[x, y];
        return null;
    }

    public void SetTileAt(int x, int y, Tile tile)
    {
        if (IsInBounds(x, y))
            field[x, y] = tile;
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < ConstValue.SIZE && y < ConstValue.SIZE;
    }

    public void SwapTiles(Tile a, Tile b)
    {
        if (a == null || b == null) return;

        Position posA = a.position;
        Position posB = b.position;

        field[posA.x, posA.y] = b;
        field[posB.x, posB.y] = a;

        (a.position, b.position) = (b.position, a.position);
    }

    public void ResetMatchedFlags()
    {
        foreach (var tile in field)
        {
            if (tile != null)
                tile.isMatched = false;
        }
    }
}