#nullable enable
using System.Collections.Generic;
using UnityEngine;

public interface IBaseAction
{
  protected float cooldown { get; set; }
  protected float forceRun { get; set; }
  protected SnakeConfig? player { get; set; }
  protected SnakeActionData? currData { get; set; }
  public AStarSearchData? PrevPathfindingData { get; set; }
  public List<Vector2>? Path { get; set; }
  public float Score { get; set; }
  public BOT_ACTION MapKey { get; set; }
  public float LastActionTStamp { get; set; }
  public void init();
  public void onChange();
  public void Run(SnakeConfig player, SnakeActionData data);
  public float UpdateScore(PlannerFactor factor);
  public Vector2 ProcessBotMovementByTarget(SnakeConfig player, Vector2 target);
  public Vector2 ProcessBotMovementByFood(SnakeConfig player, FoodConfig targetFood);
  public Vector2 ProcessBotMovementByFatalObs(
    SnakeConfig player,
    List<float> detectedObstacle
  );
  protected Vector2 TurnRadiusModification(
    SnakeConfig player,
    Vector2 newMovement,
    float turnRadius,
    Vector2 coorDir
  );
  protected void UpdateDirection(Vector2 botNewDir);
  protected FoodConfig GetFoodById(string id);
  protected AStarResultData GetPath(
    Vector2 curr,
    Vector2 target,
    List<Vector2>? predefinedPath
  );
  protected bool IsInPlayerAggresiveCone(
    SnakeConfig targetPlayer,
    SnakeConfig currPlayer
  );
  protected void ResetPathData();
  public float Mag(Vector2 pos);
  public float MagV3(List<float> pos);
  public bool AllowToChange();
}
