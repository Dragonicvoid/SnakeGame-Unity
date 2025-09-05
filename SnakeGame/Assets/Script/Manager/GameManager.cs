using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public ArenaManager arenaManager = null;
  public GridManager gridManager = null;
  public PlayerManager playerManager = null;
  public FoodManager foodManager = null;
  private UIManager uiManager = null;
  private ArenaInput inputField = null;
  private BotPlanner planner = null;

  private float botInterval = 0;

  private float gameStartTime = 0;

  private DIFFICULTY diff = DIFFICULTY.NORMAL;
  void Start()
  {

  }

  public void StartGame()
  {
    arenaManager.initializedMap();
    gameStartTime = Time.fixedTime;
    uiManager.ShowStartUI(false);
    inputField.StartInputListener();

    CreatePlayer();
    SetCollisionEvent();
    SetGameEvent();

    Action gameUpdateCb = () =>
    {
      float deltaTime = Math.Min(0.016f, Time.deltaTime);

      foreach (SnakeConfig snake in playerManager.playerList)
      {
        HandleBotLogic(snake);
        PlayerManager.UpdateCoordinate(deltaTime);
      }
    };
    InvokeRepeating("gameUpdateCb", 0f, 0.016f);
  }

  private void CreatePlayer()
  {
    Vector2 centerPos = arenaManager?.centerPos ?? new Vector2(0, 0);

    float rand = Random.Range(0, 100) / 100;
    Vector2 playerPos =
      arenaManager?.spawnPos[rand > 0.5 ? 0 : 1] ?? new Vector2(0, 0);
    Vector2 playerDir = new Vector2(1, 0);
    if (playerPos.x > centerPos.x)
    {
      playerDir.Set(-1, 0);
    }
    PlayerManager.CreatePlayer(playerPos, playerDir);

    Vector2 enemyPos =
      ArenaManager?.SpawnPos[rand > 0.5 ? 1 : 0] ?? new Vector2(0, 0);
    Vector2 enemyDir = new Vector2(1, 0);
    if (enemyPos.x > centerPos.x)
    {
      enemyDir.Set(-1, 0);
    }
    PlayerManager?.createPlayer(enemyPos, enemyDir, true);

    FoodManager?.startSpawningFood();
  }

  // Update is called once per frame
  void Update()
  {

  }
}
