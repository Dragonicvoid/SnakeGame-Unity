using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class Util
{
  public static List<T> Slice<T>(List<T> inputList, int startIndex, int endIndex)
  {
    int elementCount = endIndex - startIndex + 1;
    return inputList.Skip(startIndex).Take(elementCount).ToList();
  }

  public static T DeepCopy<T>(T other)
  {
    using (MemoryStream ms = new MemoryStream())
    {
      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(ms, other);
      ms.Position = 0;
      return (T)formatter.Deserialize(ms);
    }
  }

  public static List<T> Filter<T>(List<T> list, Func<T, bool> compare)
  {
    List<T> result = new List<T>();
    foreach (T data in list)
    {
      if (compare(data))
      {
        result.Add(data);
      }
    }

    return result;
  }

  public static List<string> GetQuery()
  {
#if (UNITY_WEBGL || UNITY_ANDROID) && !UNITY_EDITOR
      string parameters = Application.absoluteURL.Substring(Application.absoluteURL.IndexOf("?") + 1);
      return parameters.Split(new char[] { '&', '=' });
#else
    return new List<string>(Environment.GetCommandLineArgs());
#endif
  }

  public static bool ShouldDrawPathfinding()
  {
    return GetQuery().FindIndex((param) => param == "drawPathfinding") != -1;
  }

  public static bool IsCoordInsideMap(float x, float y)
  {
    float TILE = ARENA_DEFAULT_SIZE.TILE;
    int rows = Mathf.FloorToInt(ARENA_DEFAULT_SIZE.WIDTH / TILE);
    int cols = Mathf.FloorToInt(ARENA_DEFAULT_SIZE.HEIGHT / TILE);

    return !(x < 0 || x >= rows || y < 0 || y >= cols);
  }

  public static bool IsPosInsideMap(float x, float y)
  {
    Coordinate coord = ArenaConverter.ConvertPosToCoord(x, y);
    return IsCoordInsideMap(coord.X, coord.Y);
  }
}