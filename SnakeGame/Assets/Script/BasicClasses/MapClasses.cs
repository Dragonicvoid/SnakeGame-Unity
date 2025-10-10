using System.Collections.Generic;

public class Coordinate
{
  public int X { get; set; }
  public int Y { get; set; }

  public Coordinate(int X, int Y)
  {
    this.X = X;
    this.Y = Y;
  }
}

public class TileMapData
{
  public float X { get; set; }
  public float Y { get; set; }
  public ARENA_OBJECT_TYPE Type { get; set; }
  public List<string> PlayerIDList { get; set; }
  public int? GridIdx { get; set; }

  public TileMapData(
    float X,
    float Y,
    ARENA_OBJECT_TYPE Type,
    List<string> PlayerIDList,
    int? GridIdx
  )
  {
    this.X = X;
    this.Y = Y;
    this.Type = Type;
    this.PlayerIDList = PlayerIDList;

    if (GridIdx != null)
    {
      this.GridIdx = GridIdx;
    }
  }
}

public class LevelMapData
{
  public int Row { get; set; }
  public int Col { get; set; }
  public List<int> Maps { get; set; }

  public LevelMapData(
    int Row,
    int Col,
    List<int> Maps
  )
  {
    this.Row = Row;
    this.Col = Col;
    this.Maps = Maps;
  }
}
