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

  public SkinDetail? SkinDataPrim = null;

  public SkinDetail? SkinDataSecond = null;

  public SNAKE_TYPE SnakeType = SNAKE_TYPE.NORMAL;

  void Awake()
  {
    RectTransform rect = GetComponent<RectTransform>();
    renTex = new RenderTexture(
      (int)rect.rect.width,
      (int)rect.rect.height,
      Util.GetGraphicFormat(),
      Util.GetDepthFormat()
    );
    if (snakeRender != null && renTex != null)
    {
      snakeShape = new List<SnakeBody>();
      for (int i = 3; i >= -3; i--)
      {
        snakeShape.Add(new SnakeBody(
          new Vector2(0, snakeSize * 0.75f * i),
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
      yield return PersistentData.Instance.GetWaitSecond(0.016f);
      snakeRender?.Render();
    }
  }

  IEnumerator<object> startSnakeDance()
  {
    for (int i = 0; i < snakeShape.Count; i++)
    {
      TweenData data = new TweenData((i % 2) == 0 ? 1 : -1, snakeShape[i]);
      animDance(data);
      yield return PersistentData.Instance.GetWaitSecond(0.25f);
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

  public void SetSnakeSkin(SkinDetail skin, bool isPrimary)
  {
    if (SkinDataPrim == null || SkinDataSecond == null) return;

    if (isPrimary)
    {
      SkinDataPrim = skin;
    }
    else
    {
      SkinDataSecond = skin;
    }

    snakeRender?.SetSnakeSkin(skin, isPrimary);
  }
}
