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
  public Color Color;
  public half2 UV;
}

struct GraphicPos
{
  public Vector3 Pos;
  public Color Color;
}
public class AiRenderer : MonoBehaviour
{
  [SerializeField]
  float updateTime = 1;
  [SerializeField]
  Color pathColor = Color.blue;
  [SerializeField]
  Color openListColor = Color.white;
  [SerializeField]
  Color closeListColor = Color.red;
  [SerializeField]
  Color wallColor = Color.green;

  List<GraphicPos> graphicPos = new List<GraphicPos>();

  SnakeConfig? snake;

  List<List<TileMapData>> map = new List<List<TileMapData>>();

  Mesh? mesh;

  void Awake()
  {
    SetupScheduler();
  }

  void updateMeshRender()
  {
    MeshRenderer renderer = GetComponent<MeshRenderer>();

    if (!renderer)
    {
      renderer = gameObject.AddComponent<MeshRenderer>();
      Shader shader = Shader.Find("Debug/AiRenderer");
      renderer.sharedMaterial = new Material(shader);
    }

    if (!mesh)
    {
      mesh = new Mesh();
    }
    else
    {
      mesh.Clear();
    }

    int attrbTotal = 3;
    NativeArray<VertexAttributeDescriptor> layout = new NativeArray<VertexAttributeDescriptor>(attrbTotal, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    layout[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    layout[1] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
    layout[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

    int vertexPerPos = 4;
    int vertexCount = graphicPos.Count * vertexPerPos;
    mesh.SetVertexBufferParams(vertexCount, layout);
    layout.Dispose();

    NativeArray<AiRendererVertex> verts = new NativeArray<AiRendererVertex>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

    half h0 = new half(0f); half h1 = new half(1f);

    for (int i = 0; i < graphicPos.Count; i++)
    {
      int padding = i * vertexPerPos;
      float TILE = ARENA_DEFAULT_SIZE.TILE;
      float halfWidth = TILE / 2;
      float halfHeight = TILE / 2;

      GraphicPos graphic = graphicPos[i];

      verts[padding] = new AiRendererVertex
      {
        Pos = new Vector3(graphic.Pos.x - halfWidth, graphic.Pos.y - halfHeight),
        Color = graphic.Color,
        UV = new half2(h0, h0),
      };

      verts[padding + 1] = new AiRendererVertex
      {
        Pos = new Vector3(graphic.Pos.x - halfWidth, graphic.Pos.y + halfHeight),
        Color = graphic.Color,
        UV = new half2(h0, h1),
      };

      verts[padding + 2] = new AiRendererVertex
      {
        Pos = new Vector3(graphic.Pos.x + halfWidth, graphic.Pos.y + halfHeight),
        Color = graphic.Color,
        UV = new half2(h1, h1),
      };

      verts[padding + 3] = new AiRendererVertex
      {
        Pos = new Vector3(graphic.Pos.x + halfWidth, graphic.Pos.y - halfHeight),
        Color = graphic.Color,
        UV = new half2(h1, h0),
      };
    }

    mesh.SetVertexBufferData(verts, 0, 0, vertexCount);
    verts.Dispose();

    int indexPerPos = 6;
    int indexCount = graphicPos.Count * indexPerPos;
    mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

    NativeArray<int> indices = new NativeArray<int>(indexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

    for (int i = 0; i < graphicPos.Count; i++)
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

  public void SetupScheduler()
  {
    InvokeRepeating("updateDraw", 0f, updateTime);
  }

  private void updateDraw()
  {
    clearDataPath();
    drawPath();
    // drawMap();
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

    List<AStarPoint> openList = snake.State.DebugData?.PathfindingState?.OpenList ?? new List<AStarPoint>();
    foreach (AStarPoint o in openList)
    {
      drawTile(o.Point, openListColor);
    }

    List<AStarPoint> closeList = snake.State.DebugData?.PathfindingState?.CloseList ?? new List<AStarPoint>();
    foreach (AStarPoint c in closeList)
    {
      drawTile(c.Point, closeListColor);
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
        graphicPos.Add(new GraphicPos
        {
          Pos = new Vector3(pos.x, pos.y),
          Color = getColorByType(map[y][x].Type),
        });
      }
    }
  }

  private Color32 getColorByType(ARENA_OBJECT_TYPE type)
  {
    switch (type)
    {
      case ARENA_OBJECT_TYPE.NONE:
        return openListColor;
      case ARENA_OBJECT_TYPE.WALL:
        return wallColor;
      default:
        return openListColor;
    }
  }

  private void drawTile(Vector2? pos, Color32? color)
  {
    if (pos == null) return;

    if (color == null)
    {
      color = pathColor;
    }
    graphicPos.Add(new GraphicPos
    {
      Pos = new Vector3(pos.Value.x, pos.Value.y),
      Color = color ?? new Color32(),
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
    graphicPos.Clear();
  }
}
