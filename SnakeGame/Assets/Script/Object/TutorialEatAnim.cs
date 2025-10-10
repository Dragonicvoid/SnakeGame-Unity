
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class TutorialEatAnim : MonoBehaviour
{
  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
  struct VertexType
  {
    public Vector3 pos;
    public Color color;
    public half2 uv;
  }

  [SerializeField] TextMeshPro? text;

  [SerializeField] TrailVfx? trailVfx;

  [SerializeField] int highlightSize = 50;

  [SerializeField] int upDownYPos = 5;

  [SerializeField] Texture? texture;

  // Outside of map
  Vector2 initPos = new Vector2(-400, -400);
  Vector2 currPos = new Vector2(-400, -400);

  Mesh? mesh;

  Material? mat;

  RenderTexture? highlightRender;

  FoodConfig? foodToEat;

  Coroutine? currTextCour;

  Coroutine? currHighCour;

  CommandBuffer? cmdBuffer;

  void Awake()
  {
    cmdBuffer = new CommandBuffer();
    highlightRender = new RenderTexture(
      (int)ARENA_DEFAULT_SIZE.WIDTH,
      (int)ARENA_DEFAULT_SIZE.HEIGHT,
      Util.GetGraphicFormat(),
      Util.GetDepthFormat()
    );
    Util.ClearDepthRT(highlightRender, cmdBuffer, true);

    trailVfx?.SetRendTex(highlightRender);

    setMaterial();
    setTexture();
  }

  void OnEnable()
  {
    currPos.Set(initPos.x, initPos.y);
    setMeshData();
    renderMesh();
    StartFadeInText();
  }

  void setMaterial()
  {
    if (!mat)
    {
      Shader shader = Shader.Find("Transparent/CustomSprite");
      mat = new Material(shader);
    }

    mat.SetInt("_Repeat", 1);
  }

  void setTexture()
  {
    if (mat && texture)
      mat.SetTexture("_MainTex", texture);
  }

  void setMeshData()
  {
    if (!mesh)
    {
      mesh = new Mesh
      {
        name = gameObject.name
      };
    }
    mesh.Clear();
    NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    attr[1] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
    attr[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

    mesh.SetVertexBufferParams(4, attr);
    attr.Dispose();
    float currHeight = highlightSize / 2f;
    float currWidth = highlightSize / 2f;

    NativeArray<VertexType> vertex = new NativeArray<VertexType>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

    half h0 = new half(0f), h1 = new half(1f);
    Color color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    vertex[0] = new VertexType { pos = new Vector3(currPos.x - currWidth, currPos.y - currHeight), color = color, uv = new half2(h0, h0) };
    vertex[1] = new VertexType { pos = new Vector3(currPos.x + currWidth, currPos.y - currHeight), color = color, uv = new half2(h1, h0) };
    vertex[2] = new VertexType { pos = new Vector3(currPos.x - currWidth, currPos.y + currHeight), color = color, uv = new half2(h0, h1) };
    vertex[3] = new VertexType { pos = new Vector3(currPos.x + currWidth, currPos.y + currHeight), color = color, uv = new half2(h1, h1) };

    mesh.SetVertexBufferData(vertex, 0, 0, 4);
    vertex.Dispose();

    int indexCount = 6;
    mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt16);
    mesh.SetIndexBufferData(new short[6] { 0, 2, 1, 1, 2, 3 }, 0, 0, indexCount);

    mesh.subMeshCount = 1;
    mesh.bounds = new Bounds
    {
      center = transform.localPosition,
      extents = new Vector3(currWidth, currHeight)
    };
    mesh.SetSubMesh(0, new SubMeshDescriptor
    {
      indexStart = 0,
      indexCount = 6,
      topology = MeshTopology.Triangles,
      baseVertex = 0,
      bounds = new Bounds
      {
        center = transform.localPosition,
        extents = new Vector3(currWidth, currHeight)
      }
    });
  }

  void renderMesh()
  {
    if (cmdBuffer == null || !highlightRender || !mat) return;

    cmdBuffer.Clear();
    var lookMatrix = Util.CreateViewMatrix(new Vector3(0, 0, -10), Quaternion.identity, Vector3.one).inverse;
    var orthoMatrix = Matrix4x4.Ortho(-ARENA_DEFAULT_SIZE.WIDTH / 2, ARENA_DEFAULT_SIZE.WIDTH / 2, -ARENA_DEFAULT_SIZE.HEIGHT / 2, ARENA_DEFAULT_SIZE.HEIGHT / 2, 0.3f, 1000f);
    cmdBuffer.SetViewProjectionMatrices(lookMatrix, orthoMatrix);

    cmdBuffer.SetRenderTarget(highlightRender);
    cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
    cmdBuffer.DrawMesh(mesh, Matrix4x4.identity, mat, 0, 0);

    // Hack resize Web-view
    cmdBuffer.SetRenderTarget(PersistentData.Instance.RenderTex);
    cmdBuffer.ClearRenderTarget(false, false, Color.clear, 1f);

    Graphics.ExecuteCommandBuffer(cmdBuffer);
  }

  public void StopTextAnim()
  {
    if (currTextCour != null)
    {
      StopCoroutine(currTextCour);
    }
  }

  public void StartFadeInText()
  {
    StopTextAnim();
    Color prevTextColor = text?.color ?? Color.white;
    BaseTween<int> tweenData = new BaseTween<int>(
      0.5f,
      0,
      (dist, phase) =>
      {
        if (text) text.color = new Color(prevTextColor.r, prevTextColor.g, prevTextColor.b, 0f);
      },
      (dist, phase) =>
      {
        if (text) text.color = new Color(prevTextColor.r, prevTextColor.g, prevTextColor.b, dist);
      },
      (dist, phase) =>
      {
        if (text) text.color = new Color(prevTextColor.r, prevTextColor.g, prevTextColor.b, 1f);
      }
    );
    IEnumerator<object> tween = Tween.Create(tweenData);
    currTextCour = StartCoroutine(tween);
  }

  public void StopHighlightAnim()
  {
    if (currHighCour != null)
    {
      StopCoroutine(currHighCour);
    }
  }

  void moveUpAndDown(bool goUp = true)
  {
    StopHighlightAnim();
    float posDist = goUp ? 1 : -1;
    Vector2 startPos = new Vector2(currPos.x, currPos.y);
    Vector2 target = new Vector2(startPos.x, posDist * upDownYPos);

    BaseTween<Vector2> tweenData = new BaseTween<Vector2>(
      1.0f,
      startPos,
      (dist, startPos) =>
      {

      },
      (dist, startPos) =>
      {
        Vector2 delta = target - startPos;
        currPos.Set(startPos.x + (delta.x * dist), startPos.y + (delta.y * dist));
        setMeshData();
        renderMesh();
      },
      (dist, startPos) =>
      {
        currPos.Set(target.x, target.y);
        setMeshData();
        renderMesh();

        moveUpAndDown(!goUp);
      }
    );
    IEnumerator<object> tween = Tween.Create(tweenData);
    currHighCour = StartCoroutine(tween);
  }

  public void SetFoodToEat(FoodConfig food)
  {
    foodToEat = food;
    currPos = food.State.Position;
    moveUpAndDown();
  }

  void OnDisable()
  {
    if (text) text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
  }
}
