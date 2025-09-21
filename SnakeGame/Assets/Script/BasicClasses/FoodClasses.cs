using UnityEngine;

public class FoodConfig
{
  public string Id { get; set; }
  public FoodState State { get; set; }
  public int GridIndex { get; set; }
  public GameObject Object { get; set; }

  public FoodConfig(
    string Id,
    FoodState State,
    int GridIndex,
    GameObject Object
  )
  {
    this.Id = Id;
    this.State = State;
    this.GridIndex = GridIndex;
    this.Object = Object;
  }
}

public class FoodState
{
  public Vector2 Position { get; set; }
  public bool Eaten { get; set; }

  public FoodState(
    Vector2 Position,
    bool Eaten
  )
  {
    this.Position = Position;
    this.Eaten = Eaten;
  }
}
