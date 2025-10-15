using System;
using UnityEngine;

public class GameplayMoveEvent : MonoBehaviour
{
    public static GameplayMoveEvent _instance;

    public static GameplayMoveEvent Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameplayMoveEvent();
            }
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
    }

    public event Action<Vector2>? onGameUiStartTouch;
    public void GameUiStartTouch(Vector2 touchPos)
    {
        if (onGameUiStartTouch != null)
            onGameUiStartTouch(touchPos);
    }

    public event Action<Vector2>? onGameUiMoveTouch;
    public void GameUiMoveTouch(Vector2 touchPos)
    {
        if (onGameUiMoveTouch != null)
            onGameUiMoveTouch(touchPos);
    }

    public event Action? onGameUiEndTouch;
    public void GameUiEndTouch()
    {
        if (onGameUiEndTouch != null)
            onGameUiEndTouch();
    }

    public event Action<Vector2>? onSnakeMoveCalculated;
    public void SnakeMoveCalculated(Vector2 dir)
    {
        if (onSnakeMoveCalculated != null)
            onSnakeMoveCalculated(dir);
    }
}
