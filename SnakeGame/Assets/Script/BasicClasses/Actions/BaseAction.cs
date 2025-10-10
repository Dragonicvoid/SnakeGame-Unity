using System.Collections.Generic;
using UnityEngine;

public class BaseAction : IBaseAction
{
  public virtual float Cooldown { get; set; } = 0f;

  public virtual float ForceRun { get; set; } = 0f;

  public virtual float LastActionTStamp { get; set; } = 0;

  public virtual SnakeConfig? Player { get; set; } = null;

  public virtual SnakeActionData? CurrData { get; set; } = null;

  public virtual BOT_ACTION MapKey { get; set; } = BOT_ACTION.NORMAL;

  public AStarSearchData? PrevPathfindingData { get; set; } = null;

  public List<Vector2>? Path { get; set; }

  public float Score { get; set; } = 0;

  AStar? aStar = null;

  float playerConeDegree = BOT_CONFIG.GetConfig().AGGRESSIVE_CONE_RAD;

  float playerMinDist = BOT_CONFIG.GetConfig().AGGRESSIVE_CONE_DIST;

  const float maxOpenList = 700;

  const float maxCloseList = 300;

  public BaseAction()
  {
    Path = new List<Vector2>();
    aStar = new AStar();
  }

  public virtual void Init()
  {
  }

  public virtual void OnChange() { }

  public virtual void Run(SnakeConfig player, SnakeActionData data) { }

  public virtual float UpdateScore(PlannerFactor factor)
  {
    return 0f;
  }

  public Vector2 ProcessBotMovementByTarget(SnakeConfig player, Vector2 target)
  {
    Vector2 dir = player.State.Body[0].Velocity;
    Vector2 headCood = player.State.Body[0].Position;

    float headAngle = Mathf.Atan2(dir.y, dir.x);
    float headInDegree = headAngle * Mathf.Rad2Deg;

    float dirTowardTarget = Mathf.Atan2(
      target.y - headCood.y,
      target.x - headCood.x
    );
    float angleInDegree = dirTowardTarget * Mathf.Rad2Deg;
    float finalAngle = headInDegree < 0 ? (Mathf.Abs(headInDegree) + angleInDegree) : (360 - (headInDegree - angleInDegree));
    finalAngle %= 360;
    finalAngle = finalAngle < 0 ? (360 + finalAngle) : finalAngle;

    Vector2 targetVec = Util.RotateFromDegree(dir, finalAngle);
    return targetVec;
  }

  public Vector2 ProcessBotMovementByFood(SnakeConfig player, FoodConfig targetFood)
  {
    Vector2 dir = player.State.Body[0].Velocity;
    Vector2 headCood = player.State.Body[0].Position;
    Vector2 foodPos = targetFood.State.Position;

    float headAngle = Mathf.Atan2(dir.y, dir.x);
    float headInDegree = headAngle * Mathf.Rad2Deg;

    float dirTowardFood = Mathf.Atan2(
      foodPos.y - headCood.y,
      foodPos.x - headCood.x
    );
    float angleInDegree = dirTowardFood * Mathf.Rad2Deg;
    float finalAngle = headInDegree < 0 ? (Mathf.Abs(headInDegree) + angleInDegree) : (360 - (headInDegree - angleInDegree));
    finalAngle %= 360;
    finalAngle = finalAngle < 0 ? (360 + finalAngle) : finalAngle;

    Vector2 targetVec = Util.RotateFromDegree(dir, finalAngle);
    return targetVec;
  }

  public Vector2? ProcessBotMovementByFatalObs(
    SnakeConfig snake,
    List<float> detectedObstacle
  )
  {
    float turnAngle = 0;
    if (detectedObstacle.Count > 0)
    {
      if (detectedObstacle.Count == 1)
      {
        turnAngle = detectedObstacle[0];
      }
      else
      {
        float totalAngle = 0;

        for (int i = 0; i < detectedObstacle.Count; i++)
        {
          totalAngle += detectedObstacle[i];
        }

        turnAngle = Mathf.Floor(totalAngle / detectedObstacle.Count);
      }
    }

    if (turnAngle == 0) return null;

    turnAngle += Random.Range(-30, 30);
    Vector2 dir = new Vector2(snake.State.Body[0].Velocity.x, snake.State.Body[0].Velocity.y);
    Vector2 targetVec = Util.RotateFromDegree(dir, turnAngle);

    return targetVec;
  }

  public FoodConfig? GetFoodById(string id)
  {
    if (CurrData == null) return null;

    ManagerActionData manager = CurrData.Manager;

    if (manager == null) return null;

    IFoodManager? foodManager = manager.FoodManager;

    if (foodManager == null) return null;

    FoodConfig? food = foodManager?.GetFoodById(id) ?? null;

    return food;
  }

  public AStarResultData? GetPath(
    Vector2 curr,
    Vector2 target,
    List<Vector2>? predefinedPath
  )
  {
    if (predefinedPath == null)
    {
      predefinedPath = new List<Vector2>();
    }
    if (aStar == null || CurrData?.Manager?.ArenaManager?.MapData == null)
      return null;

    if (PrevPathfindingData == null)
    {
      PrevPathfindingData = new AStarSearchData(
          new List<AStarPointData>(),
          new List<AStarPointData>(),
          new Dictionary<string, AStarPointData>(),
          null
      );
    }

    aStar.SetMap(CurrData.Manager.ArenaManager.MapData);
    AStarResultData path = aStar.Search(
      curr,
      target,
      PrevPathfindingData,
      Player?.Id ?? "",
      predefinedPath
    );

    Path = path.Result;

    // optimization
    if (
      PrevPathfindingData.CloseList.Count > maxCloseList ||
      PrevPathfindingData.OpenList.Count > maxOpenList
    )
    {
      ResetPathData();
    }

    return path;
  }

  public bool IsInPlayerAggresiveCone(
    SnakeConfig targetPlayer,
    SnakeConfig currPlayer
  )
  {
    Vector2 mainPlayerPos = targetPlayer.State.Body[0].Position;
    Vector2 currPlayerPos = currPlayer.State.Body[0].Position;

    Vector2 mainPlayerVec = new Vector2(
        mainPlayerPos.x - currPlayerPos.x,
        mainPlayerPos.y - currPlayerPos.y
    );
    Vector2 BotDir = new Vector2(
        currPlayer.State.Body[0].Velocity.x,
        currPlayer.State.Body[0].Velocity.y
    );

    float angle =
      Mathf.Acos(
        (mainPlayerVec.x * BotDir.x +
          mainPlayerVec.y * BotDir.y) /
          (Vector2.Distance(Vector2.zero, mainPlayerVec) * Vector2.Distance(Vector2.zero, BotDir))
      ) * Mathf.Rad2Deg;

    if (
      Vector2.Distance(currPlayerPos, mainPlayerPos) <= playerMinDist &&
      angle <= (playerConeDegree / 2)
    )
      return true;

    return false;
  }

  public void ResetPathData()
  {
    Path?.Clear();
    PrevPathfindingData = null;
  }

  public float MagV3(float[] vec)
  {
    if (vec.Length != 3)
    {
      return 0;
    }
    return Mathf.Sqrt(
      vec[0] * vec[0] + vec[1] * vec[1] + vec[2] * vec[2]
    );
  }

  public bool AllowToChange()
  {
    float deltaTime = Time.time - LastActionTStamp;
    return deltaTime >= ForceRun;
  }
}
