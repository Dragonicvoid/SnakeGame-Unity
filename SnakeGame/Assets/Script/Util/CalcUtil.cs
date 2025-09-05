using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

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
}
