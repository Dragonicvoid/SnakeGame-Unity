using System;
using UnityEngine;

public class GameEvent : MonoBehaviour
{
    public static GameEvent Instance;

    void Awake()
    {
        Instance = this;
    }

    public event Action<SnakeConfig> onPlayerSizeIncrease;
    public void PlayerSizeIncrease(SnakeConfig snake)
    {
        onPlayerSizeIncrease(snake);
    }

    public event Action<GameOverData> onGameOver;
    public void GameOver(GameOverData data)
    {
        onGameOver(data);
    }
}
