#nullable enable
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class TrailVfx : MonoBehaviour
{
  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
  struct VertexType
  {
    public Vector3 pos;
    public half2 uv;
  }

  [SerializeField]
  Color trailColor = Color.white;
  [SerializeField]
  int rtSize = (int)Mathf.Max(ARENA_DEFAULT_SIZE.WIDTH, ARENA_DEFAULT_SIZE.HEIGHT);
  [SerializeField]
  float alphaReduce = 0.01f;
  [SerializeField]
  float trailReduceInterval = 0.2f;

  bool hasFirstDrawn = false;

  RenderTexture? rendTex;

  RenderTexture? prevTex;

  RenderTexture? quadTex;

  Mesh? mesh;

  MeshRenderer? meshRend;

  [SerializeField]
  Material? trailMat;

  [SerializeField]
  Material? alphaMat;

  Material? quadMat;

  CommandBuffer? cmdBuffer;

  Coroutine? renderCoroutine;

  void Awake()
  {
    cmdBuffer = new CommandBuffer();
  }

  void OnValidate()
  {
    setMaterial();
  }

  void setTexture()
  {
    if (!prevTex)
    {
      prevTex = new RenderTexture(
        (int)ARENA_DEFAULT_SIZE.WIDTH,
        (int)ARENA_DEFAULT_SIZE.HEIGHT,
        Util.GetGraphicFormat(),
        Util.GetDepthFormat()
      );
    }
    Util.ClearDepthRT(prevTex, new CommandBuffer(), true);

    if (!quadTex)
    {
      quadTex = new RenderTexture(
        (int)ARENA_DEFAULT_SIZE.WIDTH,
        (int)ARENA_DEFAULT_SIZE.HEIGHT,
        Util.GetGraphicFormat(),
        Util.GetDepthFormat()
      );
    }
    Util.ClearDepthRT(quadTex, new CommandBuffer(), true);

    renderCoroutine = StartCoroutine(render());
  }

  void setMaterial()
  {
    meshRend = GetComponent<MeshRenderer>();
    if (!meshRend)
    {
      meshRend = gameObject.AddComponent<MeshRenderer>();
    }

    if (!alphaMat)
    {
      Shader shader = Shader.Find("Transparent/AlphaReducer");
      alphaMat = new Material(shader);
    }
    alphaMat.SetFloat("_Reduce", alphaReduce);

    if (!trailMat)
    {
      Shader shader = Shader.Find("Transparent/TrailVfx");
      trailMat = new Material(shader);
    }

    if (!quadMat)
    {
      Shader shader = Shader.Find("Transparent/CustomSprite");
      quadMat = new Material(shader);
    }

    if (Application.isPlaying)
    {
      if (meshRend.materials.Length > 0)
      {
        meshRend.materials[0] = quadMat;
      }
      else
      {
        meshRend.materials.Append(quadMat);
      }
      meshRend.material = quadMat;
    }

    trailMat.SetTexture("_MainTex", rendTex);
    trailMat.SetTexture("_PrevTex", prevTex);
    trailMat.SetColor("_TrailCol", trailColor);

    quadMat.SetTexture("_MainTex", quadTex);
  }

  void setMesh()
  {
    if (!mesh)
    {
      mesh = new Mesh
      {
        name = gameObject.name
      };
    }
    NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    attr[1] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

    mesh.SetVertexBufferParams(4, attr);
    attr.Dispose();
    float currHeight = rtSize / 2f;
    float currWidth = rtSize / 2f;

    NativeArray<VertexType> vertex = new NativeArray<VertexType>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

    half h0 = new half(0f), h1 = new half(1f);

    vertex[0] = new VertexType { pos = new Vector3(-currWidth, -currHeight), uv = new half2(h0, h0) };
    vertex[1] = new VertexType { pos = new Vector3(currWidth, -currHeight), uv = new half2(h1, h0) };
    vertex[2] = new VertexType { pos = new Vector3(-currWidth, currHeight), uv = new half2(h0, h1) };
    vertex[3] = new VertexType { pos = new Vector3(currWidth, currHeight), uv = new half2(h1, h1) };

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

    if (Application.isPlaying)
    {
      MeshFilter filter = GetComponent<MeshFilter>();
      if (!filter)
      {
        filter = gameObject.AddComponent<MeshFilter>();
      }
      filter.mesh = mesh;
    }
  }

  IEnumerator<object> render()
  {
    while (true)
    {
      yield return PersistentData.Instance.GetWaitSecond(trailReduceInterval);
      if (cmdBuffer != null && prevTex && alphaMat)
      {
        cmdBuffer.Clear();
        var lookMatrix = Util.CreateViewMatrix(new Vector3(0, 0, -10), Quaternion.identity, Vector3.one).inverse;
        var orthoMatrix = Matrix4x4.Ortho(-rtSize / 2, rtSize / 2, -rtSize / 2, rtSize / 2, 0.3f, 1000f);
        cmdBuffer.SetViewProjectionMatrices(lookMatrix, orthoMatrix);

        int temp1 = Shader.PropertyToID("_Temp1");
        int temp2 = Shader.PropertyToID("_Temp2");

        cmdBuffer.GetTemporaryRT(temp1, (int)ARENA_DEFAULT_SIZE.WIDTH, (int)ARENA_DEFAULT_SIZE.HEIGHT, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        cmdBuffer.GetTemporaryRT(temp2, (int)ARENA_DEFAULT_SIZE.WIDTH, (int)ARENA_DEFAULT_SIZE.HEIGHT, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

        if (!hasFirstDrawn)
        {
          cmdBuffer.SetRenderTarget(temp2);
          cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
          cmdBuffer.SetRenderTarget(temp1);
          cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
          hasFirstDrawn = true;
        }
        else
        {
          // cmdBuffer.SetRenderTarget(prevTex);
          // cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
          // cmdBuffer.SetRenderTarget(quadTex);
          // cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);

          // cmdBuffer.Blit(snakeTex, temp1, trailMat, 0, 0);
          // cmdBuffer.Blit(temp1, temp2, alphaMat, 0, 0);
          // cmdBuffer.Blit(temp2, prevTex);
          // cmdBuffer.Blit(snakeTex, quadTex, trailMat, 0, 0);

          cmdBuffer.SetRenderTarget(temp1);
          cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
          cmdBuffer.SetRenderTarget(temp2);
          cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
          cmdBuffer.SetRenderTarget(quadTex);
          cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);

          cmdBuffer.Blit(rendTex, temp1, trailMat, 0, 0);
          cmdBuffer.Blit(temp1, temp2, alphaMat, 0, 0);
          cmdBuffer.Blit(temp2, prevTex, quadMat, 0, 0);

          cmdBuffer.Blit(rendTex, temp1, trailMat, 0, 0);
          cmdBuffer.Blit(temp1, quadTex, quadMat, 0, 0);
        }

        cmdBuffer.ReleaseTemporaryRT(temp1);
        cmdBuffer.ReleaseTemporaryRT(temp2);

        // Hack resize Web-view
        cmdBuffer.SetRenderTarget(PersistentData.Instance.RenderTex);
        cmdBuffer.ClearRenderTarget(false, false, Color.clear, 1f);

        Graphics.ExecuteCommandBuffer(cmdBuffer);

      }
    }
  }

  public void ClearRender()
  {
    if (cmdBuffer == null) return;
    cmdBuffer.Clear();

    cmdBuffer.SetRenderTarget(quadTex);
    cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);

    cmdBuffer.SetRenderTarget(prevTex);
    cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);

    // Hack resize Web-view
    cmdBuffer.SetRenderTarget(PersistentData.Instance.RenderTex);
    cmdBuffer.ClearRenderTarget(false, false, Color.clear, 1f);
    Graphics.ExecuteCommandBuffer(cmdBuffer);
  }


  public void SetRendTex(RenderTexture rendTex)
  {
    this.rendTex = rendTex;
    hasFirstDrawn = false;
    setTexture();
    setMaterial();
    setMesh();
  }

  void OnEnable()
  {
    renderCoroutine = StartCoroutine(render());
  }
}
