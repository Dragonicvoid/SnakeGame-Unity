
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerManager : MonoBehaviour, IPlayerManager
{
  [SerializeField] IRef<IArenaManager>? arenaManager = null;
  [SerializeField] SnakeRender? playerRender = null;
  [SerializeField] SnakeRender? enemyRender = null;
  [SerializeField] SnakeHead? playerHead = null;
  [SerializeField] SnakeHead? enemyHead = null;
  [SerializeField] TrailVfx? playerVfx = null;
  [SerializeField] TrailVfx? enemyVfx = null;
  [SerializeField] SkinSelect? skinSelect = null;
  [SerializeField] GameObject? collParent = null;
  [SerializeField] GameObject? sBodyPref = null;
  [SerializeField] GameObject? sFoodGrabPref = null;
  [SerializeField] AiRenderer? aiRenderer = null;
  [SerializeField] SpikeVfx? spikeVfx = null;
  [SerializeField] FireSpawner? fireSpawner = null;

  public List<SnakeConfig> PlayerList { set; get; }
  Dictionary<string, IEnumerator<object>> eatAnim;
  float intervalToFire = 0.25f;
  float lastTouchStart = 0f;
  string PLAYER_ID = "MAIN_PLAYER";
  string ENEMY_ID = "ENEMY";

  void Awake()
  {
    PlayerList = new List<SnakeConfig>();
    eatAnim = new Dictionary<string, IEnumerator<object>>();

    GameplayMoveEvent.Instance.onSnakeMoveCalculated -= onTouchMove;
    GameplayMoveEvent.Instance.onGameUiStartTouch -= onTouchStart;
    GameEvent.Instance.onPlayerSizeIncrease -= onSizeIncrease;
    GameEvent.Instance.onSnakeFire -= onSnakeFire;
    GameEvent.Instance.onGameOver -= onGameOver;

    GameplayMoveEvent.Instance.onSnakeMoveCalculated += onTouchMove;
    GameplayMoveEvent.Instance.onGameUiStartTouch += onTouchStart;
    GameEvent.Instance.onPlayerSizeIncrease += onSizeIncrease;
    GameEvent.Instance.onSnakeFire += onSnakeFire;
    GameEvent.Instance.onGameOver += onGameOver;
  }

  void FixedUpdate()
  {
    PlayerList.ForEach((p) =>
    {
      List<SnakeRotationData> remaining = new List<SnakeRotationData>();

      p.State.RotationQueue.ForEach((r) =>
      {
        if (Time.time > r.TimeToRun)
        {
          HandleMovement(p.Id, new MovementOpts(
            new Vector2(r.Dir.x, r.Dir.y),
            null,
            null
            ));
        }
        else
        {
          remaining.Add(r);
        }
      });
      p.State.RotationQueue = remaining;
    });
  }

  public void CreatePlayer(Vector2 pos, Vector2 moveDir, bool isBot = false)
  {
    if (!sBodyPref || !sFoodGrabPref) return;

    int totalBodies = 4;

    List<SnakeBody> bodies = new List<SnakeBody>();
    SnakeBody? prevBodies = null;
    Vector2 foodPos = new Vector2(pos.x, pos.y);

    for (int i = 0; i < totalBodies; i++)
    {
      SnakeBody? newBody = createBody(isBot, i == 0, pos);

      if (newBody == null) continue;

      if (prevBodies == null)
      {
        bodies.Add(newBody);
        if (newBody.Obj) newBody.Obj.transform.localPosition = new Vector3(newBody.Position.x, newBody.Position.y);
        foodPos = getFoodGrabberPosition(newBody);
        prevBodies = newBody;
      }
      else
      {
        bodies.Add(newBody);
        if (newBody.Obj) newBody.Obj.transform.localPosition = new Vector3(newBody.Position.x, newBody.Position.y);
        prevBodies = newBody;
      }
    }

    GameObject foodGrabObj = Instantiate(this.sFoodGrabPref);
    if (collParent) foodGrabObj.transform.SetParent(collParent.transform);

    SnakeState state = new SnakeState(
        new FoodGrabber(
            new Vector2(foodPos.x, foodPos.y),
            ARENA_DEFAULT_SIZE.FOOD_GRABBER,
            foodGrabObj
            ),
        bodies,
        moveDir,
        new Vector2(),
        GENERAL_CONFIG.SPEED,
        "",
        false,
        null,
        null
        );

    PlayerSkin? skinData = isBot
      ? skinSelect?.GetEnemySkinData()
      : skinSelect?.GetPlayerSkinData();

    SnakeConfig player = new SnakeConfig(
    isBot ? ENEMY_ID : PLAYER_ID,
    state,
    isBot,
    true,
    isBot ? enemyRender : playerRender,
    isBot
    ? new Dictionary<BOT_ACTION, IBaseAction>{
            {BOT_ACTION.NORMAL, new NormalAction() },
            { BOT_ACTION.CHASE_PLAYER, new GoToPlayer() },
            { BOT_ACTION.EAT, new GoToFood()},
    }
    : null,
    new NormalAction()
    );

    PlayerList.Add(player);
    spikeVfx?.SetSnakes(PlayerList);

    if (isBot)
    {
      if (!enemyRender) return;

      enemyRender.SnakeType = skinData?.Type ?? SNAKE_TYPE.NORMAL;
      enemyRender.SetSnakeSkin(skinData?.SkinPrimary, true);
      enemyRender.SetSnakeSkin(skinData?.SkinSecond, false);

      if (!enemyRender.RendTex && enemyVfx)
      {
        enemyRender.RendTex = new RenderTexture(
          (int)ARENA_DEFAULT_SIZE.WIDTH,
          (int)ARENA_DEFAULT_SIZE.HEIGHT,
          Util.GetGraphicFormat(),
          Util.GetDepthFormat()
        );
        Util.ClearDepthRT(enemyRender.RendTex, new CommandBuffer(), true);
        enemyVfx.SetRendTex(enemyRender.RendTex);
      }
      enemyRender?.SetSnakeBody(bodies);

      enemyHead.gameObject.SetActive(true);
      enemyHead.UpdateStatus(moveDir, bodies[0].Position);
    }
    else
    {
      if (!playerRender) return;

      playerRender.SnakeType = skinData?.Type ?? SNAKE_TYPE.NORMAL;
      playerRender.SetSnakeSkin(skinData?.SkinPrimary, true);
      playerRender.SetSnakeSkin(skinData?.SkinSecond, false);
      if (!playerRender.RendTex && playerVfx)
      {
        playerRender.RendTex = new RenderTexture(
          (int)ARENA_DEFAULT_SIZE.WIDTH,
          (int)ARENA_DEFAULT_SIZE.HEIGHT,
          Util.GetGraphicFormat(),
          Util.GetDepthFormat()
        );
        Util.ClearDepthRT(playerRender.RendTex, new CommandBuffer(), true);
        playerVfx.SetRendTex(playerRender.RendTex);
      }
      playerRender?.SetSnakeBody(bodies);

      playerHead.gameObject.SetActive(true);
      playerHead.UpdateStatus(moveDir, bodies[0].Position);
    }

    HandleMovement(player.Id, new MovementOpts
    (
        moveDir,
        null,
        true
    ));

    if (isBot)
    {
      aiRenderer?.SetSnakeToDebug(player);
    }

    GameEvent.Instance.SnakeSpawn(isBot);
  }

  public void RemoveAllPlayers()
  {
    foreach (SnakeConfig player in PlayerList)
    {
      Destroy(player.State.FoodGrabber.Obj);
      foreach (SnakeBody b in player.State.Body)
      {
        Destroy(b.Obj);
      }
      foreach (FireBody b in player.FireState.Body)
      {
        fireSpawner.RemoveFire(b.Fire);
      }

      player.FireState.Body = new List<FireBody>();
      player.State.Body = new List<SnakeBody>();
    }

    PlayerList = new List<SnakeConfig>();
    enemyRender?.SetSnakeBody(new List<SnakeBody>());
    playerRender?.SetSnakeBody(new List<SnakeBody>());
    enemyVfx?.ClearRender();
    playerVfx?.ClearRender();
    playerHead.gameObject.SetActive(false);
    enemyHead.gameObject.SetActive(false);

    aiRenderer?.SetSnakeToDebug(null);
  }

  public DodgeObstacleData FindNearestPlayerTowardPoint(
    SnakeConfig currentPlayer,
    float radius
        )
  {
    DodgeObstacleData data = new DodgeObstacleData
    {
      Angles = new List<float>(),
      Nearest = float.MaxValue,
    };
    List<float> duplicateAngleDetection = new List<float>();
    List<float> detectedObstacleAngles = new List<float>();
    SnakeState state = currentPlayer.State;

    if (state.Body.Count <= 0) return data;

    SnakeBody botHead = state.Body[0];
    float nearest = float.MaxValue;

    foreach (SnakeConfig otherPlayer in PlayerList)
    {
      if (otherPlayer.Id == currentPlayer.Id) continue;

      int idxLen = otherPlayer.State.Body.Count;
      for (int i = 1; i < idxLen; i++)
      {
        bool detectOtherPlayer = isCircleOverlap(
          botHead.Position.x,
          botHead.Position.y,
          otherPlayer.State.Body[i].Position.x,
          otherPlayer.State.Body[i].Position.y,
          radius,
          ARENA_DEFAULT_SIZE.SNAKE
        );

        if (detectOtherPlayer)
        {
          Vector2 snakeDir = new Vector2(botHead.Velocity.x, botHead.Velocity.y);
          float headAngle = Mathf.Atan2(snakeDir.y, snakeDir.x);
          float headInDegree = headAngle * Mathf.Rad2Deg;

          float obstacleAngle = Mathf.Atan2(
            botHead.Position.y - otherPlayer.State.Body[i].Position.y,
            botHead.Position.x - otherPlayer.State.Body[i].Position.x
          );
          float angleInDegree = obstacleAngle * Mathf.Rad2Deg;
          float finalAngle = headInDegree < 0 ? (Mathf.Abs(headInDegree) + angleInDegree) : (360 - (headInDegree - angleInDegree));
          finalAngle %= 360;
          finalAngle = finalAngle < 0 ? (360 + finalAngle) : finalAngle;

          float currDist = Vector2.Distance(otherPlayer.State.Body[i].Position, botHead.Position);
          if (currDist < nearest)
          {
            nearest = currDist;
          }

          if (duplicateAngleDetection.FindIndex((a) => a == obstacleAngle) == -1)
          {
            duplicateAngleDetection.Add(obstacleAngle);
            detectedObstacleAngles.Add(finalAngle);
          }
        }
      }
    }

    data.Angles = detectedObstacleAngles;
    data.Nearest = nearest;
    return data;
  }

  public DodgeObstacleData FindNearestProjNearPlayer(
   SnakeConfig currentPlayer,
   float radius
        )
  {
    DodgeObstacleData data = new DodgeObstacleData
    {
      Angles = new List<float>(),
      Nearest = float.MaxValue,
    };

    List<float> duplicateAngleDetection = new List<float>();
    List<float> detectedObstacleAngles = new List<float>();
    SnakeState state = currentPlayer.State;

    if (state.Body.Count <= 0) return data;

    SnakeBody botHead = state.Body[0];
    List<FireBody> fires = new List<FireBody>();
    foreach (SnakeConfig otherPlayer in PlayerList)
    {
      if (otherPlayer.Id == currentPlayer.Id) continue;

      fires.AddRange(otherPlayer.FireState.Body);
    }

    float nearest = float.MaxValue;

    foreach (FireBody fire in fires)
    {
      bool detectFire = isCircleOverlap(
        botHead.Position.x,
        botHead.Position.y,
        fire.Position.x,
        fire.Position.y,
        radius,
        ARENA_DEFAULT_SIZE.SNAKE
      );

      if (detectFire)
      {
        Vector2 snakeDir = new Vector2(botHead.Velocity.x, botHead.Velocity.y);
        float headAngle = Mathf.Atan2(snakeDir.y, snakeDir.x);
        float headInDegree = headAngle * Mathf.Rad2Deg;

        float obstacleAngle = Mathf.Atan2(
          botHead.Position.y - fire.Position.y,
          botHead.Position.x - fire.Position.x
        );
        float angleInDegree = obstacleAngle * Mathf.Rad2Deg;
        float finalAngle = headInDegree < 0 ? (Mathf.Abs(headInDegree) + angleInDegree) : (360 - (headInDegree - angleInDegree));
        finalAngle %= 360;
        finalAngle = finalAngle < 0 ? (360 + finalAngle) : finalAngle;

        float currDist = Vector2.Distance(fire.Position, botHead.Position);
        if (currDist < nearest)
        {
          nearest = currDist;
        }

        if (duplicateAngleDetection.FindIndex((a) => a == obstacleAngle) == -1)
        {
          duplicateAngleDetection.Add(obstacleAngle);
          detectedObstacleAngles.Add(finalAngle);
        }
      }
    }

    data.Angles = detectedObstacleAngles;
    data.Nearest = nearest;
    return data;
  }

  SnakeBody? createBody(
    bool isBot,
    bool isHead,
    Vector2 pos
  )
  {
    if (!sBodyPref) return null;

    GameObject bodyObj = Instantiate(this.sBodyPref);
    bodyObj.transform.localPosition = new Vector3(pos.x, pos.y);
    bodyObj.SetActive(true);
    LAYER group = isBot
      ? LAYER.PHYSICS_ENEMY_BODIES
      : LAYER.PHYSICS_PLAYER_BODIES;
    if (isHead)
    {
      group = isBot ? LAYER.PHYSICS_ENEMY : LAYER.PHYSICS_PLAYER;
    }
    Rigidbody2D rigidbody = bodyObj.GetComponentInChildren<Rigidbody2D>();
    if (rigidbody)
    {
      rigidbody.gameObject.layer = (int)group;
    }
    CircleCollider2D collider = bodyObj.GetComponentInChildren<CircleCollider2D>();
    if (collider)
    {
      collider.gameObject.layer = (int)group;
      collider.radius = ARENA_DEFAULT_SIZE.SNAKE / 4;
    }
    if (collParent) bodyObj.transform.SetParent(this.collParent.transform);

    Vector2 newPos = new Vector2(pos.x, pos.y);

    SnakeBody newBody = new SnakeBody(
      newPos,
      ARENA_DEFAULT_SIZE.SNAKE,
      new List<Vector2>(),
      new Vector2(0, 0),
      bodyObj
    );

    return newBody;
  }

  bool isCircleOverlap(
    float x1,
    float y1,
    float x2,
    float y2,
    float r1,
    float r2
    )
  {
    float deltaX = x2 - x1;
    float deltaY = y2 - y1;
    return Mathf.Pow(deltaX, 2) + Mathf.Pow(deltaY, 2) <= Mathf.Pow(r1 + r2, 2);
  }

  public Vector2 GetPlayerDirection(string id)
  {
    Vector2 dir = new Vector2();
    SnakeConfig player = PlayerList.Find((otherPlayer) =>
    {
      return otherPlayer.Id == id;
    });

    if (player != null)
    {
      dir = new Vector2(player.State.Body[0].Velocity.x, player.State.Body[0].Velocity.y);
      dir.Normalize();
    }

    return dir;
  }

  public void UpdateDirection(SnakeConfig player, Vector2 botNewDir)
  {
    if (player == null) return;

    Vector2 currDir = GetPlayerDirection(player.Id);

    player.State.MovementDir = new Vector2(botNewDir.x, botNewDir.y);
    List<Vector2> dirArray = new List<Vector2>();
    float remaining = Mathf.Abs(Vector2.SignedAngle(currDir, botNewDir));

    Vector2 newDir = new Vector2();
    for (
      int limit = 0;
      (Mathf.Abs(remaining) > 5) && limit < 6;
      limit++
    )
    {
      newDir =
        TurnRadiusModification(
          player,
          new Vector2(botNewDir.x, botNewDir.y),
          player.IsBot ? BOT_CONFIG.GetConfig().TURN_RADIUS : 0,
          remaining,
          currDir
        ) ?? new Vector2(0, 0);
      if (newDir == null) break;
      currDir = new Vector2(newDir.x, newDir.y);
      remaining = Vector2.SignedAngle(currDir, botNewDir);
      dirArray.Add(newDir);
    }

    if (dirArray.Count <= 0) return;

    float startTime = Time.time;
    if (player.State.RotationQueue.Count > 0)
    {
      startTime = player.State.RotationQueue[0].TimeToRun;
    }

    int idx = 0;
    player.State.RotationQueue.Clear();
    dirArray.ForEach((item) =>
    {
      float schedule = startTime + idx * TIME_CONFIG.TURNING_FRAME;
      player.State.RotationQueue.Add(new SnakeRotationData(
        schedule,
        item
      ));
      idx++;
    });
  }

  public Vector2? TurnRadiusModification(
    SnakeConfig player,
    Vector2 newMovement,
    float turnRadius,
    float remaining,
    Vector2? coorDir
  )
  {
    coorDir = coorDir != null ? coorDir : GetPlayerDirection(player.Id);
    if (coorDir == null) return null;

    float turnDeg = turnRadius * 30 + 30;
    Vector2 currDir = new Vector2(coorDir.Value.x, coorDir.Value.y);
    Vector2 newDir = new Vector2(newMovement.x, newMovement.y);
    float orientation = Util.GetOrientationBetweenVector(currDir, newDir);
    float turnAngle = turnDeg * orientation;
    float finalAngle = (Mathf.Abs(remaining) < turnAngle) ? (Mathf.Abs(remaining) * orientation) : turnAngle;
    Vector2 result = Util.RotateFromDegree(currDir, finalAngle);
    return result;
  }

  public void HandleMovement(
    string playerId,
    MovementOpts? option
)
  {
    SnakeConfig? player = PlayerList.Find((x) => x.Id == playerId);

    if (player == null) return;

    SnakeState pState = player.State;
    List<SnakeBody> physicBody = pState.Body;
    if (physicBody.Count <= 0) return;

    Vector2 newVelo = new Vector2(pState.MovementDir.x, pState.MovementDir.y);
    if (option != null)
    {
      if (option.Direction != null)
      {
        Vector2 normalized = new Vector2(option.Direction.Value.x, option.Direction.Value.y);
        normalized.Normalize();

        newVelo = normalized;
      }
    }
    if (pState.InputDirection == null) pState.InputDirection = new Vector2(0, 0);
    pState.InputDirection.Set(newVelo.x, newVelo.y);

    if (option?.InitialMovement == true)
    {
      if (option.Direction != null)
      {
        physicBody[0].Velocity = new Vector2(option.Direction.Value.x, option.Direction.Value.y);
      }

      for (int i = 1; i < physicBody.Count; i++)
      {
        physicBody[i].Velocity = new Vector2(0, 0);
      }
    }
    else
    {
      physicBody[0].Velocity = newVelo;
    }
  }

  // defaulted to 60 fps
  public void UpdateCoordinate(float delta = 0.016f)
  {
    float TILE = ARENA_DEFAULT_SIZE.TILE;
    float SNAKE = ARENA_DEFAULT_SIZE.SNAKE;
    for (int i = 0; i < PlayerList.Count; i++)
    {
      SnakeConfig snake = PlayerList[i];
      SnakeState snakeState = snake.State;
      float headX = 0f;
      float headY = 0f;

      for (int ii = 0; ii < snakeState.Body.Count; ii++)
      {
        SnakeBody bodyState = snakeState.Body[ii];
        float tempheadX = bodyState.Position.x;
        float tempheadY = bodyState.Position.y;
        Vector2 prevPos = new Vector2(tempheadX, tempheadY);
        Vector2 finalPos = new Vector2(0, 0);

        if (bodyState != null)
        {
          if (ii != 0)
          {
            float totalDist = 0;
            Vector2 lastPos = new Vector2(headX, headY);
            for (int b = bodyState.MovementQueue.Count - 1; b >= 0; b--)
            {
              float dist = Vector2.Distance(lastPos, bodyState.MovementQueue[b]);
              totalDist += dist;

              if (totalDist > SNAKE * 0.5)
              {
                if (bodyState.Obj) bodyState.Obj.transform.localPosition = new Vector3(
                  bodyState.MovementQueue[b].x,
                  bodyState.MovementQueue[b].y
                );
                bodyState.MovementQueue = Util.Slice(bodyState.MovementQueue, b + 1, bodyState.MovementQueue.Count - 1);
                break;
              }
              lastPos.Set(
                bodyState.MovementQueue[b].x,
                bodyState.MovementQueue[b].y
              );
            }

            string coordName = AStarFunctions.GetStringCoordName(new Vector2(
                bodyState.Obj?.transform.localPosition.x ?? headX,
                bodyState.Obj?.transform.localPosition.y ?? headY
            ));
            snakeState.CoordName = coordName;
            bodyState.MovementQueue.Add(new Vector2(
                headX,
                headY
            ));
          }

          if (ii == 0)
          {
            Vector2 headPos = bodyState.Position;

            Vector2 snakeDir = new Vector2(bodyState.Velocity.x, bodyState.Velocity.y);
            Vector2 newDir = snakeDir * new Vector2(TILE * delta * snakeState.Speed, TILE * delta * snakeState.Speed);

            if (bodyState.Obj) bodyState.Obj.transform.localPosition = new Vector3(
              headPos.x + newDir.x,
              headPos.y + newDir.y
            );
            Vector2 foodGrabberPos = getFoodGrabberPosition(bodyState);

            if (snakeState.FoodGrabber.Obj) snakeState.FoodGrabber.Obj.transform.localPosition = new Vector3(
              foodGrabberPos.x,
              foodGrabberPos.y
            );
            snakeState.FoodGrabber.Position = new Vector2(
              foodGrabberPos.x,
              foodGrabberPos.y
            );

            SnakeHead head = snake.IsBot ? enemyHead : playerHead;
            head.UpdateStatus(snakeDir, new Vector2(headPos.x + newDir.x, headPos.y + newDir.y));
          }

          finalPos = bodyState.Obj?.transform.localPosition ?? new Vector3(0, 0);
          bodyState.Position = new Vector2(finalPos.x, finalPos.y);

          for (int y = -1; y <= 1; y++)
          {
            for (int x = -1; x <= 1; x++)
            {
              arenaManager?.I.RemoveMapBody(new Vector2(prevPos.x + TILE * x, prevPos.y + TILE * y), snake.Id);
              arenaManager?.I.SetMapBody(
                  new Vector2(finalPos.x + TILE * x, finalPos.y + TILE * y),
                  snake.Id
              );
            }
          }
        }
        headX = tempheadX;
        headY = tempheadY;
      }
    }

    enemyRender?.Render();
    playerRender?.Render();
  }

  public void UpdateFire(float delta = 0.016f)
  {
    float TILE = ARENA_DEFAULT_SIZE.TILE;
    for (int i = 0; i < PlayerList.Count; i++)
    {
      SnakeConfig snake = PlayerList[i];
      FireState state = snake.FireState;

      List<FireBody> nonExpiredFire = new List<FireBody>();

      for (int ii = 0; ii < state.Body.Count; ii++)
      {
        FireBody bodyState = state.Body[ii];
        Vector2 prevPos = new Vector2(bodyState.Position.x, bodyState.Position.y);

        if (Time.time - bodyState.SpawnTime > GENERAL_CONFIG.FIRE_ALIVE_TIME)
        {
          fireSpawner.RemoveFire(bodyState.Fire);
          for (int y = -1; y <= 1; y++)
          {
            for (int x = -1; x <= 1; x++)
            {
              arenaManager?.I.RemoveMapBody(new Vector2(prevPos.x + TILE * x, prevPos.y + TILE * y), snake.Id);
            }
          }
          continue;
        }
        else
        {
          nonExpiredFire.Add(bodyState);
        }

        Vector2 dir = bodyState.Dir;
        Vector2 norm = new Vector2(dir.x, dir.y);
        norm.Normalize();
        Vector2 newDir = norm * new Vector2(TILE * delta * bodyState.Speed, TILE * delta * bodyState.Speed);
        Vector2 finalPos = new Vector2(prevPos.x + newDir.x, prevPos.y + newDir.y);

        if (bodyState.Fire) bodyState.Fire.transform.localPosition = new Vector3(
          finalPos.x,
          finalPos.y
        );

        bodyState.Position = new Vector2(
          finalPos.x,
          finalPos.y
        );
        for (int y = -1; y <= 1; y++)
        {
          for (int x = -1; x <= 1; x++)
          {
            arenaManager?.I.RemoveMapBody(new Vector2(prevPos.x + TILE * x, prevPos.y + TILE * y), snake.Id);
            arenaManager?.I.SetMapBody(
                new Vector2(finalPos.x + TILE * x, finalPos.y + TILE * y),
                snake.Id
            );
          }
        }
      }

      state.Body = nonExpiredFire;
    }
  }

  public void UpdateSnakeHeadSprite(SnakeConfig snake, float nearest)
  {
    SnakeHead head = snake.Id == PLAYER_ID ? playerHead : enemyHead;
    head.UpdateHeadSprite(nearest);
  }

  private void onGameOver(GameOverData data)
  {
    SnakeHead head = data.IsWon ? enemyHead : playerHead;
    head.UpdateHeadSprite(-1f);
  }

  void onTouchMove(Vector2 delta)
  {
    SnakeConfig? player = GetMainPlayer();
    if (player == null) return;
    UpdateDirection(player, delta);
  }

  void onSizeIncrease(SnakeConfig snake)
  {
    int length = snake.State.Body.Count;
    SnakeBody lastBody = snake.State.Body[length - 1];
    Vector3 pos = lastBody.Obj?.transform.position ?? new Vector3();
    SnakeBody? newBody = createBody(
      snake.IsBot,
      false,
      new Vector2(pos.x, pos.y)
    );
    snake.FoodInStomach++;

    if (newBody == null) return;

    snake.State.Body.Add(newBody);

    IEnumerator<object>? anim;
    eatAnim.TryGetValue(snake.Id, out anim);

    if (anim != null) StopCoroutine(anim);

    BaseTween<object> data = new BaseTween<object>(
        0.2f,
        null,
        (dist, _) =>
        {

        },
        (dist, _) =>
        {
          Material? mat = null;
          if (snake.IsBot)
          {
            mat = enemyRender?.Mat;
          }
          else
          {
            mat = playerRender?.Mat;
          }
          float ratio = 1 - Mathf.Abs(2 * dist - 1);
          mat?.SetFloat("_eatRatio", ratio);
        },
        (dist, _) =>
        {
          Material? mat = null;
          if (snake.IsBot)
          {
            mat = enemyRender?.Mat;
          }
          else
          {
            mat = playerRender?.Mat;
          }
          mat?.SetFloat("_eatRatio", 0);
        }
    );
    anim = Tween.Create(data);
    eatAnim.TryAdd(snake.Id, anim);

    if (snake.Id == PLAYER_ID)
    {
      GameEvent.Instance.MainPlayerEat(Mathf.Min((float)snake.FoodInStomach / GENERAL_CONFIG.FOOD_TO_FIRE, 1));
      AudioManager.Instance.PlaySFX(ASSET_KEY.SFX_EAT);
    }

    StartCoroutine(anim);
  }

  void onTouchStart(Vector2 _)
  {
    float delta = Time.time - lastTouchStart;
    lastTouchStart = Time.time;
    if (delta <= intervalToFire)
    {
      GameEvent.Instance.SnakeFire(GetMainPlayer());
    }
  }

  void onSnakeFire(SnakeConfig snake)
  {
    if (snake == null || snake.FoodInStomach < GENERAL_CONFIG.FOOD_TO_FIRE || snake.State.Body.Count <= 0) return;

    snake.FoodInStomach = 0;
    bool isMainPlayer = snake.Id == PLAYER_ID;

    if (isMainPlayer)
    {
      GameEvent.Instance.MainPlayerFire(0);
    }

    AudioManager.Instance.PlaySFX(ASSET_KEY.SFX_FIRE);

    SnakeBody head = snake.State.Body[0];
    Vector2 headPos = head.Position;
    for (int i = 0; i < GENERAL_CONFIG.FIRE_PER_SHOT; i++)
    {
      Fire fire = fireSpawner.Spawn(headPos, isMainPlayer);
      FireBody body = new FireBody(headPos, head.Velocity, fire, (i + 1) * snake.State.Speed * 2, Time.time);
      snake.FireState.Body.Add(body);
    }
  }

  Vector2 getFoodGrabberPosition(SnakeBody head)
  {
    Vector2 norm = new Vector2(head.Velocity.x, head.Velocity.y);
    norm.Normalize();
    float x = norm.x * head.Radius + head.Position.x;
    float y = norm.y * head.Radius + head.Position.y;

    return new Vector2(x, y);
  }

  public SnakeConfig? GetMainPlayer()
  {
    return GetPlayerById(PLAYER_ID);
  }

  public SnakeConfig? GetEnemy()
  {
    return GetPlayerById(ENEMY_ID);
  }

  public SnakeConfig? GetPlayerById(string id)
  {
    return PlayerList.Find((item) => item.Id == id);
  }

  public SnakeConfig GetPlayerByBody(GameObject gameObj)
  {
    return PlayerList.Find((item) =>
      item.State.Body.Find((body) => ReferenceEquals(body.Obj, gameObj)) != null
    );
  }

  public SnakeConfig GetPlayerByFoodGrabber(GameObject gameObj)
  {
    return PlayerList.Find((item) => ReferenceEquals(item.State.FoodGrabber.Obj, gameObj));
  }

}
