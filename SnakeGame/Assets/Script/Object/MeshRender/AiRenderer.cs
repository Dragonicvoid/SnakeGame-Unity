#nullable enable 
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
struct AiRendererVertex
{
  public Vector3 Pos;
  public float IsLine;
  public Color Color;
  public half2 UV;
}

struct GraphicPos
{
  public Vector3 Pos;
  public Color Color;
}

struct LinePos
{
  public Vector3 Start;
  public Vector3 End;
  public Color Color;
}
public class AiRenderer : MonoBehaviour
{
  public float LineWidth = 3f;
  public float UpdateTime = 1;
  public Color MoveDirColor = Color.black;
  public Color VeloColor = Color.gray;
  public Color InputColor = Color.magenta;
  public Color PathColor = Color.blue;
  public Color OpenListColor = Color.white;
  public Color CloseListColor = Color.red;
  public Color WallColor = Color.green;
  public Color OccupyColor = Color.yellow;

  public List<bool> Mask = new List<bool> { true, true, true };

  float lastUpdateTime = 0f;

  List<GraphicPos> circlePos = new List<GraphicPos>();

  List<LinePos> linePos = new List<LinePos>();

  SnakeConfig? snake;

  List<List<TileMapData>> map = new List<List<TileMapData>>();

  Mesh? mesh;

  void Update()
  {
    if ((Time.time - lastUpdateTime) > UpdateTime)
    {
      updateDraw();
      lastUpdateTime = Time.time;
    }
  }

  void Awake()
  {
#if UNITY_EDITOR
    Camera.main.cullingMask |= 1 << ((int)LAYER.DEBUG);
#endif
  }

