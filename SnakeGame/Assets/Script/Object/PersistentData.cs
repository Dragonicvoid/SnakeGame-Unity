using System;
using System.Collections.Generic;
using UnityEngine;

public class PersistentData : MonoBehaviour
{
  public static PersistentData _instance;
  public static PersistentData Instance
  {
    get
    {
      if (!_instance)
      {
        var obj = new GameObject("PersistentData");
        DontDestroyOnLoad(obj);
        _instance = obj.AddComponent<PersistentData>();
      }
      return _instance;
    }
  }

  public int SelectedMap = 0;

  private bool isLoading;

  List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
}

public class GameplayMoveEvent : MonoBehaviour
{
  public static GameplayMoveEvent _instance;
  public static GameplayMoveEvent Instance
  {
    get
    {

      GameObject obj = GameObject.FindGameObjectWithTag("Event");

      if (!obj || !_instance)
      {
        var newObj = new GameObject("CustomEvent");
        newObj.tag = "Event";
        DontDestroyOnLoad(newObj);
        _instance = newObj.AddComponent<GameplayMoveEvent>();
      }
      return _instance;
    }
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
