using System;

public class BaseTween<T>
{
    public float Duration { set; get; }
    public Action<float, T> OnStart { set; get; }
    public Action<float, T> OnUpdate { set; get; }

    public Action<float, T> OnComplete { set; get; }
    public float Dist { set; get; }

    public T Obj { set; get; }


    public BaseTween(float duration, T obj, Action<float, T> onStart, Action<float, T> onUpdate, Action<float, T> onComplete)
    {
        Duration = duration;
        Obj = obj;
        OnStart = onStart;
        OnUpdate = onUpdate;
        OnComplete = onComplete;
        Dist = 0;
    }
}