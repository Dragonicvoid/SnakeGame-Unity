
using System;
using System.Collections.Generic;
using UnityEngine;

public class AStar
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
    int maxDepth = 3
  )
  {
    float TILE = ARENA_DEFAULT_SIZE.TILE;
    float offset = TILE / 2f;
    int currDepth = 0;
    currID = id;

    if (prevData.OpenList.Count <= 0)
    {
      AStarPointData aStarPoint = new AStarPointData(origin, target);
      prevData.OpenList.Add(aStarPoint);

      bool memoiExist = prevData.MemoiPoint.TryGetValue(AStarFunctions.GetStringCoordName(origin), out _);
      if (memoiExist)
      {
        prevData.MemoiPoint[AStarFunctions.GetStringCoordName(origin)] = aStarPoint;
      }
      else
      {
        prevData.MemoiPoint.TryAdd(AStarFunctions.GetStringCoordName(origin), aStarPoint);
      }
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
      AStarPointData currentPoint = prevData.OpenList[lowestFIdx];

      // found path
      if (
        (Vector2.Distance(new Vector2(currentPoint.Point.x, currentPoint.Point.y), target) <= TILE) ||
        currDepth >= maxDepth
      )
      {
        if (Vector2.Distance(new Vector2(currentPoint.Point.x, currentPoint.Point.y), target) <= TILE)
        {
          prevData.PathFound = currentPoint;
        }

        AStarPointData curr = Util.DeepCopy(currentPoint);
        List<Vector2> result = new List<Vector2>();
        while (curr.PrevPoint != null && curr.Point != null)
        {
          AStarVector currPoint = curr.Point;
          currPoint.Set(currPoint.x + offset, currPoint.y + offset);
          result.Add(new Vector2(curr.Point.x, curr.Point.y));
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

      Func<AStarPointData, bool> compare = (el) =>
      {
        return (
          el.Point?.x != currentPoint.Point?.x ||
          el.Point?.y != currentPoint.Point?.y
        );
      };
      prevData.OpenList = Util.Filter(prevData.OpenList, compare);
      prevData.CloseList.Add(currentPoint);
      List<AStarVector> neighbor = getNeighbor(currentPoint.Point);

      for (int i = 0; i < neighbor.Count; i++)
      {
        // already visited
        if (
          prevData.CloseList.Find(
            (el) =>
              AStarFunctions.GetStringCoordName(neighbor[i]) == AStarFunctions.GetStringCoordName(el.Point)
          ) != null
        )
        {
          continue;
        }

        float gScore =
          currentPoint.CurrGoal +
          getCoordCost(neighbor[i]) +
          Vector2.Distance(new Vector2(currentPoint.Point.x, currentPoint.Point.y), new Vector2(neighbor[i].x, neighbor[i].y));
        bool gScoreIsBest = false;

        AStarPointData? currNeighbor;
        bool memoiExist = prevData.MemoiPoint.TryGetValue(
          AStarFunctions.GetStringCoordName(neighbor[i]),
          out currNeighbor
        );

        if (prevData.OpenList.Find((el) => neighbor[i] == el.Point) == null)
        {
          // new node
          currNeighbor = new AStarPointData(new Vector2(neighbor[i].x, neighbor[i].y), target);

          if (memoiExist)
          {
            prevData.MemoiPoint[AStarFunctions.GetStringCoordName(neighbor[i])] = currNeighbor;
          }
          else
          {
            prevData.MemoiPoint.TryAdd(
              AStarFunctions.GetStringCoordName(neighbor[i]),
              currNeighbor
            );
          }

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
          currNeighbor.CurrHeuristic = Mathf.Pow(Vector2.Distance(new Vector2(currNeighbor.Point.x, currNeighbor.Point.y), target), 2);
        }
      }

      currDepth++;
    }

    if (prevData.PathFound != null)
    {
      AStarPointData curr = Util.DeepCopy(prevData.PathFound);
      List<Vector2> result = new List<Vector2>();
      while (curr.PrevPoint != null && curr.Point != null)
      {
        AStarVector currPoint = curr.Point;
        currPoint.Set(currPoint.x + offset, currPoint.y + offset);
        result.Add(new Vector2(currPoint.x, currPoint.y));
        curr = Util.DeepCopy(curr.PrevPoint);
      }

      result.Reverse();
      result.AddRange(predefinedPath);
      result = AStarFunctions.SliceByPosition(result, origin);
      return new AStarResultData(result, prevData);
    }

    return new AStarResultData(new List<Vector2>(), prevData);
  }

  private List<AStarVector> getNeighbor(AStarVector pos)
  {
    float TILE = ARENA_DEFAULT_SIZE.TILE;
    float HEIGHT = ARENA_DEFAULT_SIZE.HEIGHT;
    float WIDTH = ARENA_DEFAULT_SIZE.WIDTH;
    (int x, int y) = AStarFunctions.GetIdxByPos(pos);
    List<AStarVector> result = new List<AStarVector>();

    // Left, Right, Up, Down
    if (isValidPosition(x + padding, y))
    {
      result.Add(new AStarVector(
      (x + padding) * TILE - WIDTH / 2,
        y * TILE - HEIGHT / 2
      ));
    }

    if (isValidPosition(x - padding, y))
    {
      result.Add(new AStarVector(
      (x - padding) * TILE - WIDTH / 2,
        y * TILE - HEIGHT / 2
      ));
    }

    if (isValidPosition(x, y + padding))
    {
      result.Add(new AStarVector(
      x * TILE - WIDTH / 2,
        (y + padding) * TILE - HEIGHT / 2
      ));
    }

    if (isValidPosition(x, y - padding))
    {
      result.Add(new AStarVector(
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
      result.Add(new AStarVector(
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
      result.Add(new AStarVector(
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
      result.Add(new AStarVector(
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
      result.Add(new AStarVector(
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

  private int getCoordCost(AStarVector pos)
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
