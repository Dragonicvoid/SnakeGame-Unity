#nullable enable 
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class AiDebugger : MonoBehaviour
{
  [SerializeField]
  Graphic view;
  [SerializeField]
  float updateTime = 1;
  [SerializeField]
  Color pathColor = Color.blue;
  [SerializeField]
  Color openListColor = Color.white;
  [SerializeField]
  Color closeListColor = Color.black;
  [SerializeField]
  Color occupyColor = Color.red;
  [SerializeField]
  Color freeColor = Color.green;
  [SerializeField]
  Label actionLabel;

  SnakeConfig? player;
  List<SnakeConfig> playerList = new List<SnakeConfig>();
  List<List<TileMapData>> map = new List<List<TileMapData>>();

  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
  struct ExampleVertex
  {
    public Vector3 pos;
    public Vector2 uv0;
    public Color32 color;
    public float size;
  }

  void setupMeshRender()
  {
    var mesh = new Mesh();
    // specify vertex count and layout
    var layout = new[]
    {
      new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
      new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
      new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4),
      new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 1),
    };
    var vertexCount = 11;
    mesh.SetVertexBufferParams(vertexCount, layout);

    var verts = new NativeArray<ExampleVertex>(vertexCount, Allocator.Temp);

    verts[0] = new ExampleVertex
    {
      pos = new Vector3(),
      uv0 = new Vector2(),
      color = new Color32(),
      size = 2f,
    };

    mesh.SetVertexBufferData(verts, 0, 0, vertexCount);
  }

  public void SetupScheduler()
  {
    InvokeRepeating("updateDraw", 0f, updateTime);
  }

  private void updateDraw()
  {
    // view Clear;
    drawPath();
    drawMap();
    updateLabel();
  }

  void drawPath()
  {
    if (view == null || player?.State.DebugData == null) return;

    bool shouldDrawPath = Util.ShouldDrawPathfinding();

    if (!shouldDrawPath) return;

    List<Vector2> path = this.player.State.DebugData.EnemyPath ?? new List<Vector2>();
    foreach (Vector2 p in path)
    {
      drawTile(p, null);
    }

    List<AStarPoint> openList =
      player.State.DebugData.PathfindingState?.OpenList ?? new List<AStarPoint>();

    foreach (AStarPoint o in openList)
    {
      Vector2? point = o.Point;
      if (point == null) continue;
      drawTile(point.Value, openListColor);
    }

    List<AStarPoint> closeList =
      player.State.DebugData.PathfindingState?.CloseList ?? new List<AStarPoint>();
    foreach (AStarPoint c in closeList)
    {
      Vector2? point = c.Point;
      if (point == null) return;
      drawTile(point.Value, closeListColor);
    }
  }

  private void drawMap()
  {
    const ctx = view;

    const drawMap = shouldDrawMap();

    if (!this.player || !drawMap) return;

    const { TILE } = ARENA_DEFAULT_OBJECT_SIZE;
    const head = this.player.state.body[0];

    if (!head) return;

    const headCoord = convertPosToCoord(head.position.x, head.position.y);

    const arenaWidth = ARENA_DEFAULT_VALUE.WIDTH;
    const arenaHeight = ARENA_DEFAULT_VALUE.HEIGHT;

    for (let y = 0; y < Math.floor(arenaHeight / TILE); y++)
    {
      for (let x = 0; x < Math.floor(arenaWidth / TILE); x++)
      {
        if (!this.map[y] || !this.map[y][x]) return;

        if (
          this.map[y][x].playerIDList.length > 0 ||
          this.map[y][x].type === ARENA_OBJECT_TYPE.SPIKE
        )
        {
          ctx.strokeColor.set(this.occupyColor);
        }
        else
        {
          ctx.strokeColor.set(this.freeColor);
        }
        ctx.lineWidth = 4;
        ctx.circle(
          this.map[y][x].x + TILE / 2,
          this.map[y][x].y + TILE / 2,
          10,
        );
        ctx.stroke();
        ctx.close();
      }
    }
  }

  private void drawTile(Vector2 pos, Color? color)
  {
    if (color == null)
    {
      color = pathColor;
    }
    const ctx = this.view!;

    ctx.strokeColor.set(color);
    ctx.circle(pos.x, pos.y, 10);
    ctx.stroke();
    ctx.close();
  }

  public void SetPlayerToDebug(SnakeConfig? player)
  {
    this.player = player;
  }

  public void SetPlayerList(List<SnakeConfig> playerList)
  {
    this.playerList = playerList;
  }

  public void SetMapToDebug(List<List<TileMapData>> map)
  {
    this.map = map;
  }

  private void updateLabel()
  {
    const actionData = new Map<string, number>();
    this.playerList.forEach((player) =>
    {
      if (!player.action) return;

      const actionName = player.action?.mapKey;
      const data = actionData.get(player.action.mapKey);
      if (data !== undefined)
      {
        actionData.set(actionName, data + 1);
      }
      else
      {
        actionData.set(actionName, 1);
      }
    });

    let finalString = "";
    actionData.forEach((total, actionName) =>
    {
      finalString += `${ actionName} : ${ total}\n`;
    });

    if (this.actionLabel) this.actionLabel.string = finalString;
  }

  void OnDestroy()
  {
    this.unscheduleAllCallbacks();
  }
}
