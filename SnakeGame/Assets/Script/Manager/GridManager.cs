using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour, IGridManager
{
  private List<GridConfig> gridList = new List<GridConfig>();

  public List<GridConfig> GridList
  {
    get
    {
      return gridList;
    }
    set
    {
      gridList = value;
    }
  }

  void Awake()
  {
    Setup();
  }

  public void Setup()
  {
    float GRID_HEIGHT = ARENA_DEFAULT_SIZE.GRID_HEIGHT;
    float GRID_WIDTH = ARENA_DEFAULT_SIZE.GRID_WIDTH;

    int maxCol = Mathf.CeilToInt(ARENA_DEFAULT_SIZE.WIDTH / GRID_WIDTH);
    int maxRow = Mathf.CeilToInt(ARENA_DEFAULT_SIZE.HEIGHT / GRID_HEIGHT);
    gridList = new List<GridConfig>();

    for (int i = 0; i < maxRow; i++)
    {
      for (int j = 0; j < maxCol; j++)
      {
        GridConfig grid = new GridConfig(
          j * GRID_WIDTH - ARENA_DEFAULT_SIZE.WIDTH / 2f,
          j * GRID_WIDTH + GRID_WIDTH - ARENA_DEFAULT_SIZE.WIDTH / 2f,
          i * GRID_HEIGHT - ARENA_DEFAULT_SIZE.HEIGHT / 2f,
          i * GRID_HEIGHT + GRID_HEIGHT - ARENA_DEFAULT_SIZE.HEIGHT / 2f,
          j * GRID_WIDTH + GRID_WIDTH * 0.5f - ARENA_DEFAULT_SIZE.WIDTH / 2f,
          i * GRID_HEIGHT + GRID_HEIGHT * 0.5f - ARENA_DEFAULT_SIZE.HEIGHT / 2f,
          new List<FoodConfig>(),
          new List<SpikeConfig>(),
          new Dictionary<string, int>()
        )
  ;
        gridList.Add(grid);
      }
    }
  }

  public void AddSpike(SpikeConfig spike)
  {
    Vector2 pos = spike.Position;
    for (int i = 0; i < gridList.Count; i++)
    {
      GridConfig grid = gridList[i];

      bool insideX = grid.X1 <= pos.x && pos.x < grid.X2;
      bool insideY = grid.Y1 <= pos.y && pos.y < grid.Y2;

      if (insideX && insideY)
      {
        spike.GridIndex = i;
        gridList[i].Spikes.Add(spike);
        break;
      }
    }
  }

  public void AddFood(FoodConfig foodInstance)
  {
    Vector2 pos = foodInstance.State.Position;
    for (int i = 0; i < gridList.Count; i++)
    {
      GridConfig grid = gridList[i];

      bool insideX = grid.X1 <= pos.x && pos.x < grid.X2;
      bool insideY = grid.Y1 <= pos.y && pos.y < grid.Y2;

      if (insideX && insideY)
      {
        foodInstance.GridIndex = i;
        gridList[i].Foods.Add(foodInstance);
        break;
      }
    }
  }

  public void UpdateFood(FoodConfig food)
  {
    RemoveFood(food);
    AddFood(food);
  }

  public void RemoveFood(FoodConfig food)
  {
    GridConfig grid = gridList[food.GridIndex];

    if (grid == null) return;

    int index = grid.Foods.FindIndex((f) => f == food);
    if (index != -1)
    {
      grid.Foods = Util.Slice(grid.Foods, index, grid.Foods.Count - 1);
    }
  }

  public void RemoveBodyOnGrid(Vector2 pos, string playerID)
  {
    int gridIdx = ArenaConverter.GetGridIdxByPos(pos.x, pos.y);

    if (gridIdx == -1) return;

    GridConfig currGrid = gridList[gridIdx];
    if (currGrid != null)
    {
      int gridTotalBodies;
      bool isExist = currGrid.ChickBodies.TryGetValue(playerID, out gridTotalBodies);
      if (isExist)
      {
        currGrid.ChickBodies[playerID] = Mathf.Max(gridTotalBodies - 1, 0);
      }
      else
      {
        currGrid.ChickBodies.TryAdd(playerID, 0);
      }
    }
  }

  public void AddBodyOnGrid(Vector2 pos, string playerID)
  {
    int gridIdx = ArenaConverter.GetGridIdxByPos(pos.x, pos.y);

    if (gridIdx == -1) return;

    GridConfig currGrid = gridList[gridIdx];
    if (currGrid != null)
    {
      int gridTotalBodies;
      bool isExist = currGrid.ChickBodies.TryGetValue(playerID, out gridTotalBodies);
      if (isExist)
      {
        currGrid.ChickBodies[playerID] = gridTotalBodies + 1;
      }
      else
      {
        currGrid.ChickBodies.TryAdd(playerID, 1);
      }
    }
  }
}
