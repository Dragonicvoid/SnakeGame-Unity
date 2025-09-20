using System.Collections.Generic;
using UnityEngine;

public static class AStarFunctions
{
  public static string GetStringCoordName(AStarVector pos)
  {
    (int x, int y) = GetIdxByPos(pos);
    string result = "Coord_" + x + "_" + y;
    return result;
  }

  public static string GetStringCoordName(Vector2 pos)
  {
    (int x, int y) = GetIdxByPos(new AStarVector(pos.x, pos.y));
    string result = "Coord_" + x + "_" + y;
    return result;
  }

  public static (int x, int y) GetIdxByPos(AStarVector pos)
  {
    float tile = ARENA_DEFAULT_SIZE.TILE;
    float width = ARENA_DEFAULT_SIZE.WIDTH;
    float height = ARENA_DEFAULT_SIZE.HEIGHT;

    int idxX = Mathf.FloorToInt((pos.x + width / 2) / tile);
    int idxY = Mathf.FloorToInt((pos.y + height / 2) / tile);

    return (
      idxX,
      idxY
    );
  }

  public static List<Vector2> SliceByPosition(
    List<Vector2> result,
    Vector2 currPos
  )
  {
    int sliceIdx = 0;
    float closestDist = float.MaxValue;
    for (int i = 0; i < result.Count; i++)
    {
      AStarVector currTile = new AStarVector(result[i].x, result[i].y);
      float currDist = Vector2.Distance(new Vector2(currTile.x, currTile.y), new Vector2(currPos.x, currPos.y));

      if (currDist < closestDist)
      {
        closestDist = currDist;
        sliceIdx = i + 1;
      }
    }

    return Util.Slice(result, sliceIdx, result.Count - 1);
  }
}
