using System.Collections.Generic;
using UnityEngine;

public class GoToPlayer : BaseAction
{
    public override float Cooldown { get; set; } = 2f;

    public override float ForceRun { get; set; } = 0f;
    public override BOT_ACTION MapKey { get; set; } = BOT_ACTION.CHASE_PLAYER;

    public float StartTime = 0;

    float aggresiveDuration = 0;

    float forceToChangePath = 2f;

    float aggresiveStartTStamp = 0f;

    float aggresiveEndsTStamp = 0f;

    bool hasEnterPlayerCone = false;

    bool headingLeft = false;

    bool firstTime = true;
    bool isAggresive = false;

    public GoToPlayer() : base()
    {
        StartTime = Time.time;
    }

    public override void Run(SnakeConfig player, SnakeActionData data)
    {
        this.Player = player;
        CurrData = data;

        ManagerActionData manager = data.Manager;
        if (manager == null) return;

        IArenaManager? arenaManager = manager.ArenaManager;
        IPlayerManager? playerManager = manager.PlayerManager;

        if (arenaManager == null || playerManager == null) return;

        Vector2 newDir = new Vector2(0, 0);

        SnakeConfig? mainPlayer = playerManager.GetMainPlayer();

        if (mainPlayer == null) return;

        float TILE = ARENA_DEFAULT_SIZE.TILE;
        Vector2 frontRay = mainPlayer.State.Body[0].Velocity;
        Vector2 currHeadPos = player.State.Body[0].Position;
        Vector2 mainPlayerHead = mainPlayer.State.Body[0].Position;

        if (Vector2.Distance(mainPlayerHead, currHeadPos) < TILE * 5)
        {
            // Be more accurate if near target
            ResetPathData();
        }
        else
        {
            headingLeft = isLeft(
              frontRay,
              new Vector2(
                currHeadPos.x - mainPlayerHead.x,
                currHeadPos.y - mainPlayerHead.y
              )
            );
        }

        float maxVelo =
          Mathf.Abs(frontRay.x) > Mathf.Abs(frontRay.y)
            ? Mathf.Abs(frontRay.x)
            : Mathf.Abs(frontRay.y);
        float[] vecX = { frontRay.x / maxVelo, frontRay.y / maxVelo, 0 };
        float[] vecY = { 0, 0, -1 };
        float[] leftVec = crossProd(vecX, vecY);
        float[] rightVec = crossProd(vecY, vecX);

        float[] normLeft = normalizeV3(leftVec);
        float[] normRight = normalizeV3(rightVec);
        float[] normFront = normalizeV3(new float[] { frontRay.x, frontRay.y, 0 });

        Vector2 leftSideHead = new Vector2(
      mainPlayerHead.x + normLeft[0] * TILE * 2,
      mainPlayerHead.y + normLeft[1] * TILE * 2
        );
        Vector2 rightSideHead = new Vector2(
      mainPlayerHead.x + normRight[0] * TILE * 2,
      mainPlayerHead.y + normRight[1] * TILE * 2
        );

        Vector2 mainPlayerSide = new Vector2(
      normFront[0] * TILE * 2,
      normFront[1] * TILE * 2
        );

        Vector2 frontPlayerSide = new Vector2(
       mainPlayerHead.x + normFront[0] * TILE * 4,
       mainPlayerHead.y + normFront[1] * TILE * 4
        );

        Vector2 otherPlayerSide = new Vector2(
      normFront[0] * TILE * 4,
      normFront[1] * TILE * 4
        );
        if (headingLeft)
        {
            mainPlayerSide.x += leftSideHead.x;
            mainPlayerSide.y += leftSideHead.y;

            otherPlayerSide.x += rightSideHead.x;
            otherPlayerSide.y += rightSideHead.y;
        }
        else
        {
            mainPlayerSide.x += rightSideHead.x;
            mainPlayerSide.y += rightSideHead.y;

            otherPlayerSide.x += leftSideHead.x;
            otherPlayerSide.y += leftSideHead.y;
        }

        Vector2 targetPos = new Vector2(
      mainPlayerSide.x,
      mainPlayerSide.y
        );

        AStarResultData? path = GetPath(Player.State.Body[0].Position, targetPos, new List<Vector2>{
          mainPlayerSide,
            frontPlayerSide,
            otherPlayerSide,
        });

        if (path == null || path.Result.Count <= 0) return;

        newDir = ProcessBotMovementByTarget(Player, path.Result[0]);

        if (newDir == null) return;

        playerManager.UpdateDirection(Player, newDir);
    }

