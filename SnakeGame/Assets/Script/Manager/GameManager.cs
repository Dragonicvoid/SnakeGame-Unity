using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public IArenaManager arenaManager = null;
  public IGridManager gridManager = null;
  public PlayerManager playerManager = null;
  public FoodManager foodManager = null;
  private IUiManager uiManager = null;
  private ArenaInput inputField = null;
  private BotPlanner planner = null;

  private float botInterval = 0;

  private float gameStartTime = 0;

  private DIFFICULTY diff = DIFFICULTY.MEDIUM;
  void Start()
  {

  }

  public void StartGame()
  {
    arenaManager.InitializedMap();
    gameStartTime = Time.fixedTime;
    uiManager.ShowStartUI(false);
    inputField.StartInputListener();

    CreatePlayer();
    SetCollisionEvent();
    SetGameEvent();

    StartCoroutine(gameUpdate());
  }

  IEnumerator<object> gameUpdate()
  {
    yield return new WaitForSeconds(0.016f);
    float deltaTime = Math.Min(0.016f, Time.deltaTime);

    foreach (SnakeConfig snake in playerManager.PlayerList)
    {
      HandleBotLogic(snake);
      PlayerManager.UpdateCoordinate(deltaTime);
    }
  }

  private void CreatePlayer()
  {
    Vector2 centerPos = arenaManager?.CenterPos ?? new Vector2(0, 0);

    float rand = UnityEngine.Random.Range(0, 100) / 100;
    Vector2 playerPos =
      arenaManager?.SpawnPos[rand > 0.5 ? 0 : 1] ?? new Vector2(0, 0);
    Vector2 playerDir = new Vector2(1, 0);
    if (playerPos.x > centerPos.x)
    {
      playerDir.Set(-1, 0);
    }
    playerManager.CreatePlayer(playerPos, playerDir);

    Vector2 enemyPos =
      arenaManager?.SpawnPos[rand > 0.5 ? 1 : 0] ?? new Vector2(0, 0);
    Vector2 enemyDir = new Vector2(1, 0);
    if (enemyPos.x > centerPos.x)
    {
      enemyDir.Set(-1, 0);
    }
    playerManager?.CreatePlayer(enemyPos, enemyDir, true);

    foodManager?.StartSpawningFood();
  }

  void stopGame()
  {
    foodManager?.StopSpawningFood();
    inputField?.StopInputListener();
    StopCoroutine(gameUpdate());
    stopCollisionEvent();
  }

  public void GoToMainMenu()
  {
    foodManager?.RemoveAllFood();
    playerManager?.RemoveAllPlayers();
    foodManager?.RemoveAllFood();
    uiManager?.ShowStartUI();
    uiManager?.ShowEndUI(null, false);
  }

  void setCollisionEvent()
  {
    PhysicsSystem2D.instance.on(
      Contact2DType.BEGIN_CONTACT,
      this.headCollideCb,
    );

    PhysicsSystem2D.instance.on(
      Contact2DType.BEGIN_CONTACT,
      this.foodCollideCb,
    );
  }

  void setGameEvent()
  {
    PersistentDataManager.instance.eventTarget.once(
      GAME_EVENT.GAME_OVER,
      this.gameOverCb,
    );
  }

  void stopCollisionEvent()
  {
    PhysicsSystem2D.instance.off(
      Contact2DType.BEGIN_CONTACT,
      this.headCollideCb,
    );

    PhysicsSystem2D.instance.off(
      Contact2DType.BEGIN_CONTACT,
      this.foodCollideCb,
    );
  }

  void onHeadCollide(selfCollider: Collider2D, otherCollider: Collider2D)
  {
    const gameOverData: GameOverData = {
    player: this.playerManager?.getMainPlayer(),
      enemy: this.playerManager?.getEnemy(),
      time: game.totalTime,
      diff: this.diff,
      isWon: false,
    }
    ;

    if (
      (selfCollider.group === PHYSICS_GROUP.PLAYER &&
        otherCollider.group === PHYSICS_GROUP.ENEMY) ||
      (selfCollider.group === PHYSICS_GROUP.ENEMY &&
        otherCollider.group === PHYSICS_GROUP.PLAYER)
    )
    {
      PersistentDataManager.instance.eventTarget.emit(
        GAME_EVENT.GAME_OVER,
        gameOverData,
      );
    }

    if (
      (selfCollider.group === PHYSICS_GROUP.PLAYER &&
        otherCollider.group === PHYSICS_GROUP.OBSTACLE) ||
      (selfCollider.group === PHYSICS_GROUP.OBSTACLE &&
        otherCollider.group === PHYSICS_GROUP.PLAYER) ||
      (selfCollider.group === PHYSICS_GROUP.ENEMY_BODIES &&
        otherCollider.group === PHYSICS_GROUP.PLAYER) ||
      (selfCollider.group === PHYSICS_GROUP.PLAYER &&
        otherCollider.group === PHYSICS_GROUP.ENEMY_BODIES)
    )
    {
      PersistentDataManager.instance.eventTarget.emit(
        GAME_EVENT.GAME_OVER,
        gameOverData,
      );
    }

    if (
      (selfCollider.group === PHYSICS_GROUP.ENEMY &&
        otherCollider.group === PHYSICS_GROUP.OBSTACLE) ||
      (selfCollider.group === PHYSICS_GROUP.OBSTACLE &&
        otherCollider.group === PHYSICS_GROUP.ENEMY) ||
      (selfCollider.group === PHYSICS_GROUP.PLAYER_BODIES &&
        otherCollider.group === PHYSICS_GROUP.ENEMY) ||
      (selfCollider.group === PHYSICS_GROUP.ENEMY &&
        otherCollider.group === PHYSICS_GROUP.PLAYER_BODIES)
    )
    {
      gameOverData.isWon = true;
      PersistentDataManager.instance.eventTarget.emit(
        GAME_EVENT.GAME_OVER,
        gameOverData,
      );
    }
  }

  void onFoodCollide(selfCollider: Collider2D, otherCollider: Collider2D)
  {
    if (
      (selfCollider.group === PHYSICS_GROUP.FOOD_GRABBER &&
        otherCollider.group === PHYSICS_GROUP.FOOD) ||
      (selfCollider.group === PHYSICS_GROUP.FOOD &&
        otherCollider.group === PHYSICS_GROUP.FOOD_GRABBER)
    )
    {
      const selfNodeParent = selfCollider.node.parent;
      const otherNodeParent = otherCollider.node.parent;

      if (selfNodeParent && otherNodeParent)
      {
        const food =
          this.foodManager?.getFoodByObj(selfNodeParent) ??
          this.foodManager?.getFoodByObj(otherNodeParent);
        const snake =
          this.playerManager?.getPlayerByFoodGrabber(selfNodeParent) ??
          this.playerManager?.getPlayerByFoodGrabber(otherNodeParent);

        if (snake && food && !food.state.eaten)
          this.foodManager?.processEatenFood(snake, food);
      }
    }
  }

  void onGameOver(GameOverData data)
  {
    stopGame();
    uiManager?.ShowEndUI(data);
  }

  void handleBotLogic(SnakeConfig snake)
  {
    if (!snake.IsBot) return;

    if (
      playerManager == null ||
      arenaManager == null ||
      foodManager == null ||
      planner == null
    )
      return;

    List<float> detectedPlayer = new List<float>();
    List<float> detectedWall = new List<float>();
    FoodConfig? detectedFood = null;
    //handle bot Booster Activation
    // this.processBotBoosterUsage(player);

    //if bot in the middle of turning sequene, disable the turn logic
    if (snake.State.InDirectionChange) return;
    //detect player and food
    detectedPlayer = playerManager.FindNearestPlayerTowardPoint(
      snake,
      BOT_CONFIG.TRIGGER_AREA_DST
    );

    detectedWall =
      arenaManager.FindNearestObstacleTowardPoint(
        snake,
        BOT_CONFIG.TRIGGER_AREA_DST
      ) ?? new List<float>();

    // need to updated to adjust botData
    FoodTargetData targetFood = snake.State.TargetFood;
    if (detectedPlayer.Count < 1 && targetFood == null)
    {
      detectedFood =
        arenaManager.GetNearestDetectedFood(
          snake,
          BOT_CONFIG.TRIGGER_AREA_DST
        ) ?? null;
    }

    if (detectedFood != null)
    {
      targetFood = new FoodTargetData(
      detectedFood,
        Time.time
      )
      ;
    }

    if (targetFood != null)
    {
      FoodConfig targetExist = foodManager.FoodList.Find(
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

    SnakeBody currState = snake.State.Body[0];

    GridConfig gridWithMostFood = arenaManager?.GetGridWithMostFood();

    PlannerFactor factor = new PlannerFactor(
      snake,
      playerManager.PlayerList,
    detectedPlayer,
      detectedWall,
      currState.Position,
      detectedFood
    // gridWithMostFood: gridWithMostFood,
    // listOfAvailableGrid: this.gridManager?.gridList ?? [],
    )
    ;
    List<IBaseAction> possibleActions = new List<IBaseAction>();
    foreach (KeyValuePair<BOT_ACTION, IBaseAction> entry in snake.PossibleActions)
    {
      possibleActions.Add(entry.Value);

    }

    IBaseAction currAction = planner.Plan(possibleActions, factor);

    bool differentAction = currAction != snake.Action;
    if (currAction != null && snake.Action.AllowToChange())
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
        playerManager,
      arenaManager,
        foodManager
      ),
      detectedPlayer,
      detectedWall,
      detectedFood
    ));

    snake.State.DebugData = new SnakeDebugData(
      snake.Id,
      snake.Action?.MapKey,
      snake.Action.Path,
      snake.Action?.PrevPathfindingData,
      possibleActions
      );
  }
}