  void updateMeshRender()
  {
    MeshRenderer renderer = GetComponent<MeshRenderer>();

    if (!renderer)
    {
      renderer = gameObject.AddComponent<MeshRenderer>();
      Shader shader = Shader.Find("Debug/AiRenderer");
      renderer.material = new Material(shader);
    }

    if (!mesh)
    {
      mesh = new Mesh();
    }
    else
    {
      mesh.Clear();
    }

    int attrbTotal = 4;
    NativeArray<VertexAttributeDescriptor> layout = new NativeArray<VertexAttributeDescriptor>(attrbTotal, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    layout[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    layout[1] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 1);
    layout[2] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
    layout[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

    int vertexPerPos = 4;
    int vertexCount = (circlePos.Count + linePos.Count) * vertexPerPos;
    mesh.SetVertexBufferParams(vertexCount, layout);
    layout.Dispose();

    NativeArray<AiRendererVertex> verts = new NativeArray<AiRendererVertex>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

    half h0 = new half(0f); half h1 = new half(1f);

    for (int i = 0; i < circlePos.Count; i++)
    {
      int padding = i * vertexPerPos;
      float TILE = ARENA_DEFAULT_SIZE.TILE;
      float halfWidth = TILE / 2;
      float halfHeight = TILE / 2;

      GraphicPos graphic = circlePos[i];

      verts[padding] = new AiRendererVertex
      {
        Pos = new Vector3(graphic.Pos.x - halfWidth, graphic.Pos.y - halfHeight),
        IsLine = 0f,
        Color = graphic.Color,
        UV = new half2(h0, h0),
      };

      verts[padding + 1] = new AiRendererVertex
      {
        Pos = new Vector3(graphic.Pos.x - halfWidth, graphic.Pos.y + halfHeight),
        IsLine = 0f,
        Color = graphic.Color,
        UV = new half2(h0, h1),
      };

      verts[padding + 2] = new AiRendererVertex
      {
        Pos = new Vector3(graphic.Pos.x + halfWidth, graphic.Pos.y + halfHeight),
        IsLine = 0f,
        Color = graphic.Color,
        UV = new half2(h1, h1),
      };

      verts[padding + 3] = new AiRendererVertex
      {
        Pos = new Vector3(graphic.Pos.x + halfWidth, graphic.Pos.y - halfHeight),
        IsLine = 0f,
        Color = graphic.Color,
        UV = new half2(h1, h0),
      };
    }

    for (int i = 0; i < linePos.Count; i++)
    {
      int padding = (i + circlePos.Count) * vertexPerPos;
      float halfWidth = LineWidth / 2;

      LinePos graphic = linePos[i];

      verts[padding] = new AiRendererVertex
      {
        Pos = new Vector3(graphic.Start.x - halfWidth, graphic.Start.y),
        IsLine = 1f,
        Color = graphic.Color,
        UV = new half2(h0, h0),
      };

      verts[padding + 1] = new AiRendererVertex
      {
        Pos = new Vector3(graphic.End.x - halfWidth, graphic.End.y),
        IsLine = 1f,
        Color = graphic.Color,
        UV = new half2(h0, h1),
      };

      verts[padding + 2] = new AiRendererVertex
      {
        Pos = new Vector3(graphic.End.x + halfWidth, graphic.End.y),
        IsLine = 1f,
        Color = graphic.Color,
        UV = new half2(h1, h1),
      };

      verts[padding + 3] = new AiRendererVertex
      {
        Pos = new Vector3(graphic.Start.x + halfWidth, graphic.Start.y),
        IsLine = 1f,
        Color = graphic.Color,
        UV = new half2(h1, h0),
      };
    }

    mesh.SetVertexBufferData(verts, 0, 0, vertexCount);
    verts.Dispose();

    int indexPerPos = 6;
    int indexCount = (circlePos.Count + linePos.Count) * indexPerPos;
    mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

    NativeArray<int> indices = new NativeArray<int>(indexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

    for (int i = 0; i < circlePos.Count; i++)
    {
      int padding = i * indexPerPos;
      int vertPad = i * vertexPerPos;

      indices[padding] = vertPad;
      indices[padding + 1] = vertPad + 1;
      indices[padding + 2] = vertPad + 2;
      indices[padding + 3] = vertPad;
      indices[padding + 4] = vertPad + 2;
      indices[padding + 5] = vertPad + 3;
    }

    for (int i = 0; i < linePos.Count; i++)
    {
      int padding = (i + circlePos.Count) * indexPerPos;
      int vertPad = (i + circlePos.Count) * vertexPerPos;

      indices[padding] = vertPad;
      indices[padding + 1] = vertPad + 1;
      indices[padding + 2] = vertPad + 2;
      indices[padding + 3] = vertPad;
      indices[padding + 4] = vertPad + 2;
      indices[padding + 5] = vertPad + 3;
    }

    mesh.SetIndexBufferData(indices, 0, 0, indexCount);
    indices.Dispose();

    mesh.bounds = new Bounds
    {
      center = transform.localPosition,
      extents = new Vector3(700f / 2f, 700f / 2f)
    };
    mesh.subMeshCount = 1;
    mesh.SetSubMesh(0, new SubMeshDescriptor
    {
      indexStart = 0,
      indexCount = indexCount,
      topology = MeshTopology.Triangles,
      baseVertex = 0,
    });

    MeshFilter filter = GetComponent<MeshFilter>();
    if (!filter)
    {
      filter = gameObject.AddComponent<MeshFilter>();
    }
    filter.mesh = mesh;
  }

  private void updateDraw()
  {
    clearDataPath();

    if (Mask[0])
    {
      drawMap();
    }

    if (Mask[1])
    {
      drawPath();
    }

    if (Mask[2])
    {
      drawLine();
    }

    updateMeshRender();
  }

  void drawPath()
  {
    if (snake == null) return;

    List<Vector2> paths = snake.State.DebugData?.EnemyPath ?? new List<Vector2>();
    foreach (Vector2 p in paths)
    {
      drawTile(p, null);
    }

    List<AStarPointData> openList = snake.State.DebugData?.PathfindingState?.OpenList ?? new List<AStarPointData>();
    foreach (AStarPointData o in openList)
    {
      drawTile(new Vector2(o.Point.x, o.Point.y), OpenListColor);
    }

    List<AStarPointData> closeList = snake.State.DebugData?.PathfindingState?.CloseList ?? new List<AStarPointData>();
    foreach (AStarPointData c in closeList)
    {
      drawTile(new Vector2(c.Point.x, c.Point.y), CloseListColor);
    }
  }

  private void drawMap()
  {
    float TILE = ARENA_DEFAULT_SIZE.TILE;

    float arenaWidth = ARENA_DEFAULT_SIZE.WIDTH;
    float arenaHeight = ARENA_DEFAULT_SIZE.HEIGHT;

    float maxCoordX = Mathf.FloorToInt(arenaWidth / TILE);
    float maxCoordY = Mathf.FloorToInt(arenaHeight / TILE);

    for (int y = 0; y < maxCoordY; y++)
    {
      for (int x = 0; x < maxCoordX; x++)
      {
        Vector2 pos = new Vector2(x * TILE - arenaWidth / 2 + TILE / 2, y * TILE - arenaHeight / 2 + TILE / 2);
        Color color = Color.white;

        if (map[y][x].PlayerIDList.Count > 0)
        {
          color = OccupyColor;
        }

        if (map[y][x].Type != ARENA_OBJECT_TYPE.NONE)
        {
          color = getColorByType(map[y][x].Type);
        }

        circlePos.Add(new GraphicPos
        {
          Pos = new Vector3(pos.x, pos.y),
          Color = color,
        });
      }
    }
  }

  private Color32 getColorByType(ARENA_OBJECT_TYPE type)
  {
    switch (type)
    {
      case ARENA_OBJECT_TYPE.NONE:
        return OpenListColor;
      case ARENA_OBJECT_TYPE.SPIKE:
      case ARENA_OBJECT_TYPE.WALL:
        return WallColor;
      default:
        return OpenListColor;
    }
  }

  private void drawTile(Vector2? pos, Color32? color)
  {
    if (pos == null) return;

    if (color == null)
    {
      color = PathColor;
    }
    circlePos.Add(new GraphicPos
    {
      Pos = new Vector3(pos.Value.x, pos.Value.y),
      Color = color ?? new Color32(),
    });
  }

  private void drawLine()
  {
    if (snake == null) return;

    Vector2 headPos = snake.State.Body[0].Position;

    Vector2 rotTarget = new Vector2(snake.State.MovementDir.x, snake.State.MovementDir.y);
    rotTarget.Normalize();
    rotTarget *= 50;
    Vector2 targetPos = new Vector2(
      headPos.x + rotTarget.x,
      headPos.y + rotTarget.y
    );
    linePos.Add(new LinePos
    {
      Start = new Vector3(headPos.x, headPos.y),
      End = new Vector3(targetPos.x, targetPos.y),
      Color = MoveDirColor,
    });

    Vector2 inputTarget = new Vector2(snake.State.InputDirection.x, snake.State.InputDirection.y);
    inputTarget.Normalize();
    inputTarget *= 50;
    targetPos = new Vector2(
      headPos.x + inputTarget.x,
      headPos.y + inputTarget.y
    );
    linePos.Add(new LinePos
    {
      Start = new Vector3(headPos.x, headPos.y),
      End = new Vector3(targetPos.x, targetPos.y),
      Color = InputColor,
    });

    Vector2 veloTarget = new Vector2(snake.State.Body[0].Velocity.x, snake.State.Body[0].Velocity.y);
    veloTarget.Normalize();
    veloTarget *= 50;
    targetPos = new Vector2(
      headPos.x + veloTarget.x,
      headPos.y + veloTarget.y
    );
    linePos.Add(new LinePos
    {
      Start = new Vector3(headPos.x, headPos.y),
      End = new Vector3(targetPos.x, targetPos.y),
      Color = VeloColor,
    });
  }

  public void SetSnakeToDebug(SnakeConfig? snake)
  {
    this.snake = snake;
  }

  public void SetMapToDebug(List<List<TileMapData>> map)
  {
    this.map = map;
  }

  private void clearDataPath()
  {
    mesh?.Clear();
    circlePos.Clear();
    linePos.Clear();
  }
}
