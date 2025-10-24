
using System.Collections.Generic;
using UnityEngine;

public interface IArenaManager
{
  public List<List<TileMapData>> MapData { get; set; }
  public List<Vector2> SpawnPos { get; set; }
  public Vector2 CenterPos { get; set; }
  public void InitializedMap();
  public void UpdateTileType(Coordinate coord, ARENA_OBJECT_TYPE type);
  public DodgeObstacleData FindObsAnglesFromSnake(SnakeConfig player, float radius);
  public FoodConfig GetNearestDetectedFood(
    SnakeConfig player
  );
  public GridConfig GetGridWithMostFood();
  public void SetMapBody(Vector2 pos, string playerId);
  public void RemovePlayerMapBody(SnakeConfig player);
  public void RemoveMapBody(Vector2 pos, string playerId);
  public void ClearSpikeRender();
}
