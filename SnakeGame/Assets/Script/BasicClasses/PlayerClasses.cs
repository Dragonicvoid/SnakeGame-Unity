#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class FoodGrabber
{
  public Vector2 Position { get; set; }
  public float Radius { get; set; }
  public GameObject? Obj { get; set; }

  public FoodGrabber(
    Vector2 Position,
    float Radius,
    GameObject? Obj
  )
  {
    this.Position = Position;
    this.Radius = Radius;
    if (Obj != null)
    {
      this.Obj = Obj;
    }
  }
}

public class FoodTargetData
{
  public FoodConfig Food { get; set; }
  public float TimeTargeted { get; set; }

  public FoodTargetData(
    FoodConfig Food,
    float TimeTargeted
  )
  {
    this.Food = Food;
    this.TimeTargeted = TimeTargeted;
  }
}

public class SnakeDebugData
{
  public string? EnemyID;
  public BOT_ACTION? ActionName;
  public List<Vector2>? EnemyPath;
  public AStarSearchData? PathfindingState;
  public List<IBaseAction>? PossibleActions;

  public SnakeDebugData(
  string? EnemyID,
  BOT_ACTION? ActionName,
  List<Vector2>? EnemyPath,
  AStarSearchData? PathfindingState,
  List<IBaseAction>? PossibleActions
  )
  {
    if (EnemyID != null)
    {
      this.EnemyID = EnemyID;
    }

    if (ActionName != null)
    {
      this.ActionName = ActionName;
    }

    if (EnemyPath != null)
    {
      this.EnemyPath = EnemyPath;
    }

    if (PathfindingState != null)
    {
      this.PathfindingState = PathfindingState;
    }

    if (PossibleActions != null)
    {
      this.PossibleActions = PossibleActions;
    }
  }
}

public class SnakeBody
{
  public Vector2 Position { get; set; }
  public float Radius { get; set; }
  public List<Vector2> MovementQueue { get; set; }
  // actual current direction in the game
  public Vector2 Velocity
  { get; set; }
  public GameObject? Obj { get; set; }

  public SnakeBody(
    Vector2 Position,
   float Radius,
   List<Vector2> MovementQueue,
   Vector2 Velocity,
   GameObject? Obj
  )
  {
    this.Position = Position;
    this.Radius = Radius;
    this.MovementQueue = MovementQueue;
    this.Velocity = Velocity;

    if (Obj != null)
    {
      this.Obj = Obj;
    }
  }
}

public class SnakeState
{
  public FoodGrabber FoodGrabber { get; set; }
  public List<SnakeBody> Body { get; set; }
  // this is target Vec2 direction doesn't mean its
  // actual current direction in the game
  public Vector2 MovementDir { get; set; }
  public Vector2 InputDirection { get; set; }
  public List<SnakeRotationData> RotationQueue { get; set; }
  public float Speed { get; set; }
  public string CoordName { get; set; }
  public bool InDirectionChange { get; set; }
  public SnakeDebugData? DebugData { get; set; }
  public FoodTargetData? TargetFood { get; set; }

  public SnakeState(
    FoodGrabber FoodGrabber,
    List<SnakeBody> Body,
    Vector2 MovementDir,
    Vector2 InputDirection,
    float Speed,
    string CoordName,
    bool InDirectionChange,
    SnakeDebugData? DebugData,
    FoodTargetData? TargetFood
  )
  {
    this.FoodGrabber = FoodGrabber;
    this.Body = Body;
    this.MovementDir = MovementDir;
    this.InputDirection = InputDirection;
    this.Speed = Speed;
    this.CoordName = CoordName;
    this.InDirectionChange = InDirectionChange;
    RotationQueue = new List<SnakeRotationData>();

    if (DebugData != null)
    {
      this.DebugData = DebugData;
    }

    if (TargetFood != null)
    {
      this.TargetFood = TargetFood;
    }
  }
}

public class SnakeConfig
{
  public string Id { get; set; }
  public float LastReactTime { get; set; }
  public SnakeState State { get; set; }
  public bool IsBot { get; set; }
  public bool IsAlive { get; set; }
  public int FoodInStomach { get; set; }
  public ISnakeRenderable? Render
  { get; set; }
  public Dictionary<BOT_ACTION, IBaseAction>? PossibleActions { get; set; }
  public IBaseAction? Action { get; set; }

  public SnakeConfig(
    string Id,
    SnakeState State,
    bool IsBot,
    bool IsAlive,
    ISnakeRenderable? Render,
    Dictionary<BOT_ACTION, IBaseAction>? PossibleActions,
    IBaseAction? Action
    )
  {
    this.Id = Id;
    this.State = State;
    this.IsBot = IsBot;
    this.IsAlive = IsAlive;
    FoodInStomach = 0;
    LastReactTime = Time.fixedTime;

    if (Render != null)
    {
      this.Render = Render;
    }

    if (PossibleActions != null)
    {
      this.PossibleActions = PossibleActions;
    }

    if (Action != null)
    {
      this.Action = Action;
    }
  }
}

public class SnakeActionData
{
  public ManagerActionData Manager { get; set; }
  public List<float> DetectedPlayer { get; set; }
  public List<float> DetectedWall { get; set; }
  public FoodConfig? DetectedFood { get; set; }

  public SnakeActionData(
    ManagerActionData Manager,
    List<float> DetectedPlayer,
    List<float> DetectedWall,
    FoodConfig? DetectedFood
  )
  {
    this.Manager = Manager;
    this.DetectedPlayer = DetectedPlayer;
    this.DetectedWall = DetectedWall;
    if (DetectedFood != null)
    {
      this.DetectedFood = DetectedFood;
    }
  }
}

public class SnakeRotationData
{
  public float TimeToRun { get; set; }
  public Vector2 Dir { get; set; }

  public SnakeRotationData(float timeToRun, Vector2 dir)
  {
    TimeToRun = timeToRun;
    Dir = dir;
  }
}

public class ManagerActionData
{
  public IPlayerManager? PlayerManager { get; set; }
  public IArenaManager? ArenaManager { get; set; }
  public IFoodManager? FoodManager { get; set; }

  public ManagerActionData(
  IPlayerManager? PlayerManager,
  IArenaManager? ArenaManager,
  IFoodManager? FoodManager
  )
  {
    if (PlayerManager != null)
    {
      this.PlayerManager = PlayerManager;
    }

    if (ArenaManager != null)
    {
      this.ArenaManager = ArenaManager;
    }

    if (FoodManager != null)
    {
      this.FoodManager = FoodManager;
    }
  }
}

public class MovementOpts
{
  public Vector2? Direction { get; set; }
  public float? Speed { get; set; }
  public bool? InitialMovement { get; set; }

  public MovementOpts(
    Vector2? Direction,
    float? Speed,
    bool? InitialMovement
  )
  {
    if (Direction != null)
    {
      this.Direction = Direction;
    }
    if (Speed != null)
    {
      this.Speed = Speed;
    }
    if (InitialMovement != null)
    {
      this.InitialMovement = InitialMovement;
    }
  }
}

public class SnakeTypeAndSkin
{
  public SkinDetail Skin { get; set; }
  public SNAKE_TYPE Type { get; set; }

  public SnakeTypeAndSkin(
    SkinDetail Skin,
    SNAKE_TYPE Type
  )
  {
    this.Skin = Skin;
    this.Type = Type;
  }
}