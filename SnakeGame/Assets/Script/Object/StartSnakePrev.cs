
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class StartSnakePrev : MonoBehaviour
{
  class TweenData
  {
    public SnakeBody Body;

    public float Offset;

    public TweenData(float offset, SnakeBody body)
    {
      Offset = offset;
      Body = body;
    }
  }

  [SerializeField]
  SnakeRender? snakeRender = null;

  [SerializeField]
  RenderTexture? renTex = null;
  [SerializeField]
  Image? headImage = null;

  float snakeSize = ARENA_DEFAULT_SIZE.SNAKE;

  float danceLength = ARENA_DEFAULT_SIZE.SNAKE * 0.1f;

  List<SnakeBody> snakeShape;

  float duration = 1.65f;

  public SkinDetail? SkinDataPrim = null;

  public SkinDetail? SkinDataSecond = null;

  public SNAKE_TYPE SnakeType = SNAKE_TYPE.NORMAL;

  Vector2 front = Vector2.up;

  void Awake()
  {
    RectTransform rect = GetComponent<RectTransform>();
    renTex = new RenderTexture(
      (int)rect.rect.width,
      (int)rect.rect.height,
      Util.GetGraphicFormat(),
      Util.GetDepthFormat()
    );
    Util.ClearDepthRT(renTex, new CommandBuffer(), true);
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
    startSnakeDance();
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

  void startSnakeDance()
  {
    for (int i = 0; i < snakeShape.Count; i++)
    {
      float offset = i * 0.25f;
      float sinVal = Mathf.Sin(offset * Mathf.PI * 2f);
      snakeShape[i].Position = new Vector2(
        sinVal * danceLength,
        snakeShape[i].Position.y
      );
      TweenData data = new TweenData(offset, snakeShape[i]);
      animDance(data);
    }
  }

  void animDance(TweenData obj)
  {
    BaseTween<TweenData> newTween = new BaseTween<TweenData>(
    duration,
    obj,
    (dist, data) =>
    {
      float sinVal = Mathf.Sin((data.Offset + dist) * Mathf.PI * 2);

      data.Body.Position = new Vector2(
              sinVal * danceLength,
              data.Body.Position.y
          );
    },
    (dist, data) =>
    {
      float sinVal = Mathf.Sin((data.Offset + dist) * Mathf.PI * 2);
      Vector2 pos = new Vector2(
        sinVal * danceLength,
        data.Body.Position.y
      );

      if (data.Offset <= 0)
      {
        updateHeadStatus(snakeShape[1].Position - snakeShape[0].Position, pos);
      }

      data.Body.Position = pos;
    },
    (dist, data) =>
    {
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

  void updateHeadStatus(Vector2 dir, Vector2 pos)
  {
    updateAngle(dir);
    updatePos(pos);
  }

  void updateAngle(Vector2 dir)
  {
    float angle = Mathf.Atan2(front.y, front.x) - Mathf.Atan2(dir.y, dir.x);
    angle += Mathf.PI;

    headImage.rectTransform.eulerAngles = new Vector3(0, 0, (Mathf.PI * 2 - angle) * Mathf.Rad2Deg);
  }

  void updatePos(Vector2 pos)
  {
    headImage.rectTransform.anchoredPosition = new Vector2(pos.x, pos.y);
  }
}
