using System;
using System.Collections.Generic;
using UnityEngine;

public static class Tween
{
  public static IEnumerator<object> Create<T>(BaseTween<T> data)
  {
    float elapsedTime = 0;
    data.OnStart(0, data.Obj);
    while (elapsedTime < data.Duration)
    {
      data.Dist = elapsedTime / data.Duration;
      elapsedTime += Time.deltaTime;
      data.OnUpdate(data.Dist, data.Obj);
      yield return data.Dist;
    }

    data.Dist = 1f;
    data.OnComplete(data.Dist, data.Obj);
  }

  public static IEnumerator<object> AnimateShakeWithPos<T>(
    float intensity,
    float duration,
    T obj,
    Action<float, T, Vector3> onStart,
    Action<float, T, Vector3> onUpdate,
    Action<float, T, Vector3> onFinish
    )
  {
    Vector2[] star = new Vector2[6] {
      new Vector2(0f, 0f),
      new Vector2(0f, 1f),
      new Vector2(0.75f, -1f),
      new Vector2(-1f, 0.75f),
      new Vector2(1f, 0.75f),
      new Vector2(-0.75f, -1f),
    };
    BaseTween<T> tweenData = new BaseTween<T>(
      duration,
      obj,
      (dist, obj) =>
      {
        Vector2 lastPos = star[0];
        Vector3 currPos = new Vector3(lastPos.x * intensity, lastPos.x * intensity, 0);
        onStart(dist, obj, currPos);
      },
      (dist, obj) =>
      {
        float distPerSegment = 1.0f / star.Length;
        int i = Mathf.FloorToInt(dist / distPerSegment);

        if (i >= star.Length) return;
        int nextI = ((i + 1) >= star.Length) ? 0 : (i + 1);

        Vector2 start = star[i];
        Vector2 end = star[nextI];
        Vector2 delta = end - start;
        float f = dist % distPerSegment;

        Vector2 currPos = new Vector2((start.x + delta.x * f) * intensity, (start.y + delta.y * f) * intensity);

        onUpdate(dist, obj, currPos);
      },
      (dist, obj) =>
      {
        Vector2 lastPos = star[0];
        Vector3 currPos = new Vector3(lastPos.x * intensity, lastPos.x * intensity, 0);
        onFinish(dist, obj, currPos);
      }
    );

    IEnumerator<object> tween = Create(tweenData);
    return tween;
  }
}
