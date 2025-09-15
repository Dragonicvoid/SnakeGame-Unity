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
}
