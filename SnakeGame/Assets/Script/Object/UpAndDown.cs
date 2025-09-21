#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public class UpAndDown : MonoBehaviour
{
  struct TweenData
  {
    public Vector2 target;
    public Vector2 start;
    public Action<TweenData> repeatAction;
  }

  [SerializeField]
  GameObject? sprite = null;
  [SerializeField]
  float height = 20f;
  [SerializeField]
  float duration = 1.2f;

  Vector3? initPos = null;

  Coroutine? anim = null;

  int mult = 1;

  void Awake()
  {
    if (!sprite) return;

    initPos = new Vector3(sprite.transform.position.x, sprite.transform.position.y);
  }

  void OnEnable()
  {
    TweenData data = new TweenData
    {
      start = new Vector2(0, -height),
      target = new Vector2(0, height),
      repeatAction = animate,
    };

    animate(data);
  }

  void animate(TweenData data)
  {
    BaseTween<TweenData> obj = new BaseTween<TweenData>(
      duration,
      data,
      (dist, data) =>
      {
        if (initPos == null || sprite == null) return;

        sprite.transform.localPosition = new Vector3(
          initPos.Value.x,
          Mathf.Floor(initPos.Value.y + Mathf.Floor((mult < 0 ? data.start.y : data.target.y) / 2) * 2)
        );
      },
      (dist, data) =>
      {
        if (initPos == null || sprite == null) return;

        float start = mult < 0 ? data.start.y : data.target.y;
        float target = mult < 0 ? data.target.y : data.start.y;

        float delta = target - start;
        target = start + (delta * dist);
        sprite.transform.localPosition = new Vector3(
          initPos.Value.x,
          Mathf.Floor(initPos.Value.y + Mathf.Floor(target / 2) * 2)
        );
      },
      (dist, data) =>
      {
        mult *= -1;
        data.repeatAction(data);
      }
    );
    IEnumerator<object> enumerator = Tween.Create(obj);
    anim = StartCoroutine(enumerator);
  }

  void OnDestroy()
  {
    if (anim != null)
    {
      StopCoroutine(anim);
      anim = null;
    }
  }
}
