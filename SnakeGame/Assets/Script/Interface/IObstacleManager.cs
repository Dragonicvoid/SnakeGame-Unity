using System.Collections.Generic;
using UnityEngine;

public interface IObstacleManager
{
  public void InitializeObstacleMap();
  public void ClearObstacle();
  public void CreateSpike(Coordinate coor);
  public List<ObstacleData> GetObstacleList(ARENA_OBJECT_TYPE obstacleType);

  public bool IsPosSafeForSpawn(Coordinate coord);
}
