using System;
using UnityEngine;

public class CollisionEvent : MonoBehaviour
{
    public static CollisionEvent _instance;

    public static CollisionEvent Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CollisionEvent();
            }
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
    }

    public event Action<HeadCollideData>? onHeadCollide;
    public void HeadCollide(HeadCollideData data)
    {
        if (onHeadCollide != null)
            onHeadCollide(data);
    }

    public event Action<FoodCollideData>? onFoodCollide;
    public void FoodCollide(FoodCollideData data)
    {
        if (onFoodCollide != null)
            onFoodCollide(data);
    }
}
