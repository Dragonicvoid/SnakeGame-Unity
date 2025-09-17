using System;
using UnityEngine;

public class GameplayMoveEvent : MonoBehaviour
{
    public static GameplayMoveEvent Instance;

    void Awake()
    {
        Instance = this;
    }

    public event Action<Vector2> onGameUiStartTouch;
    public void GameUiStartTouch(Vector2 touchPos)
    {
        onGameUiStartTouch(touchPos);
    }

    public event Action<Vector2> onGameUiMoveTouch;
    public void GameUiMoveTouch(Vector2 touchPos)
    {
        onGameUiMoveTouch(touchPos);
    }

    public event Action onGameUiEndTouch;
    public void GameUiEndTouch()
    {
        onGameUiEndTouch();
    }

    public event Action<Vector2> onSnakeMoveCalculated;
    public void SnakeMoveCalculated(Vector2 dir)
    {
        onSnakeMoveCalculated(dir);
    }
}
