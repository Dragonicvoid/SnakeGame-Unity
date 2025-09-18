#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IPlayerManager
{
  [SerializeField]
  IArenaManager? arenaManager = null;

  [SerializeField]
  SnakeRender? playerRender = null;

  [SerializeField]
  SnakeRender? enemyRender = null;

  [SerializeField]
  CustomSprite? playerDisplay = null;

  [SerializeField]
  CustomSprite? enemyDisplay = null;

  [SerializeField]
  SkinSelect? skinSelect = null;

  [SerializeField]
  GameObject? collParent = null;

  [SerializeField]
  GameObject? sBodyPref = null;

  [SerializeField]
  GameObject? sFoodGrabPref = null;

  [SerializeField]
  AiRenderer? aiRenderer = null;

  public List<SnakeConfig> PlayerList { set; get; }

  string PLAYER_ID = "MAIN_PLAYER";

  string ENEMY_ID = "ENEMY";

  Dictionary<string, IEnumerator<object>> eatAnim;

  void Awake()
  {
    PlayerList = new List<SnakeConfig>();
    eatAnim = new Dictionary<string, IEnumerator<object>>();
    GameplayMoveEvent.Instance.onSnakeMoveCalculated += onTouchMove;
  }

  void Update()
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
        25f,
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

    if (isBot)
    {
      if (!enemyRender) return;

      enemyRender.SnakeType = skinData?.Type ?? SNAKE_TYPE.NORMAL;
      enemyRender.SetSnakeSkin(skinData?.Skin, true);

      if (!enemyRender.RendTex && enemyDisplay)
      {
        enemyRender.RendTex = new RenderTexture(
          (int)ARENA_DEFAULT_SIZE.WIDTH,
          (int)ARENA_DEFAULT_SIZE.HEIGHT,
          UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
          UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat_S8_UInt
        );
        enemyDisplay.Texture = enemyRender.RendTex;
      }
      enemyRender?.SetSnakeBody(bodies);
    }
    else
    {
      if (!playerRender) return;

      playerRender.SnakeType = skinData?.Type ?? SNAKE_TYPE.NORMAL;
      playerRender.SetSnakeSkin(skinData?.Skin, true);
      if (!playerRender.RendTex && playerDisplay)
      {
        playerRender.RendTex = new RenderTexture(
          (int)ARENA_DEFAULT_SIZE.WIDTH,
          (int)ARENA_DEFAULT_SIZE.HEIGHT,
          UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
          UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat_S8_UInt
        );
        playerDisplay.Texture = playerRender.RendTex;
      }
      playerRender?.SetSnakeBody(bodies);
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
      aiRenderer?.SetupScheduler();
    }
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
      player.State.Body = new List<SnakeBody>();
    }

    PlayerList = new List<SnakeConfig>();
    enemyRender?.SetSnakeBody(new List<SnakeBody>());
    playerRender?.SetSnakeBody(new List<SnakeBody>());

    aiRenderer?.SetSnakeToDebug(null);
  }

  public List<float> FindNearestPlayerTowardPoint(
    SnakeConfig currentPlayer,
    float radius
        )
  {
    List<float> duplicateAngleDetection = new List<float>();
    List<float> detectedObstacleAngles = new List<float>();
    SnakeState state = currentPlayer.State;

    if (state.Body.Count <= 0) return new List<float>();

    SnakeBody botHeadPos = state.Body[0];

    foreach (SnakeConfig otherPlayer in PlayerList)
    {
      if (otherPlayer.Id == currentPlayer.Id) return new List<float>();

      int idxLen = otherPlayer.State.Body.Count;
      for (int i = 1; i < idxLen; i++)
      {
        bool detectOtherPlayer = isCircleOverlap(
          botHeadPos.Position.x,
          botHeadPos.Position.y,
          otherPlayer.State.Body[i].Position.x,
          otherPlayer.State.Body[i].Position.y,
          radius,
          ARENA_DEFAULT_SIZE.SNAKE
        );

        if (detectOtherPlayer)
        {
          float obstacleAngle = Mathf.Atan2(
            botHeadPos.Position.y - otherPlayer.State.Body[i].Position.y,
            botHeadPos.Position.x - otherPlayer.State.Body[i].Position.x

          );
          if (duplicateAngleDetection.FindIndex((a) => a == obstacleAngle) == -1)
          {
            duplicateAngleDetection.Add(obstacleAngle);
            float angleInDegree = (obstacleAngle * 180) / Mathf.PI;
            detectedObstacleAngles.Add(angleInDegree);
          }
        }
      }
    }

    return detectedObstacleAngles;
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
      collider.radius = ARENA_DEFAULT_SIZE.SNAKE / 2;
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
      dir = player.State.MovementDir;
    }

    return dir;
  }

  public void HandleMovement(
    string playerId,
    MovementOpts? option
)
  {
    if (option?.Speed < 0) return;

    SnakeConfig? player = PlayerList.Find((x) => x.Id == playerId);

    if (player == null) return;

    SnakeState pState = player.State;
    List<SnakeBody> physicBody = pState.Body;
    Vector2 movDir = pState.MovementDir;
    if (physicBody.Count <= 0) return;

    if (option != null)
    {
      if (option.Direction != null)
      {
        Vector2 normalized = new Vector2(option.Direction.Value.x, option.Direction.Value.y);
        normalized.Normalize();

        movDir.x = normalized.x * pState.Speed;
        movDir.y = normalized.y * pState.Speed;
      }
    }
    if (pState.InputDirection == null) pState.InputDirection = new Vector2(0, 0);
    pState.InputDirection.Set(movDir.x, movDir.y);

    Vector2 velocity = new Vector2(movDir.x, movDir.y);

    if (option?.InitialMovement != null)
    {
      physicBody[0].Velocity = new Vector2(velocity.x, velocity.y);
      if (player.IsBot && option.Direction != null)
        pState.MovementDir = new Vector2(option.Direction.Value.x, option.Direction.Value.y);
    }
    else
    {
      physicBody[0].Velocity = new Vector2(velocity.x, velocity.y);

      if (
        1 >= physicBody.Count &&
        (physicBody[1].Velocity.x != 0 || physicBody[1].Velocity.y != 0)
      )
      {
        for (int i = 1; i < physicBody.Count; i++)
        {
          physicBody[1].Velocity = new Vector2(0, 0);
        }
      }
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
            Vector2 newDir = snakeDir * new Vector2(TILE * delta, TILE * delta);

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
          }

          finalPos = bodyState.Obj?.transform.localPosition ?? new Vector3(0, 0);
          bodyState.Position = new Vector2(finalPos.x, finalPos.y);

          for (int y = -1; y <= 1; y++)
          {
            for (int x = -1; x <= 1; x++)
            {
              arenaManager?.RemoveMapBody(new Vector2(prevPos.x + TILE * x, prevPos.y + TILE * y), snake.Id);
              arenaManager?.SetMapBody(
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

  void onTouchMove(Vector2 delta)
  {
    HandleMovement(PLAYER_ID, new MovementOpts
    (
        delta,
        null,
        null
    ));
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

    StartCoroutine(anim);
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
