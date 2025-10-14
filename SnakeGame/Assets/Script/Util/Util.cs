using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public static class Util
{
  public static T Pop<T>(this List<T> list)
  {
    if (list.Count <= 0) return default;

    int index = list.Count - 1;
    T r = list[index];
    list.RemoveAt(index);
    return r;
  }
  public static List<T> Slice<T>(List<T> inputList, int startIndex, int endIndex)
  {
    int elementCount = endIndex - startIndex + 1;
    return inputList.Skip(startIndex).Take(elementCount).ToList();
  }

  public static List<T> AddToIndex<T>(List<T> inputList, int idx, T value)
  {
    List<T> firstHalf = Slice(inputList, 0, idx - 1);
    firstHalf.Add(value);
    List<T> secondHalf = Slice(inputList, idx, inputList.Count - 1);
    List<T> newList = new List<T>(firstHalf);
    newList.AddRange(secondHalf);

    return newList;
  }

  public static List<T> RemoveFromIdx<T>(List<T> inputList, int idx)
  {
    List<T> firstHalf = Slice(inputList, 0, idx - 1);
    List<T> secondHalf = ((idx + 1) >= inputList.Count) ? new List<T>() : Slice(inputList, idx + 1, inputList.Count - 1);
    List<T> newList = new List<T>(firstHalf);
    newList.AddRange(secondHalf);

    return newList;
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

  /**
 * Checks whether vec2 is on the left (returns +1) or on the right (returns -1) side of vec1
 * @param vec1
 * @param vec2
 * @returns +1 or -1
 */
  public static float GetOrientationBetweenVector(Vector2 vec1, Vector2 vec2)
  {
    float value = vec1.x * vec2.y - vec1.y * vec2.x;
    float sign = Mathf.Sign(value);
    return sign == 0f ? 1f : sign;
  }

  public static float CalculateDistanceBetweenTwoDots(
    float x1,
    float y1,
    float x2,
    float y2
  )
  {
    float deltaX = x1 - x2;
    float deltaY = y1 - y2;
    return CalculateVectorMagnitude(deltaX, deltaY);
  }

  public static float CalculateVectorMagnitude(float x, float y)
  {
    return Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
  }

  /**
   * Checks axis-aligned bounding box collision between 2 boxes
   * Assumption: origin is (0.5, 0.5)
   * @param x1zero
   * @param y1zero
   * @param width1
   * @param height1
   * @param x2zero
   * @param y2zero
   * @param width2
   * @param height2
   * @returns true if collision happens, false if no collision happens
   */
  public static bool CheckAABBCollision(
    float x1,
    float y1,
    float width1,
    float height1,
    float x2,
    float y2,
    float width2,
    float height2
  )
  {
    /**
     * x1 and y1 with origin (0, 0)
     */
    float x1zero = x1 - width1 * 0.5f;
    float y1zero = y1 - height1 * 0.5f;

    /**
     * x2 and y2 with origin (0, 0)
     */
    float x2zero = x2 - width2 * 0.5f;
    float y2zero = y2 - height2 * 0.5f;

    bool collisionX = x1zero + width1 >= x2zero && x2zero + width2 >= x1zero;
    bool collisionY = y1zero + height1 >= y2zero && y2zero + height2 >= y1zero;

    return collisionX && collisionY;
  }

  public static float GetCircleOverlap(
    float x1,
    float y1,
    float radius1,
    float x2,
    float y2,
    float radius2
  )
  {
    float distance = CalculateDistanceBetweenTwoDots(x1, y1, x2, y2);

    return radius1 + radius2 - distance;
  }

  public static bool CheckCircleCollision(
    float x1,
    float y1,
    float radius1,
    float x2,
    float y2,
    float radius2
  )
  {
    return GetCircleOverlap(x1, y1, radius1, x2, y2, radius2) >= 0;
  }

  public static Vector2 GetPositionInsideRadius(float maxRadius, Vector2 coord)
  {
    float randomRadius = GetRandomIntInRange(maxRadius, 0);
    float angle = UnityEngine.Random.Range(0, (float)Math.PI * 2f);

    return new Vector2(
      Mathf.Cos(angle) * randomRadius + coord.x,
      Mathf.Sin(angle) * randomRadius + coord.y
    );
  }

  public static float GetRandomIntInRange(float max, float min)
  {
    return Mathf.Floor(UnityEngine.Random.Range(0, max - min + 1)) + min;
  }

  /**
  * 0 degree is from vector2 (0, 1); 
  * Moving counter clockwise
  */
  public static Vector2 RotateFromDegree(Vector2 from, float angleInDeg)
  {
    float actualAngle = angleInDeg;
    float radian = Mathf.Deg2Rad * actualAngle;

    float[,] mat = { { Mathf.Cos(radian), -Mathf.Sin(radian) }, { Mathf.Sin(radian), Mathf.Cos(radian) } };
    Vector2 res = new Vector2(from.x * mat[0, 0] + from.y * mat[0, 1], from.x * mat[1, 0] + from.y * mat[1, 1]);

    return res;
  }

  public static void DumpToConsole(object obj)
  {
    var output = JsonUtility.ToJson(obj, true);
    Debug.Log(output);
  }

  public static Texture2D ToTexture2D(RenderTexture rTex)
  {
    Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
    RenderTexture.active = rTex;
    tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
    tex.Apply();
    return tex;
  }

  public static float EaseOut(float x, float pow)
  {
    return 1 - Mathf.Pow(1 - x, pow);
  }

  // credit: https://discussions.unity.com/t/how-do-i-calculate-a-view-matrix-using-matrix4x4-lookat/246263/2
  public static Matrix4x4 CreateViewMatrix(Vector3 pos, Quaternion rot, Vector3 scale)
  {
    Matrix4x4 viewMatrix = Matrix4x4.TRS(pos, rot, scale).inverse;
    if (SystemInfo.usesReversedZBuffer)
    {
      viewMatrix.m20 = -viewMatrix.m20;
      viewMatrix.m21 = -viewMatrix.m21;
      viewMatrix.m22 = -viewMatrix.m22;
      viewMatrix.m23 = -viewMatrix.m23;
    }

    return viewMatrix;
  }

  public static GraphicsFormat GetGraphicFormat()
  {
    if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat))
    {
      return GraphicsFormat.R32G32B32A32_SFloat;
    }

    return GraphicsFormat.R8G8B8A8_UNorm;
  }

  public static GraphicsFormat GetDepthFormat()
  {
    return GraphicsFormat.D32_SFloat_S8_UInt;
  }

  // Dist is 0-1
  // return -1 if array count is not positive integer
  public static int GetArrayIdxByDist(float dist, int arrayCount)
  {
    if (arrayCount <= 0)
    {
      return -1;
    }

    if (dist >= 1)
    {
      return arrayCount - 1;
    }

    float perArray = 1f / arrayCount;
    return Mathf.FloorToInt(dist / perArray);
  }

  public static void ClearDepthRT(RenderTexture rt, CommandBuffer cmdBuffer, bool run = false)
  {
    if (run) cmdBuffer.Clear();
    cmdBuffer.SetRenderTarget(rt);
    cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);

    ClearWebViewScreen(cmdBuffer);

    if (run) Graphics.ExecuteCommandBuffer(cmdBuffer);
  }

  // Serialize save file
  public static string Serialize<T>(this T toSerialize)
  {
    XmlSerializer xml = new XmlSerializer(typeof(T));
    StringWriter writer = new StringWriter();
    xml.Serialize(writer, toSerialize);
    return writer.ToString();
  }

  public static T Deserialize<T>(this string toDeserialize)
  {
    XmlSerializer xml = new XmlSerializer(typeof(T));
    StringReader reader = new StringReader(toDeserialize);
    return (T)xml.Deserialize(reader);
  }

  // Return in s
  public static long GetCurrWorldTime()
  {
    DateTime currentTime = DateTime.UtcNow;
    long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
    return unixTime;
  }

  // Hack for web view since it uses last RT size
  // but does not automatically returns it to original size
  public static void ClearWebViewScreen(CommandBuffer cmdBuffer)
  {
    if (PersistentData.Instance)
    {
      cmdBuffer.SetRenderTarget(PersistentData.Instance.RenderTex);
      cmdBuffer.ClearRenderTarget(false, false, Color.clear, 1f);
    }
  }
}