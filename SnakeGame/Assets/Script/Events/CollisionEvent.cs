using System;
using UnityEngine;

public class CollisionEvent : MonoBehaviour
{
    public static CollisionEvent Instance;

    void Awake()
    {
        Instance = this;
    }

    public event Action<HeadCollideData> onHeadCollide;
    public void HeadCollide(HeadCollideData data)
    {
        if (onHeadCollide != null)
            onHeadCollide(data);
    }

    public event Action<FoodCollideData> onFoodCollide;
    public void FoodCollide(FoodCollideData data)
    {
        if (onFoodCollide != null)
            onFoodCollide(data);
    }
}
