#nullable enable
using System.Collections.Generic;
using UnityEngine;


public class StartSnakePrev : MonoBehaviour
{
  class TweenData
  {
    public int Mult;
    public SnakeBody Body;

    public TweenData(int mult, SnakeBody body)
    {
      Mult = mult;
      Body = body;
    }
  }

  [SerializeField]
  SnakeRender? snakeRender = null;

  float snakeSize = ARENA_DEFAULT_SIZE.SNAKE;

  List<IEnumerator<float>> tween;

  float danceLength = ARENA_DEFAULT_SIZE.SNAKE * 0.1f;

  List<SnakeBody> snakeShape;

  float duration = 0.5f;

  public SkinDetail? SkinData = null;

  public SNAKE_TYPE SnakeType = SNAKE_TYPE.NORMAL;

  void Awake()
  {
    snakeShape = new List<SnakeBody>();
    for (int i = 6; i >= -6; i--)
    {
      snakeShape.Add(new SnakeBody(
              new Vector2(0, snakeSize * i),
        snakeSize,
        new List<Vector2>(),
        new Vector2(),
        null
              ));
    }
    snakeRender?.SetSnakeBody(snakeShape);
    StartCoroutine(startSnakeDance());
  }

  IEnumerator<object> startSnakeDance()
  {
    for (int i = 0; i < snakeShape.Count; i++)
    {
      TweenData data = new TweenData((i % 2) == 0 ? 1 : -1, snakeShape[i]);
      animDance(data);
      yield return new WaitForSeconds(0.25f);
    }
  }

  void animDance(TweenData obj)
  {
    BaseTween<TweenData> newTween = new BaseTween<TweenData>(
    duration,
    obj,
    (dist, data) =>
    {
      float sinVal = (dist * 2 - 1) * data.Mult;

      data.Body.Position = new Vector2(
              sinVal * danceLength,
              data.Body.Position.y
          );
    },
    (dist, data) =>
    {
      float sinVal = (dist * 2 - 1) * data.Mult;

      data.Body.Position = new Vector2(
              sinVal * danceLength,
              data.Body.Position.y
          );
    },
    (dist, data) =>
    {
      data.Mult *= -1;
      animDance(obj);
    }
);
    StartCoroutine(Tween.Create(newTween));
  }
}
