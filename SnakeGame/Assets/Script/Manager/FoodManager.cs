#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour, IFoodManager
{
  class TweenData
  {
    public FoodConfig Food;
    public SnakeConfig Snake;

    public TweenData(FoodConfig food, SnakeConfig snake)
    {
      Food = food;
      Snake = snake;
    }
  }
  [SerializeField]
  FoodSpawner? foodSpawner = null;
  [SerializeField]
  IRef<IObstacleManager>? obsManager = null;
  [SerializeField]
  IRef<IGridManager>? gridManager = null;
  [SerializeField]
  private int maxFoodInstance = 5;
  [SerializeField]
  private float foodSpawnInterval = 3f;

  private int maxRetries = 5;

  private int foodCounter = 0;

  public List<FoodConfig> FoodList { set; get; }

  Coroutine? spawnRandFoodCo = null;

  void Awake()
  {
    FoodList = new List<FoodConfig>();
  }

  public void StartSpawningFood()
  {

    foodCounter = 0;
    spawnRandFoodCo = StartCoroutine(spawnRandomFood());
  }

  public void StopSpawningFood()
  {
    if (spawnRandFoodCo != null)
    {
      StopCoroutine(spawnRandFoodCo);
    }
  }

  public FoodConfig? SpawnFood(Vector2 pos, bool animated = true)
  {
    Coordinate coord = ArenaConverter.ConvertPosToCoord(pos.x, pos.y);
    bool isSafe = obsManager?.I.IsPosSafeForSpawn(coord) ?? false;

    if (!isSafe)
    {
      return null;
    }

    GameObject? obj = foodSpawner?.Spawn(pos);

    if (!obj)
    {
      return null;
    }

    FoodConfig food = new FoodConfig(foodCounter.ToString(), new FoodState(pos, false), 0, obj);
    gridManager?.I.AddFood(food);
    FoodList.Add(food);

    UpAndDown upAndDown = obj.GetComponent<UpAndDown>();
    if (animated)
    {
      upAndDown.StartAnimating();
    }
    else
    {
      upAndDown.StopAnimating();
    }

    foodCounter++;

    return food;
  }

  IEnumerator<object> spawnRandomFood()
  {
    int retries = 0;
    while (true)
    {
      yield return null;
      if (!foodSpawner || retries >= maxRetries)
      {
        retries = 0;
        yield return PersistentData.Instance.GetWaitSecond(foodSpawnInterval);
        continue;
      }

      if (foodSpawner?.transform.childCount >= maxFoodInstance) continue;

      Vector2 pos = new Vector2(
        Random.Range(0f, ARENA_DEFAULT_SIZE.WIDTH) - ARENA_DEFAULT_SIZE.WIDTH / 2,
        Random.Range(0f, ARENA_DEFAULT_SIZE.HEIGHT) -
          ARENA_DEFAULT_SIZE.HEIGHT / 2
      );

      FoodConfig? food = SpawnFood(pos);

      if (food == null)
      {
        retries++;
        continue;
      }

      yield return PersistentData.Instance.GetWaitSecond(foodSpawnInterval);
    }
  }

  public void ProcessEatenFood(SnakeConfig player, FoodConfig food)
  {
    if (player.State.Body.Count == 0) return;

    Vector2 targetVec = new Vector2(player.State.Body[0].Position.x, player.State.Body[0].Position.y);
    Vector2 startVec = new Vector2(food.State.Position.x, food.State.Position.y);
    BaseTween<TweenData> tweenData = new BaseTween<TweenData>(
      0.1f,
      new TweenData(food, player),
      (dist, data) =>
      {
        data.Food.State.Eaten = true;
        data.Snake.State.TargetFood = null;
      },
      (dist, data) =>
      {
        Vector2 delta = new Vector2(targetVec.x - startVec.x, targetVec.y - startVec.y);
        delta *= dist;

        Vector2 res = new Vector2(startVec.x + delta.x, startVec.y + delta.y);
        food.Object.transform.localPosition = res;
      },
      (dist, data) =>
      {
        removeFood(data.Food);
        GameEvent.Instance.PlayerSizeIncrease(data.Snake);
      });
    IEnumerator<object> coroutine = Tween.Create(tweenData);
    StartCoroutine(coroutine);
  }

  void removeFood(FoodConfig food)
  {
    gridManager?.I.RemoveFood(food);
    foodSpawner?.RemoveFood(food.Object);

    int idx = FoodList.FindIndex((f) => f.Id == food.Id);
    FoodList = Util.RemoveFromIdx(FoodList, idx);
  }

  public void RemoveAllFood()
  {
    foreach (FoodConfig food in FoodList)
    {
      foodSpawner?.RemoveFood(food.Object);
    }

    FoodList.Clear();
  }

  public FoodConfig GetFoodById(string id)
  {
    return FoodList.Find((food) => food.Id == id);
  }

  public FoodConfig GetFoodByObj(GameObject obj)
  {
    return FoodList.Find((food) => food.Object == obj);
  }
}
