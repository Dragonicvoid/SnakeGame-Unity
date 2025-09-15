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

public class GameEvent : MonoBehaviour
{
  public static GameEvent _instance;
  public static GameEvent Instance
  {
    get
    {

      GameObject obj = GameObject.FindGameObjectWithTag("Event");

      if (!obj)
      {
        obj = new GameObject("CustomEvent");
        obj.tag = "Event";
        DontDestroyOnLoad(obj);
      }

      _instance = obj.GetComponent<GameEvent>();

      if (_instance)
      {
        _instance = obj.AddComponent<GameEvent>();
      }
      return _instance;
    }
  }

  public event Action<SnakeConfig> onPlayerSizeIncrease;
  public void PlayerSizeIncrease(SnakeConfig snake)
  {
    onPlayerSizeIncrease(snake);
  }
}

public class UiEvent : MonoBehaviour
{
  public static UiEvent _instance;
  public static UiEvent Instance
  {
    get
    {

      GameObject obj = GameObject.FindGameObjectWithTag("Event");

      if (!obj)
      {
        obj = new GameObject("CustomEvent");
        obj.tag = "Event";
        DontDestroyOnLoad(obj);
      }

      _instance = obj.GetComponent<UiEvent>();

      if (_instance)
      {
        _instance = obj.AddComponent<UiEvent>();
      }
      return _instance;
    }
  }

  public event Action<int> onSkinSelected;
  public void SkinSelected(int skinId)
  {
    onSkinSelected(skinId);
  }

  public event Action onPrevSkinDoneRender;
  public void PrevSkinDoneRender()
  {
    onPrevSkinDoneRender();
  }
}

public class GameplayMoveEvent : MonoBehaviour
{
  public static GameplayMoveEvent _instance;
  public static GameplayMoveEvent Instance
  {
    get
    {
      GameObject obj = GameObject.FindGameObjectWithTag("Event");

      if (!obj)
      {
        obj = new GameObject("CustomEvent");
        obj.tag = "Event";
        DontDestroyOnLoad(obj);
      }

      _instance = obj.GetComponent<GameplayMoveEvent>();

      if (_instance)
      {
        _instance = obj.AddComponent<GameplayMoveEvent>();
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
