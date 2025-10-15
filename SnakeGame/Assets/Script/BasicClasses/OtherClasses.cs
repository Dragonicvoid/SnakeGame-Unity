using System.Collections.Generic;
using UnityEngine;

public class ObstacleData
{
  public GameObject Parent { get; set; }
  public Vector2 Position { get; set; }
  public GameObject? Obj { get; set; }

  public ObstacleData(
    GameObject Parent,
    Vector2 Position,
    GameObject? Obj
  )
  {
    this.Parent = Parent;
    this.Position = Position;
    if (Obj)
    {
      this.Obj = Obj;
    }
  }
}

public struct DodgeObstacleData
{
  public List<float> Angles { get; set; }
  public float Nearest { get; set; }
}