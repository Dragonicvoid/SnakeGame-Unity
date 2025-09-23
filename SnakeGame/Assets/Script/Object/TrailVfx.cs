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

  bool hasFirstDrawn = false;

  [SerializeField]
  RenderTexture? temp;

  RenderTexture? snakeTex;

  RenderTexture? prevTex;

  Mesh? mesh;

  MeshRenderer? meshRend;

  Material? mat;

  Material? alphaMat;

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
        UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
        UnityEngine.Experimental.Rendering.GraphicsFormat.S8_UInt
      );
    }

    temp = new RenderTexture(
        (int)ARENA_DEFAULT_SIZE.WIDTH,
        (int)ARENA_DEFAULT_SIZE.HEIGHT,
        UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
        UnityEngine.Experimental.Rendering.GraphicsFormat.S8_UInt
      );

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
    alphaMat.SetFloat("_Reducer", alphaReduce);

    if (!mat)
    {
      Shader shader = Shader.Find("Transparent/TrailVfx");
      mat = new Material(shader);
    }

    if (Application.isPlaying)
    {
      if (meshRend.materials.Length > 0)
      {
        meshRend.materials[0] = mat;
      }
      else
      {
        meshRend.materials.Append(mat);
      }
      meshRend.material = mat;
    }

    mat.SetTexture("_MainTex", snakeTex);
    mat.SetTexture("_PrevTex", prevTex);
    mat.SetColor("_TrailCol", trailColor);
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
      float deltaTime = Time.deltaTime;
      yield return new WaitForEndOfFrame();
      if (cmdBuffer != null && prevTex && alphaMat)
      {
        cmdBuffer.Clear();
        alphaMat.SetFloat("_Reducer", deltaTime);
        var lookMatrix = Camera.main.worldToCameraMatrix;
        var orthoMatrix = Matrix4x4.Ortho(-rtSize / 2, rtSize / 2, -rtSize / 2, rtSize / 2, 0.3f, 1000f);
        cmdBuffer.SetViewProjectionMatrices(lookMatrix, orthoMatrix);

        cmdBuffer.SetRenderTarget(temp);
        cmdBuffer.ClearRenderTarget(true, false, Color.clear, 1f);

        if (!hasFirstDrawn)
        {
          cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
          hasFirstDrawn = true;
        }
        else
        {
          cmdBuffer.Blit(snakeTex, prevTex, mat, 0, 0);
          cmdBuffer.Blit(prevTex, temp, alphaMat, 0, 0);
          cmdBuffer.SetRenderTarget(prevTex);
          cmdBuffer.ClearRenderTarget(true, false, Color.clear, 1f);
          cmdBuffer.Blit(temp, prevTex, alphaMat, 0, 0);
        }

        // Hack resize Web-view
        cmdBuffer.SetRenderTarget(PersistentData.Instance.RenderTex);
        cmdBuffer.ClearRenderTarget(false, false, Color.clear, 1f);

        Graphics.ExecuteCommandBuffer(cmdBuffer);
      }
    }
  }

  public void SetSnakeTex(RenderTexture snakeTex)
  {
    this.snakeTex = snakeTex;
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