    public override float UpdateScore(PlannerFactor factor)
    {
        Score = ACTION_SCORE.GO_TO_PLAYER_DEFAULT;
        SnakeConfig player = factor.Player;
        List<SnakeConfig> playerList = factor.PlayerList;

        if (player == null || playerList == null) return Score;

        if (firstTime)
        {
            Cooldown = BOT_CONFIG.GetConfig().AGGRESSIVE_COOLDOWN;
            aggresiveDuration = BOT_CONFIG.GetConfig().AGGRRESIVE_TIME;
            firstTime = false;
            aggresiveEndsTStamp = Cooldown > 0 ? (Cooldown * -1) : 0;
        }

        SnakeConfig? mainPlayer = playerList.Find((p) =>
        {
            return !p.IsBot;
        });

        if (mainPlayer == null) return Score;

        bool inPlayerCone = IsInPlayerAggresiveCone(mainPlayer, player);

        // Check if its the first time enemy enter zone
        // if yes randomize attack chance
        if (inPlayerCone && !hasEnterPlayerCone && !isAggresive)
        {
            hasEnterPlayerCone = true;
        }
        else if (!inPlayerCone && hasEnterPlayerCone)
        {
            hasEnterPlayerCone = false;
        }

        // needs to be check before cone check to diferrentiate bot normal
        // and on aggressive cooldown
        if (!isValidToBeAggresive(playerList, player, null)) return Score;

        if (!inPlayerCone && !isAggresive) return Score;

        float currentTStamp = Time.time;
        float deltaTStamp = currentTStamp - aggresiveStartTStamp;
        // lose aggresion after duration
        if (isAggresive && (deltaTStamp > aggresiveDuration))
        {
            isAggresive = false;
            aggresiveEndsTStamp = currentTStamp;
            return Score;
        }

        Score += ACTION_SCORE.BECOME_AGGRESIVE;

        if (deltaTStamp < forceToChangePath)
        {
            return Score;
        }

        // becoming aggresive
        if (!isAggresive)
        {
            aggresiveStartTStamp = currentTStamp;
        }
        isAggresive = true;
        ResetPathData();

        return Score;
    }

    private float[] crossProd(float[] a, float[] b)
    {
        if (a.Length != 3 || b.Length != 3) return new float[] { 0, 0, 0 };

        return new float[] {
        a[1] * b[2] - a[2] * b[1],
      a[2] * b[0] - a[0] * b[2],
      a[0] * b[1] - a[1] * b[0],
    };
    }

    private float[] normalizeV3(float[] vec)
    {
        float magtitute = MagV3(vec);

        if (magtitute > 0)
        {
            return new float[] { vec[0] / magtitute, vec[1] / magtitute, vec[2] / magtitute };
        }

        return vec;
    }

    private bool isLeft(Vector2 targetVec, Vector2 currVec)
    {
        return Util.GetOrientationBetweenVector(currVec, targetVec) >= 0;
    }

    bool isValidToBeAggresive(
      List<SnakeConfig> playerList,
      SnakeConfig currPlayer,
      float? currTime
    )
    {
        if (currTime == null)
        {
            currTime = Time.time;
        }

        for (int i = 0; i < playerList.Count; i++)
        {
            SnakeConfig player = playerList[i];

            if (!player.IsBot || !player.IsAlive) continue;

            IBaseAction? goToPlayerAct = null;
            player.PossibleActions?.TryGetValue(
              BOT_ACTION.CHASE_PLAYER,
              out goToPlayerAct
            );

            if (
              goToPlayerAct is GoToPlayer &&
              (goToPlayerAct as GoToPlayer).isAggresive
          )
            {
                if (currPlayer.Id == player.Id)
                {
                    return true;
                }
            }
        }

        float deltaTStamp = currTime.Value - aggresiveEndsTStamp;
        if (deltaTStamp < Cooldown && !isAggresive)
        {
            Score += ACTION_SCORE.AGGRESIVE_ON_COOLDOWN;
            return false;
        }

        return true;
    }

    public override void OnChange()
    {
        ResetPathData();
    }
}
