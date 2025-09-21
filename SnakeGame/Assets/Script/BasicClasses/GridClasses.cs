using System.Collections.Generic;
using UnityEngine;

public class GridConfig
{
  public float X1 { get; set; }
  public float X2 { get; set; }
  public float Y1 { get; set; }
  public float Y2 { get; set; }
  public float MidX { get; set; }
  public float MidY { get; set; }
  public List<FoodConfig> Foods { get; set; }
  public List<SpikeConfig> Spikes { get; set; }
  public Dictionary<string, int> ChickBodies { get; set; }

  public GridConfig(
    float X1,
    float X2,
    float Y1,
    float Y2,
    float MidX,
    float MidY,
    List<FoodConfig> Foods,
    List<SpikeConfig> Spikes,
    Dictionary<string, int> ChickBodies
  )
  {
    this.X1 = X1;
    this.X2 = X2;
    this.Y1 = Y1;
    this.Y2 = Y2;
    this.MidX = MidX;
    this.MidY = MidY;
    this.Foods = Foods;
    this.Spikes = Spikes;
    this.ChickBodies = ChickBodies;
  }
};

public class SpikeConfig
{
  public int GridIndex;
  public Vector2 Position;

  public SpikeConfig(
    int GridIndex,
    Vector2 Position
  )
  {
    this.GridIndex = GridIndex;
    this.Position = Position;
  }
};
