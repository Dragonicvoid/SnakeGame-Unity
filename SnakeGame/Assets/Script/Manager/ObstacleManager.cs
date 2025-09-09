#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour, IObstacleManager
{
  [SerializeField]
  GameObject spike;

  [SerializeField]
  GameObject obstacleParent;

  [SerializeField]
  GridManager gridManager;

  private List<List<TileMapData>> obstacleMap = new List<List<TileMapData>>();
  List<ObstacleData> spikes = new List<ObstacleData>();

  void Awake()
  {
    ClearObstacle();
    InitializeObstacleMap();
  }

  public void InitializeObstacleMap()
  {
    float TILE = ARENA_DEFAULT_SIZE.TILE;
    int rows = Mathf.FloorToInt(ARENA_DEFAULT_SIZE.WIDTH / TILE);
    int cols = Mathf.FloorToInt(ARENA_DEFAULT_SIZE.HEIGHT / TILE);
    obstacleMap = new List<List<TileMapData>>();

    for (int y = cols - 1; y >= 0; y--)
    {
      obstacleMap[y] = new List<TileMapData>();
      for (int x = 0; x < rows; x++)
      {
        Vector2 pos = ArenaConverter.ConvertCoorToArenaPos(x, y);
        int gridPos = ArenaConverter.GetGridIdxByPos(pos.x, pos.y);
        TileMapData tileData = new TileMapData(pos.x, pos.y, ARENA_OBJECT_TYPE.NONE, new List<string>(), gridPos);
        obstacleMap[y][x] = tileData;
      }
    }
  }

  public void ClearObstacle()
  {
    foreach (ObstacleData s in spikes)
    {
      Destroy(s.Obj);
    }
    spikes.Clear();
    obstacleMap.Clear();
  }

  public void CreateSpike(Coordinate coor)
  {
    if (obstacleParent == null) return;
    RectTransform spikeUiTransform = spike.GetComponent<RectTransform>();
    if (spikeUiTransform == null) return;
    float width = spikeUiTransform.rect.width;
    float height = spikeUiTransform.rect.height;
    setObstacleMapObject(coor.X, coor.Y, ARENA_OBJECT_TYPE.SPIKE);
    Vector2 pos = ArenaConverter.ConvertCoorToArenaPos(coor.X, coor.Y);
    int gridPos = ArenaConverter.GetGridIdxByPos(pos.x, pos.y);
    gridManager.AddSpike(new SpikeConfig(gridPos != -1 ? gridPos : 0, pos));

    ObstacleData spikeData = new ObstacleData
    (
      obstacleParent,
      pos,
      null
    );
    GameObject? spikeObj = instantiateSpike(coor);
    spikeData.Obj = spikeObj;
    spikes.Add(spikeData);
  }

  private GameObject? instantiateSpike(Coordinate coor)
  {
    if (spike == null) return null;

    GameObject newSpike = Instantiate(spike);
    newSpike.transform.SetParent(obstacleParent.transform);

    Vector2 pos = ArenaConverter.ConvertCoorToArenaPos(coor.X, coor.Y);
    newSpike.transform.localPosition = new Vector3(pos.x, pos.y);
    newSpike.SetActive(true);

    return newSpike;
  }

  private ARENA_OBJECT_TYPE getObstacleMapObjectType(float x, float y)
  {
    float TILE = ARENA_DEFAULT_SIZE.TILE;
    int idxX = Mathf.FloorToInt(x / TILE);
    int idxY = Mathf.FloorToInt(y / TILE);

    if (!Util.IsCoordInsideMap(idxX, idxY))
    {
      return ARENA_OBJECT_TYPE.NONE;
    }

    List<TileMapData> column = obstacleMap[idxX];
    TileMapData cell = column[idxY];
    // If cell does not exist, return NONE as default


    return cell.Type;
  }

  private void setObstacleMapObject(
    int x,
    int y,
    ARENA_OBJECT_TYPE type
    )
  {
    obstacleMap[y][x].Type = type;
  }

  public List<ObstacleData> GetObstacleList(ARENA_OBJECT_TYPE obstacleType)
  {
    switch (obstacleType)
    {
      case ARENA_OBJECT_TYPE.SPIKE:
        return spikes;
      default:
        return spikes;
    }
  }

  public bool IsPosSafeForSpawn(Coordinate coord)
  {
    int safe = 0;
    for (int y = -1; y <= 1; y++)
    {
      for (int x = -1; x <= 1; x++)
      {
        safe |=
          (int)obstacleMap[coord.Y + y][coord.X + x].Type &
          (int)(ARENA_OBJECT_TYPE.SPIKE | ARENA_OBJECT_TYPE.WALL);
      }
    }
    return safe == 0;
  }
}
