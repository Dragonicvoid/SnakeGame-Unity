#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
  private List<List<TileMapData>> map = new List<List<TileMapData>>();

  private int padding = 1;

  private string currID = "";

  public void SetMap(List<List<TileMapData>> map)
  {
    this.map = map;
  }

  public AStarResultData Search(
    Vector2 origin,
    Vector2 target,
    AStarSearchData prevData,
    string id,
    List<Vector2>? predefinedPath = null,
    int maxDepth = 10
  )
  {
    float TILE = ARENA_DEFAULT_SIZE.TILE;
    float offset = TILE / 2f;
    int currDepth = 0;
    this.currID = id;

    if (prevData.OpenList.Count <= 0)
    {
      AStarPoint aStarPoint = new AStarPoint(origin, target);
      prevData.OpenList.Add(aStarPoint);
      prevData.MemoiPoint.Add(AStarFunctions.GetStringCoordName(origin), aStarPoint);
    }

    while (prevData.OpenList.Count > 0 && prevData.PathFound == null)
    {
      int lowestFIdx = 0;
      for (int j = 0; j < prevData.OpenList.Count; j++)
      {
        if (prevData.OpenList[j].CurrF < prevData.OpenList[lowestFIdx].CurrF)
        {
          lowestFIdx = j;
        }
      }
      AStarPoint currentPoint = prevData.OpenList[lowestFIdx];

      // found path
      if (
        (currentPoint.Point.HasValue &&
          Vector2.Distance(currentPoint.Point.Value, target) <= TILE) ||
        currDepth >= maxDepth
      )
      {
        if (
          currentPoint.Point.HasValue &&
          Vector2.Distance(currentPoint.Point.Value, target) <= TILE
        )
        {
          prevData.PathFound = currentPoint;
        }

        AStarPoint curr = Util.DeepCopy(currentPoint);
        List<Vector2> result = new List<Vector2>();
        while (curr.PrevPoint != null && curr.Point != null)
        {
          Vector2 currPoint = curr.Point.Value;
          currPoint.Set(currPoint.x + offset, currPoint.y + offset);
          result.Add((Vector2)curr.Point);
          curr = Util.DeepCopy(curr.PrevPoint);
        }

        result.Reverse();
        if (predefinedPath != null)
        {
          result.AddRange(predefinedPath);
        }
        result = AStarFunctions.SliceByPosition(result, origin);
        return new AStarResultData(result, prevData);
      }

      Func<AStarPoint, bool> compare = (el) =>
      {
        return (
          el.Point?.x != currentPoint.Point?.x ||
          el.Point?.y != currentPoint.Point?.y
        );
      };
      prevData.OpenList = Util.Filter(prevData.OpenList, compare);
      prevData.CloseList.Add(currentPoint);
      List<Vector2> neighbor = currentPoint.Point.HasValue
        ? getNeighbor((Vector2)currentPoint.Point)
        : new List<Vector2>();

      for (int i = 0; i < neighbor.Count; i++)
      {
        // already visited
        if (
          prevData.CloseList.Find(
            (el) =>
              el.Point.HasValue &&
              AStarFunctions.GetStringCoordName(neighbor[i]) == AStarFunctions.GetStringCoordName(el.Point.Value)
          ) != null
        )
        {
          continue;
        }

        float gScore =
          currentPoint.CurrGoal +
          getCoordCost(neighbor[i]) +
          (currentPoint.Point.HasValue
            ? Vector2.Distance(currentPoint.Point.Value, neighbor[i])
            : 0f);
        bool gScoreIsBest = false;

        AStarPoint? currNeighbor;
        prevData.MemoiPoint.TryGetValue(
          AStarFunctions.GetStringCoordName(neighbor[i]),
          out currNeighbor
        );

        if (prevData.OpenList.Find((el) => neighbor[i] == el.Point) == null)
        {
          // new node
          currNeighbor = new AStarPoint(neighbor[i], target);
          prevData.MemoiPoint.Add(
            AStarFunctions.GetStringCoordName(neighbor[i]),
            currNeighbor
          );
          prevData.OpenList.Add(currNeighbor);
          gScoreIsBest = true;
        }
        else if (currNeighbor != null && gScore < currNeighbor.CurrGoal)
        {
          // visited node, but with better goal;
          gScoreIsBest = true;
        }

        if (currNeighbor != null && gScoreIsBest)
        {
          currNeighbor.PrevPoint = currentPoint;
          currNeighbor.CurrGoal = gScore;
          currNeighbor.CurrHeuristic = currNeighbor.Point.HasValue
            ? Mathf.Pow(Vector2.Distance(currNeighbor.Point.Value, target), 2)
            : currNeighbor.CurrHeuristic;
        }
      }

      currDepth++;
    }

    if (prevData.PathFound != null)
    {
      AStarPoint curr = Util.DeepCopy(prevData.PathFound);
      List<Vector2> result = new List<Vector2>();
      while (curr.PrevPoint != null && curr.Point != null)
      {
        Vector2 currPoint = curr.Point.Value;
        currPoint.Set(currPoint.x + offset, currPoint.y + offset);
        result.Add(currPoint);
        curr = Util.DeepCopy(curr.PrevPoint);
      }

      result.Reverse();
      result.AddRange(predefinedPath);
      result = AStarFunctions.SliceByPosition(result, origin);
      return new AStarResultData(result, prevData);
      ;
    }

    Debug.Log("NO PATH");
    return new AStarResultData(new List<Vector2>(), prevData)
;
  }

  private List<Vector2> getNeighbor(Vector2 pos)
  {
    float TILE = ARENA_DEFAULT_SIZE.TILE;
    float HEIGHT = ARENA_DEFAULT_SIZE.HEIGHT;
    float WIDTH = ARENA_DEFAULT_SIZE.WIDTH;
    (int x, int y) = AStarFunctions.GetIdxByPos(pos);
    List<Vector2> result = new List<Vector2>();

    // Left, Right, Up, Down
    if (isValidPosition(x + padding, y))
    {
      result.Add(new Vector2(
      (x + padding) * TILE - WIDTH / 2,
        y * TILE - HEIGHT / 2
      ));
    }

    if (isValidPosition(x - padding, y))
    {
      result.Add(new Vector2(
      (x - padding) * TILE - WIDTH / 2,
        y * TILE - HEIGHT / 2
      ));
    }

    if (isValidPosition(x, y + padding))
    {
      result.Add(new Vector2(
      x * TILE - WIDTH / 2,
        (y + padding) * TILE - HEIGHT / 2
      ));
    }

    if (isValidPosition(x, y - padding))
    {
      result.Add(new Vector2(
       x * TILE - WIDTH / 2,
        (y - padding) * TILE - HEIGHT / 2
      ));
    }

    // Diagonal
    if (
      isValidPosition(x + padding, y + padding) &&
      isValidPosition(x + padding, y) &&
      isValidPosition(x, y + padding)
    )
    {
      result.Add(new Vector2(
       (x + padding) * TILE - WIDTH / 2,
        (y + padding) * TILE - HEIGHT / 2
      ));
    }

    if (
      isValidPosition(x - padding, y + padding) &&
      isValidPosition(x - padding, y) &&
      isValidPosition(x, y + padding)
    )
    {
      result.Add(new Vector2(
      (x - padding) * TILE - WIDTH / 2,
        (y + padding) * TILE - HEIGHT / 2
      ));
    }

    if (
      isValidPosition(x - padding, y - padding) &&
      isValidPosition(x - padding, y) &&
      isValidPosition(x, y - padding)
    )
    {
      result.Add(new Vector2(
      (x - padding) * TILE - WIDTH / 2,
        (y - padding) * TILE - HEIGHT / 2
      ));
    }

    if (
      isValidPosition(x + padding, y - padding) &&
      isValidPosition(x + padding, y) &&
      isValidPosition(x, y - padding)
    )
    {
      result.Add(new Vector2(
      (x + padding) * TILE - WIDTH / 2,
        (y - padding) * TILE - HEIGHT / 2
      ));
    }

    return result;
  }

  private bool isValidPosition(
    int idxX,
    int idxY,
    int depth = 1
  )
  {
    bool neighbor = true;
    if (depth > 0)
    {
      depth--;
      neighbor =
        isValidPosition(idxX - 1, idxY, depth) &&
        isValidPosition(idxX - 1, idxY + 1, depth) &&
        isValidPosition(idxX, idxY + 1, depth) &&
        isValidPosition(idxX + 1, idxY + 1, depth) &&
        isValidPosition(idxX + 1, idxY, depth) &&
        isValidPosition(idxX + 1, idxY - 1, depth) &&
        isValidPosition(idxX, idxY - 1, depth) &&
        isValidPosition(idxX - 1, idxY - 1, depth);
    }

    if (map[idxY] == null || map[idxY][idxX] == null)
      return false;

    bool safeObstacle = map[idxY][idxX].Type != ARENA_OBJECT_TYPE.SPIKE;

    string occupyByOther = map[idxY][idxX].PlayerIDList.Find(
      (id) => id != currID
    );

    return safeObstacle && occupyByOther == null && neighbor;
  }

  private int getCoordCost(Vector2 pos)
  {
    (int x, int y) = AStarFunctions.GetIdxByPos(pos);
    if (map[y] == null || map[y][x] == null) return 1;

    switch (map[y][x].Type)
    {
      case ARENA_OBJECT_TYPE.NONE:
        return 1;
      default:
        return 1;
    }
  }
}
