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
        onHeadCollide(data);
    }

    public event Action<FoodCollideData> onFoodCollide;
    public void FoodCollide(FoodCollideData data)
    {
        onFoodCollide(data);
    }
}
