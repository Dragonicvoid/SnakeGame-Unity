
using UnityEngine;

public static class ArenaConverter
{
  public static Vector2 ConvertCoorToArenaPos(int x, int y)
  {
    Vector2 offset = new Vector2(
      ARENA_DEFAULT_SIZE.WIDTH / 2f,
      ARENA_DEFAULT_SIZE.HEIGHT / 2f
    );

    Vector2 realPos = new Vector2(
      x * ARENA_DEFAULT_SIZE.TILE + ARENA_DEFAULT_SIZE.TILE / 2f,
      y * ARENA_DEFAULT_SIZE.TILE + ARENA_DEFAULT_SIZE.TILE / 2f
    );

    return new Vector2(realPos.x - offset.x, realPos.y - offset.y);
  }

  public static Coordinate ConvertPosToCoord(float x, float y)
  {
    Vector2 offset = new Vector2(
      ARENA_DEFAULT_SIZE.WIDTH / 2f,
      ARENA_DEFAULT_SIZE.HEIGHT / 2f

    );
    Coordinate coord = new Coordinate(
      Mathf.FloorToInt((x + offset.x) / ARENA_DEFAULT_SIZE.TILE),
      Mathf.FloorToInt((y + offset.y) / ARENA_DEFAULT_SIZE.TILE)
    );
    return coord;
  }


  public static int GetGridIdxByPos(float x, float y)
  {
    int currIdx;

    Vector2 offset = new Vector2(ARENA_DEFAULT_SIZE.WIDTH / 2, ARENA_DEFAULT_SIZE.HEIGHT / 2);

    int maxRow = Mathf.CeilToInt(
      ARENA_DEFAULT_SIZE.WIDTH / ARENA_DEFAULT_SIZE.GRID_WIDTH
    );
    int currX = Mathf.FloorToInt(
      Mathf.Clamp(x + offset.x, 0, ARENA_DEFAULT_SIZE.WIDTH) /
        ARENA_DEFAULT_SIZE.GRID_WIDTH
    );
    int currY = Mathf.FloorToInt(
      Mathf.Clamp(y + offset.y, 0, ARENA_DEFAULT_SIZE.HEIGHT) /
        ARENA_DEFAULT_SIZE.GRID_HEIGHT
    );
    currIdx = currY * maxRow + currX;
    if (currIdx < 0) return -1;

    return currIdx;
  }
}