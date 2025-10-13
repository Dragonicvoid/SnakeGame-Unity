using System;
using UnityEngine;

public class GameEvent : MonoBehaviour
{
  public static GameEvent _instance;

  public static GameEvent Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = new GameEvent();
      }
      return _instance;
    }
  }

  void Awake()
  {
    _instance = this;
  }

  public event Action? onMainPlayerspawn;
  public event Action? onEnemySpawn;
  public void SnakeSpawn(bool isBot)
  {
    if (isBot)
    {
      if (onEnemySpawn != null)
      {
        onEnemySpawn();
      }
    }
    else
    {
      if (onMainPlayerspawn != null)
      {
        onMainPlayerspawn();
      }
    }
  }

  public event Action? onTutorialFinish;
  public void FinishTutorial()
  {
    if (onTutorialFinish != null)
      onTutorialFinish();
  }

  public event Action<SnakeConfig>? onPlayerSizeIncrease;
  public void PlayerSizeIncrease(SnakeConfig snake)
  {
    if (onPlayerSizeIncrease != null)
      onPlayerSizeIncrease(snake);
  }

  public event Action<float>? onMainPlayerEat;
  public void MainPlayerEat(float dist)
  {
    if (onMainPlayerEat != null)
      onMainPlayerEat(dist);
  }

  public event Action<GameOverData>? onGameOver;
  public void GameOver(GameOverData data)
  {
    if (onGameOver != null)
      onGameOver(data);
  }

  public event Action<SnakeConfig>? onSnakeFire;
  public void SnakeFire(SnakeConfig snake)
  {
    if (onSnakeFire != null)
      onSnakeFire(snake);
  }

  public event Action<float>? onMainPlayerFire;
  public void MainPlayerFire(float dist)
  {
    if (onMainPlayerFire != null)
      onMainPlayerFire(dist);
  }
}
