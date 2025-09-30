#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public IRef<IArenaManager>? ArenaManager = null;
  public IRef<IGridManager>? GridManager = null;
  public IRef<IPlayerManager>? PlayerManager = null;
  public IRef<IFoodManager>? FoodManager = null;
  public UiManager? UiManager = null;
  public ArenaInput? ArenaInput = null;
  public BotPlanner? Planner = null;

  private float botInterval = 0;

  private float gameStartTime = 0;

  Coroutine? gameUpdateCorutine = null;

  bool paused = true;

  void FixedUpdate()
  {
    if (paused) return;

    gameUpdate();
  }

  public void StartGame()
  {
    UiEvent.Instance.onGameStartAnimFinish -= onGameStartAnimFinish;
    UiEvent.Instance.onGameStartAnimFinish += onGameStartAnimFinish;
    UiManager?.StartGame();
  }

  void gameUpdate()
  {
    float deltaTime = Math.Min(0.016f, Time.deltaTime);

    foreach (SnakeConfig snake in PlayerManager?.I.PlayerList ?? new List<SnakeConfig>())
    {
      handleBotLogic(snake);
    }

    PlayerManager?.I.UpdateCoordinate(deltaTime);
  }

  private void CreatePlayer()
  {
    Vector2 centerPos = ArenaManager?.I.CenterPos ?? new Vector2(0, 0);

    float rand = UnityEngine.Random.Range(0, 100) / 100;
    Vector2 playerPos =
      ArenaManager?.I.SpawnPos[rand > 0.5 ? 0 : 1] ?? new Vector2(0, 0);
    Vector2 playerDir = new Vector2(1, 0);
    if (playerPos.x > centerPos.x)
    {
      playerDir.Set(-1, 0);
    }
    PlayerManager?.I.CreatePlayer(playerPos, playerDir);

    Vector2 enemyPos =
      ArenaManager?.I.SpawnPos[rand > 0.5 ? 1 : 0] ?? new Vector2(0, 0);
    Vector2 enemyDir = new Vector2(1, 0);
    if (enemyPos.x > centerPos.x)
    {
      enemyDir.Set(-1, 0);
    }
    PlayerManager?.I.CreatePlayer(enemyPos, enemyDir, true);

    FoodManager?.I.StartSpawningFood();
  }

  void stopGame()
  {
    FoodManager?.I.StopSpawningFood();
    ArenaInput?.StopInputListener();
    paused = true;
    stopCollisionEvent();
    stopGameEvent();
  }

  public void GoToMainMenu()
  {
    UiManager?.EndGame();
    FoodManager?.I.RemoveAllFood();
    PlayerManager?.I.RemoveAllPlayers();
    FoodManager?.I.RemoveAllFood();
    ArenaManager?.I.ClearSpikeRender();
    UiManager?.ShowEndUI(null, false);
  }

  void setCollisionEvent()
  {
    stopCollisionEvent();
    CollisionEvent.Instance.onHeadCollide += onHeadCollide;
    CollisionEvent.Instance.onFoodCollide += onFoodCollide;
  }

  void setGameEvent()
  {
    stopGameEvent();
    GameEvent.Instance.onGameOver += onGameOver;
  }

  void stopCollisionEvent()
  {
    CollisionEvent.Instance.onHeadCollide -= onHeadCollide;
    CollisionEvent.Instance.onFoodCollide -= onFoodCollide;
  }

  void stopGameEvent()
  {
    GameEvent.Instance.onGameOver -= onGameOver;
  }

  void onHeadCollide(HeadCollideData data)
  {
    GameOverData gameOverData = new GameOverData(
      Time.time,
      PersistentData.Instance.Difficulty,
      false,
      PlayerManager?.I.GetMainPlayer(),
      PlayerManager?.I.GetEnemy()
    );

    int selfLayer = data.Self.layer;
    int otherLayer = data.Other.layer;

    if (
      (selfLayer == (int)LAYER.PHYSICS_PLAYER &&
        otherLayer == (int)LAYER.PHYSICS_ENEMY) ||
      (selfLayer == (int)LAYER.PHYSICS_ENEMY &&
        otherLayer == (int)LAYER.PHYSICS_PLAYER)
    )
    {
      GameEvent.Instance.GameOver(gameOverData);
    }

    if (
      (selfLayer == (int)LAYER.PHYSICS_PLAYER &&
        otherLayer == (int)LAYER.PHYSICS_OBSTACLE) ||
      (selfLayer == (int)LAYER.PHYSICS_OBSTACLE &&
        otherLayer == (int)LAYER.PHYSICS_PLAYER) ||
      (selfLayer == (int)LAYER.PHYSICS_ENEMY_BODIES &&
        otherLayer == (int)LAYER.PHYSICS_PLAYER) ||
      (selfLayer == (int)LAYER.PHYSICS_PLAYER &&
        otherLayer == (int)LAYER.PHYSICS_ENEMY_BODIES)
    )
    {
      GameEvent.Instance.GameOver(gameOverData);
    }

    if (
      (selfLayer == (int)LAYER.PHYSICS_ENEMY &&
        otherLayer == (int)LAYER.PHYSICS_OBSTACLE) ||
      (selfLayer == (int)LAYER.PHYSICS_OBSTACLE &&
        otherLayer == (int)LAYER.PHYSICS_ENEMY) ||
      (selfLayer == (int)LAYER.PHYSICS_PLAYER_BODIES &&
        otherLayer == (int)LAYER.PHYSICS_ENEMY) ||
      (selfLayer == (int)LAYER.PHYSICS_ENEMY &&
        otherLayer == (int)LAYER.PHYSICS_PLAYER_BODIES)
    )
    {
      gameOverData.IsWon = true;
      GameEvent.Instance.GameOver(gameOverData);
    }
  }

  void onFoodCollide(FoodCollideData data)
  {
    int selfLayer = data.Self.layer;
    int otherLayer = data.Other.layer;
    if (
      (selfLayer == (int)LAYER.PHYSICS_FOOD_GRABBER &&
        otherLayer == (int)LAYER.PHYSICS_FOOD) ||
      (selfLayer == (int)LAYER.PHYSICS_FOOD &&
        otherLayer == (int)LAYER.PHYSICS_FOOD_GRABBER)
    )
    {
      GameObject selfParent = data.Self.gameObject.transform.parent.gameObject;
      GameObject otherParent = data.Other.gameObject.transform.parent.gameObject;

      if (selfParent && otherParent)
      {
        FoodConfig? food =
          FoodManager?.I.GetFoodByObj(selfParent) ??
          FoodManager?.I.GetFoodByObj(otherParent);
        SnakeConfig? snake =
          PlayerManager?.I.GetPlayerByFoodGrabber(selfParent) ??
          PlayerManager?.I.GetPlayerByFoodGrabber(otherParent);

        if (snake != null && food != null && !food.State.Eaten)
          FoodManager?.I.ProcessEatenFood(snake, food);
      }
    }
  }

  void onGameOver(GameOverData data)
  {
    stopGame();
    UiManager?.ShowEndUI(data);
  }

  void onGameStartAnimFinish()
  {
    ArenaManager?.I.InitializedMap();

    UiEvent.Instance.onGameStartAnimFinish -= onGameStartAnimFinish;
    gameStartTime = Time.fixedTime;
    ArenaInput?.StartInputListener();

    CreatePlayer();
    setCollisionEvent();
    setGameEvent();

    paused = false;
  }

  void handleBotLogic(SnakeConfig snake)
  {
    if (!snake.IsBot) return;

    if (
      PlayerManager == null ||
      ArenaManager == null ||
      FoodManager == null ||
      Planner == null
    )
      return;

    List<float> detectedPlayer = new List<float>();
    List<float> detectedWall = new List<float>();
    FoodConfig? detectedFood = null;

    if (snake.State.InDirectionChange) return;

    detectedPlayer = PlayerManager.I.FindNearestPlayerTowardPoint(
      snake,
      BOT_CONFIG.GetConfig().TRIGGER_AREA_DST
    );

    detectedWall =
      ArenaManager.I.FindObsAnglesFromSnake(
        snake,
        BOT_CONFIG.GetConfig().TRIGGER_AREA_DST
      ) ?? new List<float>();

    // need to updated to adjust botData
    FoodTargetData? targetFood = snake.State.TargetFood;
    if (detectedPlayer.Count < 1 && targetFood == null)
    {
      detectedFood =
        ArenaManager.I.GetNearestDetectedFood(
          snake,
          BOT_CONFIG.GetConfig().TRIGGER_AREA_DST
        ) ?? null;
    }

    if (detectedFood != null)
    {
      targetFood = new FoodTargetData(
      detectedFood,
        Time.time
      );
    }

    if (targetFood != null)
    {
      FoodConfig targetExist = FoodManager.I.FoodList.Find(
        (item) => item.Id == targetFood.Food.Id
      );
      bool isEaten = targetFood.Food.State.Eaten;
      bool isExpired = Time.time - targetFood.TimeTargeted > TIME_CONFIG.FOOD_EXPIRED;
      snake.State.TargetFood = targetFood;
      if (targetExist == null || isEaten || isExpired)
      {
        snake.State.TargetFood = null;
      }
    }

    if (snake.State.Body.Count <= 0) return;

    SnakeBody currState = snake.State.Body[0];
    GridConfig? gridWithMostFood = ArenaManager?.I.GetGridWithMostFood();

    PlannerFactor factor = new PlannerFactor(
      snake,
      PlayerManager.I.PlayerList,
    detectedPlayer,
      detectedWall,
      currState.Position,
      detectedFood
    // gridWithMostFood: gridWithMostFood,
    // listOfAvailableGrid: this.gridManager?.gridList ?? [],
    )
    ;
    List<IBaseAction> possibleActions = new List<IBaseAction>();
    if (snake.PossibleActions != null)
    {
      foreach (KeyValuePair<BOT_ACTION, IBaseAction> entry in snake.PossibleActions)
      {
        possibleActions.Add(entry.Value);

      }
    }

    IBaseAction currAction = Planner.Plan(possibleActions, factor);
    bool differentAction = currAction != snake.Action;
    if (currAction != null && snake.Action?.AllowToChange() == true)
    {
      if (differentAction)
      {
        snake.Action.OnChange();
      }

      snake.Action = currAction;

      if (differentAction)
      {
        currAction.Init();
      }
    }

    snake.Action?.Run(snake, new SnakeActionData(
      new ManagerActionData(
        PlayerManager.I,
      ArenaManager?.I,
        FoodManager.I
      ),
      detectedPlayer,
      detectedWall,
      detectedFood
    ));

    snake.State.DebugData = new SnakeDebugData(
      snake.Id,
      snake.Action?.MapKey,
      snake.Action?.Path,
      snake.Action?.PrevPathfindingData,
      possibleActions
      );
  }
}
