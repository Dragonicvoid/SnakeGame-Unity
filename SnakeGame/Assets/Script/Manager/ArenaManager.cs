#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ArenaManager : MonoBehaviour, IArenaManager
{
  [SerializeField]
  IRef<IGridManager>? gridManager;
  [SerializeField]
  IRef<IObstacleManager>? obsManager;
  [SerializeField]
  AiRenderer? aiDebugger;

  private List<List<TileMapData>> mapData = new List<List<TileMapData>>();

  private List<Vector2> spawnPos = new List<Vector2>();

  private Vector2 centerPos = new Vector2();

  public List<List<TileMapData>> MapData { get { return mapData; } set { mapData = value; } }
  public List<Vector2> SpawnPos { get { return spawnPos; } set { spawnPos = value; } }
  public Vector2 CenterPos { get { return centerPos; } set { centerPos = value; } }

  void Awake()
  {
    InitializedMap();
  }

  public void InitializedMap()
  {
    int mapIdx = PersistentData.Instance.SelectedMap;
    LevelMapData map = MAP.ConfigMaps[mapIdx];
    spawnPos = new List<Vector2>();
    centerPos = ArenaConverter.ConvertCoorToArenaPos(
      Mathf.FloorToInt(map.Row / 2f),
      Mathf.FloorToInt(map.Col / 2f)
    );
    mapData = new List<List<TileMapData>>();
    gridManager?.I.Setup();
    obsManager?.I.ClearObstacle();
    obsManager?.I.InitializeObstacleMap();

    for (int i = 0; i < map.Col; i++)
    {
      mapData.Add(new List<TileMapData>());
    }

    for (int y = map.Col - 1; y >= 0; y--)
    {
      for (int x = 0; x < map.Row; x++)
      {
        float posX =
          x * ARENA_DEFAULT_SIZE.TILE - ARENA_DEFAULT_SIZE.WIDTH / 2f;
        float posY =
          y * ARENA_DEFAULT_SIZE.TILE - ARENA_DEFAULT_SIZE.HEIGHT / 2f;
        int gridIdx = ArenaConverter.GetGridIdxByPos(
          posX,
          posY
        );
        mapData[y].Add(new TileMapData(posX, posY, ARENA_OBJECT_TYPE.NONE, new List<string>(), gridIdx));
        int idx = y * map.Row + x;
        handleTileByType((ARENA_OBJECT_TYPE)map.Maps[idx], new Coordinate(x, y));
      }
    }

    aiDebugger?.SetMapToDebug(mapData);
  }

  private void handleTileByType(ARENA_OBJECT_TYPE type, Coordinate coor)
  {
    switch (type)
    {
      case ARENA_OBJECT_TYPE.SPIKE:
        obsManager?.I.CreateSpike(coor);
        break;
      case ARENA_OBJECT_TYPE.NONE:
        break;
      case ARENA_OBJECT_TYPE.FOOD:
        break;
      case ARENA_OBJECT_TYPE.WALL:
        break;
      case ARENA_OBJECT_TYPE.SNAKE:
        break;
      case ARENA_OBJECT_TYPE.SPAWN_POINT:
        Vector2 pos = ArenaConverter.ConvertCoorToArenaPos(coor.X, coor.X);
        spawnPos.Add(pos);
        break;
      default:
        break;
    }
    UpdateTileType(coor, type);
  }

  public void UpdateTileType(Coordinate coord, ARENA_OBJECT_TYPE type)
  {
    if (
      mapData != null &&
      Util.IsCoordInsideMap(coord.X, coord.Y)
    )
    {
      mapData[coord.Y][coord.X].Type = type;
    }
  }

  /**
 * Get all obstacle angle in reverse from snake head position
 * assuming snake Movement Dir is Vector2(0,1).
 * The result will be between 0-360 moving clockwise from snake move dir
 */
  public List<float>? FindObsAnglesFromSnake(SnakeConfig player, float radius)
  {
    SnakeBody? playerHead = player.State.Body[0];

    if (playerHead == null) return null;

    Vector2 pos = playerHead.Position;
    HashSet<GridConfig?>? gridToCheck = getGridsToCheck(pos);
    List<SpikeConfig> spikes = new List<SpikeConfig>();

    if (gridToCheck == null) return null;

    foreach (GridConfig? grid in gridToCheck)
    {
      if (grid != null)
      {
        spikes.AddRange(grid.Spikes);
      }
    }

    float ang = Mathf.Atan2(player.State.MovementDir.y, player.State.MovementDir.x);
    float snakeAng = ang * Mathf.Rad2Deg;
    Debug.Log(player.State.MovementDir);

    if (spikes.Count <= 0) return new List<float>();

    List<float> duplicateAngleDetection = new List<float>();
    List<float> detectedObstacleAngles = new List<float>();
    SnakeState state = player.State;

    SnakeBody? botHead = state.Body[0];
    Vector2 snakeDir = botHead.Velocity;


    foreach (SpikeConfig spike in spikes)
    {
      bool isDetectObs = isCircleHitBox(
        botHead.Position.x,
        botHead.Position.y,
        spike.Position.x,
        spike.Position.y,
        radius,
        ARENA_DEFAULT_SIZE.TILE
      );

      if (isDetectObs)
      {
        float snakeAngle = Mathf.Atan2(snakeDir.y, snakeDir.x);
        float snakeAngleInDegree = snakeAngle * Mathf.Rad2Deg;

        float invrsObsAngle = Mathf.Atan2(
          botHead.Position.y - spike.Position.y,
          botHead.Position.x - spike.Position.x
        );
        float angleInDegree = invrsObsAngle * Mathf.Rad2Deg;
        angleInDegree = angleInDegree < 0 ? 180 + (180 + angleInDegree) : angleInDegree;

        angleInDegree -= snakeAngleInDegree;
        angleInDegree %= 360;
        angleInDegree = angleInDegree < 0 ? 360 - angleInDegree : angleInDegree;

        if (duplicateAngleDetection.FindIndex((angle) => angle == invrsObsAngle) == -1)
        {
          duplicateAngleDetection.Add(invrsObsAngle);
          detectedObstacleAngles.Add(angleInDegree);
        }
      }
    }

    return detectedObstacleAngles;
  }

  private bool isCircleHitBox(
    float x1,
    float y1,
    float x2,
    float y2,
    float circleRadius,
    float boxWidth
  )
  {
    float deltaX = Mathf.Abs(x1 - x2);
    float deltaY = Mathf.Abs(y1 - y2);

    if (deltaX > boxWidth / 2 + circleRadius)
    {
      return false;
    }
    if (deltaY > boxWidth / 2 + circleRadius)
    {
      return false;
    }

    if (deltaX <= boxWidth / 2)
    {
      return true;
    }
    if (deltaY <= boxWidth / 2)
    {
      return true;
    }

    float hitCorner =
      Mathf.Pow(deltaX - boxWidth / 2, 2) + Mathf.Pow(deltaY - boxWidth / 2, 2);

    return hitCorner <= Mathf.Pow(circleRadius, 2);
  }

  public FoodConfig? GetNearestDetectedFood(
    SnakeConfig player,
    float radius
  )
  {
    FoodConfig? result = null;

    SnakeBody? playerHead = player.State.Body[0];

    if (playerHead == null) return null;

    Vector2 pos = playerHead.Position;

    HashSet<GridConfig?>? gridToCheck = getGridsToCheck(pos);

    if (gridToCheck == null) return null;

    float nearestLength = float.MaxValue;
    foreach (GridConfig? grid in gridToCheck)
    {
      if (grid == null) continue;

      foreach (FoodConfig f in grid.Foods)
      {
        float distance = Vector2.Distance(f.State.Position, playerHead.Position);
        if (result == null || distance < nearestLength)
        {
          result = f;
          nearestLength = distance;
        }
      }
    }

    return result;
  }

  private HashSet<GridConfig?>? getGridsToCheck(Vector2 pos)
  {
    if (gridManager == null) return null;

    HashSet<GridConfig?> gridToCheck = new HashSet<GridConfig?>();

    // check its surroundings
    for (int i = -1; i <= 1; i++)
    {
      for (int j = -1; j <= 1; j++)
      {
        int gridIdx = ArenaConverter.GetGridIdxByPos(
          pos.x + j * ARENA_DEFAULT_SIZE.GRID_WIDTH,
          pos.y + i * ARENA_DEFAULT_SIZE.GRID_HEIGHT
        );

        if (gridIdx == -1) continue;

        gridToCheck.Add(gridManager.I.GridList[gridIdx]);
      }
    }

    return gridToCheck;
  }
  public GridConfig? GetGridWithMostFood()
  {
    GridConfig? result = null;

    List<GridConfig> gridList = gridManager?.I.GridList ?? new List<GridConfig>();
    foreach (GridConfig? grid in gridList)
    {
      if (result == null || grid.Foods.Count > result.Foods.Count)
      {
        result = grid;
      }
    }
    return result;
  }

  public void SetMapBody(Vector2 pos, string playerId)
  {
    if (mapData == null) return;

    Coordinate coord = ArenaConverter.ConvertPosToCoord(pos.x, pos.y);

    if (!Util.IsCoordInsideMap(coord.X, coord.Y)) return;

    mapData[coord.Y][coord.X].PlayerIDList.Add(playerId);
  }

  public void RemovePlayerMapBody(SnakeConfig player)
  {
    foreach (SnakeBody part in player.State.Body)
    {
      RemoveMapBody(part.Position, player.Id);
    }
  }

  public void RemoveMapBody(Vector2 pos, string playerId)
  {
    if (mapData == null) return;

    Coordinate coord = ArenaConverter.ConvertPosToCoord(pos.x, pos.y);

    if (!Util.IsCoordInsideMap(coord.X, coord.Y)) return;

    List<string> playerIds = mapData[coord.Y][coord.X].PlayerIDList;
    playerIds = Util.Filter(playerIds, (id) =>
    {
      return id != playerId;
    });
    mapData[coord.Y][coord.X].PlayerIDList = playerIds;
  }
}
