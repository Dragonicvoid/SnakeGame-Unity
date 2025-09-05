#nullable enable
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
public class AStarPoint
{
  public Vector2? Point { get; set; }
  public AStarPoint? PrevPoint { get; set; }
  public float CurrGoal { get; set; }
  public float CurrHeuristic { get; set; }
  public float CurrF
  {
    get
    {
      return CurrGoal + CurrHeuristic;
    }
  }

  public AStarPoint(Vector2 origin, Vector2 target)
  {
    CurrHeuristic = Vector2.Distance(origin, target);
    Point = origin;
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
  public List<AStarPoint> OpenList { get; set; }
  public List<AStarPoint> CloseList { get; set; }
  public Dictionary<string, AStarPoint> MemoiPoint { get; set; }
  public AStarPoint? PathFound { get; set; }

  public AStarSearchData(
    List<AStarPoint> OpenList,
    List<AStarPoint> CloseList,
    Dictionary<string, AStarPoint> MemoiPoint,
    AStarPoint? PathFound
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