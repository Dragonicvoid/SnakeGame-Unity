#nullable enable
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

  const float playerConeDegree = BOT_CONFIG.AGGRESSIVE_CONE_RAD;

  const float playerMinDist = BOT_CONFIG.AGGRESSIVE_CONE_DIST;

  const float maxOpenList = 700;

  const float maxCloseList = 300;

  public BaseAction()
  {
    Path = new List<Vector2>();
    aStar = new AStar();
  }

  public virtual void Init() { }

  public virtual void OnChange() { }

  public virtual void Run(SnakeConfig player, SnakeActionData data) { }

  public virtual float UpdateScore(PlannerFactor factor)
  {
    return 0f;
  }

  public Vector2 ProcessBotMovementByTarget(SnakeConfig player, Vector2 target)
  {
    Vector2 headCood = player.State.Body[0].Position;
    float dirTowardTarget = Mathf.Atan2(
      headCood.y - target.y,
      headCood.x - target.x
    );
    Vector2 targetVec = new Vector2(
-Mathf.Cos(dirTowardTarget),
  Mathf.Sin(2 * Mathf.PI - dirTowardTarget)
    );
    return targetVec;
  }

  public Vector2 ProcessBotMovementByFood(SnakeConfig player, FoodConfig targetFood)
  {
    Vector2 headCood = player.State.Body[0].Position;
    Vector2 foodPos = targetFood.State.Position;
    float dirTowardFood = Mathf.Atan2(
      headCood.y - foodPos.y,
      headCood.x - foodPos.x
    );
    Vector2 targetVec = new Vector2(
      -Mathf.Cos(dirTowardFood),
      Mathf.Sin(2 * Mathf.PI - dirTowardFood)
    );
    return targetVec;
  }

  public Vector2? ProcessBotMovementByFatalObs(
    SnakeConfig _,
    List<float> detectedObstacle
  )
  {
    if (detectedObstacle.Count > 0)
    {
      float? turnAngle = null;
      if (detectedObstacle.Count == 1)
      {
        turnAngle = detectedObstacle[0] + 135;
      }
      else
      {
        float angleOne = detectedObstacle[detectedObstacle.Count - 1];
        float angleTwo = detectedObstacle[0];
        float highestAngleDifference = 360 - Mathf.Abs(angleTwo - angleOne);
        for (int i = 1; i < detectedObstacle.Count; i++)
        {
          float angleDiff = Mathf.Abs(
            detectedObstacle[i] - detectedObstacle[i - 1]
          );
          if (angleDiff > highestAngleDifference)
          {
            angleOne = detectedObstacle[i - 1];
            highestAngleDifference = angleDiff;
          }
        }
        turnAngle = highestAngleDifference / 2 + angleOne;
      }
      if (turnAngle > 360)
      {
        turnAngle -= 360;
      }
      float turnAngleInRad = (turnAngle ?? 0f) * Mathf.PI / 180f;
      Vector2 targetVec = new Vector2(
        -Mathf.Cos(turnAngleInRad),
        Mathf.Sin(2 * Mathf.PI - turnAngleInRad)
      );
      return targetVec;
    }

    return null;
  }

  public Vector2? TurnRadiusModification(
    SnakeConfig player,
    Vector2 newMovement,
    float turnRadius,
    Vector2? coorDir
  )
  {
    if (CurrData == null) return null;

    ManagerActionData? manager = CurrData.Manager;
    if (manager == null) return null;

    IPlayerManager? playerManager = manager.PlayerManager;
    coorDir = coorDir != null ? coorDir : playerManager?.GetPlayerDirection(player.Id);
    if (coorDir == null) return null;

    //TURN RADIUS config range 0 - 5, transform to degrees = 30 - 180
    float turnRadians = Mathf.Deg2Rad * (turnRadius * 30 + 30);
    Vector2 currDir = new Vector2(coorDir.Value.x, coorDir.Value.y);
    Vector2 newDir = new Vector2(newMovement.x, newMovement.y);
    float orientation = Util.GetOrientationBetweenVector(currDir, newDir);
    float angle = Vector2.Angle(currDir, newDir);
    if (Mathf.Abs(angle) < turnRadians) return newDir;
    float turnAngle = turnRadians * orientation;
    Vector2 result = Util.RotateFromAngle(currDir, turnAngle);
    return result;
  }

  public void UpdateDirection(Vector2 botNewDir)
  {
    if (Player == null || CurrData == null) return;

    ManagerActionData manager = CurrData.Manager;
    if (manager == null) return;
    IPlayerManager? playerManager = manager.PlayerManager;

    if (playerManager == null) return;

    Vector2 newDir = Player.State.MovementDir;
    Vector2 currDir = playerManager.GetPlayerDirection(Player.Id);

    newDir = new Vector2(
      Mathf.Ceil(botNewDir.x * ARENA_DEFAULT_SIZE.TILE),
      Mathf.Ceil(botNewDir.y * ARENA_DEFAULT_SIZE.TILE)
    );

    if (newDir != null)
    {
      Player.State.MovementDir = new Vector2(newDir.x, newDir.y);
    }

    Vector2 targetDir = new Vector2(
      newDir.x,
      newDir.y
    );
    List<Vector2> dirArray = new List<Vector2>();
    for (
      int limit = 0;
      newDir != null &&
      currDir.x != targetDir.x &&
      currDir.y != targetDir.y &&
      limit < 6;
      limit++
    )
    {
      if (playerManager != null) return;
      newDir =
        TurnRadiusModification(
          Player,
          new Vector2(targetDir.x, targetDir.y),
          BOT_CONFIG.TURN_RADIUS,
          currDir
        ) ?? new Vector2(0, 0);
      if (newDir == null) return;
      currDir = new Vector2(newDir.x, newDir.y);
      dirArray.Add(newDir);
    }

    if (dirArray.Count <= 0) return;

    Player.State.RotationQueue.Clear();
    int idx = 0;
    dirArray.ForEach((item) =>
    {
      float schedule = idx * 0.064f;
      Player.State.RotationQueue.Add(new SnakeRotationData(
        Time.time + idx * 0.064f,
        item
      ));
      idx++;
    });
  }

  IEnumerator<object> rotateHead(float schedule, IPlayerManager playerManager, Vector2 item)
  {
    yield return new WaitForSeconds(schedule);

    if (Player != null)
    {
      playerManager.HandleMovement(Player.Id, new MovementOpts(
      new Vector2(item.x, item.y),
      null,
      null
      ));
    }
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
          new List<AStarPoint>(),
          new List<AStarPoint>(),
          new Dictionary<string, AStarPoint>(),
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

    Vector2 currPlayerVec = new Vector2(
        currPlayerPos.x - mainPlayerPos.x,
        currPlayerPos.y - mainPlayerPos.y
    );
    Vector2 mainPlayerVec = new Vector2(
        targetPlayer.State.MovementDir.x,
        targetPlayer.State.MovementDir.y
    );


    float angle =
      Mathf.Acos(
        (currPlayerVec.x * mainPlayerVec.x +
          currPlayerVec.y * mainPlayerVec.y) /
          (Vector2.Distance(Vector2.zero, mainPlayerVec) * Vector2.Distance(Vector2.zero, currPlayerVec))
      ) *
        180 /
      Mathf.PI;

    if (
      Vector2.Distance(currPlayerPos, mainPlayerPos) <= playerMinDist &&
      angle <= playerConeDegree / 2
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
    float deltaTime = Time.time - this.LastActionTStamp;
    return deltaTime >= ForceRun;
  }
}
