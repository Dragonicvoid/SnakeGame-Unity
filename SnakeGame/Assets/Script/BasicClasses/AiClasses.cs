using System;
using System.Collections.Generic;
using UnityEngine;

public class PlannerFactor
{
  public SnakeConfig Player { get; set; }
  public List<SnakeConfig> PlayerList { get; set; }
  public List<float> DetectedPlayer { get; set; }
  public List<float> DetectedWall { get; set; }
  public Vector2 CurrCoord { get; set; }
  public FoodConfig? DetectedFood { get; set; }

  public PlannerFactor(
    SnakeConfig Player,
    List<SnakeConfig> PlayerList,
    List<float> DetectedPlayer,
    List<float> DetectedWall,
    Vector2 CurrCoord,
    FoodConfig? DetectedFood
  )
  {
    this.Player = Player;
    this.PlayerList = PlayerList;
    this.DetectedPlayer = DetectedPlayer;
    this.DetectedWall = DetectedWall;
    this.CurrCoord = CurrCoord;

    if (DetectedFood != null)
    {
      this.DetectedFood = DetectedFood;
    }
  }
}

[Serializable]
public class AStarPointData
{
  public AStarVector Point { get; set; }
  public AStarPointData? PrevPoint { get; set; }
  public float CurrGoal { get; set; }
  public float CurrHeuristic { get; set; }
  public float CurrF
  {
    get
    {
      return CurrGoal + CurrHeuristic;
    }
  }

  public AStarPointData(Vector2 origin, Vector2 target)
  {
    CurrHeuristic = Vector2.Distance(origin, target);
    Point = new AStarVector(origin.x, origin.y);
  }
}

public class AStarResultData
{
  public List<Vector2> Result { get; set; }
  public AStarSearchData? Data { get; set; }

  public AStarResultData(List<Vector2> Result, AStarSearchData? Data)
  {
    this.Result = Result;
    if (Data != null)
    {
      this.Data = Data;
    }
  }
}

public class AStarSearchData
{
  public List<AStarPointData> OpenList { get; set; }
  public List<AStarPointData> CloseList { get; set; }
  public Dictionary<string, AStarPointData> MemoiPoint { get; set; }
  public AStarPointData? PathFound { get; set; }

  public AStarSearchData(
    List<AStarPointData> OpenList,
    List<AStarPointData> CloseList,
    Dictionary<string, AStarPointData> MemoiPoint,
    AStarPointData? PathFound
  )
  {
    this.OpenList = OpenList;
    this.CloseList = CloseList;
    this.MemoiPoint = MemoiPoint;
    if (PathFound != null)
    {
      this.PathFound = PathFound;
    }
  }
}

[Serializable]
public class AStarVector
{
  public float x { get; set; }

  public float y { get; set; }

  public AStarVector(float x, float y)
  {
    this.x = x;
    this.y = y;
  }

  public void Set(float x, float y)
  {
    this.x = x;
    this.y = y;
  }
}