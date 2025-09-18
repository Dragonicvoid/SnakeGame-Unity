#nullable enable
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


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

  [SerializeField]
  RenderTexture? renTex = null;

  float snakeSize = ARENA_DEFAULT_SIZE.SNAKE;

  List<IEnumerator<float>> tween;

  float danceLength = ARENA_DEFAULT_SIZE.SNAKE * 0.1f;

  List<SnakeBody> snakeShape;

  float duration = 0.5f;

  public SkinDetail? SkinData = null;

  public SNAKE_TYPE SnakeType = SNAKE_TYPE.NORMAL;

  void Awake()
  {
    RectTransform rect = GetComponent<RectTransform>();
    renTex = new RenderTexture(
      (int)rect.rect.width,
      (int)rect.rect.height,
      UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
      UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat_S8_UInt
    );
    if (snakeRender != null && renTex != null)
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
      snakeRender.SetSnakeBody(snakeShape);
      snakeRender.RendTex = renTex;
      RawImage rawImage = GetComponent<RawImage>();
      rawImage.texture = renTex;
    }
  }

  void OnEnable()
  {
    StartCoroutine(startSnakeDance());
    StartCoroutine(renderSnake());
  }

  IEnumerator<object> renderSnake()
  {
    while (true)
    {
      yield return new WaitForSeconds(0.016f);
      snakeRender?.Render();
    }
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
