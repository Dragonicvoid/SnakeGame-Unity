using System.Collections.Generic;

public interface IObstacleManager
{
  public List<ObstacleData> Spikes { set; get; }
  public void InitializeObstacleMap();
  public void ClearObstacle();
  public void CreateSpike(Coordinate coor);
  public List<ObstacleData> GetObstacleList(ARENA_OBJECT_TYPE obstacleType);

  public bool IsPosSafeForSpawn(Coordinate coord);
}
