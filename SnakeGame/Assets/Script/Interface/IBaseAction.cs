using System.Collections.Generic;
using UnityEngine;

public interface IBaseAction
{
  public float Cooldown { get; set; }
  public float ForceRun { get; set; }
  public SnakeConfig? Player { get; set; }
  public SnakeActionData? CurrData { get; set; }
  public AStarSearchData? PrevPathfindingData { get; set; }
  public List<Vector2>? Path { get; set; }
  public float Score { get; set; }
  public BOT_ACTION MapKey { get; set; }
  public float LastActionTStamp { get; set; }
  public void Init();
  public void OnChange();
  public void Run(SnakeConfig player, SnakeActionData data);
  public float UpdateScore(PlannerFactor factor);
  public Vector2 ProcessBotMovementByTarget(SnakeConfig player, Vector2 target);
  public Vector2 ProcessBotMovementByFood(SnakeConfig player, FoodConfig targetFood);
  public Vector2? ProcessBotMovementByFatalObs(
    SnakeConfig player,
    List<float> detectedObstacle
  );
  public FoodConfig? GetFoodById(string id);
  public AStarResultData? GetPath(
    Vector2 curr,
    Vector2 target,
    List<Vector2>? predefinedPath
  );
  public bool IsInPlayerAggresiveCone(
    SnakeConfig targetPlayer,
    SnakeConfig currPlayer
  );
  public void ResetPathData();
  public float MagV3(float[] pos);
  public bool AllowToChange();
}
