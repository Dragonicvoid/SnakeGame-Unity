#nullable enable
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class SnakeTexture : MonoBehaviour
{
  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
  struct VertexType
  {
    public Vector3 pos;
    public Color color;
    public half2 uv;
  }
  [SerializeField]
  float rtSize = 100f;

  SkinDetail? skinPrimary = null;

  SkinDetail? skinSecond = null;

  public RenderTexture PrimaryTex;

  public RenderTexture SecondTex;

  Mesh? primMesh = null;

  Mesh? secondMesh = null;

  Material? primMat = null;

  Material? secondMat = null;

  CommandBuffer? cmdBuff = null;

  void Awake()
  {
    PrimaryTex = new RenderTexture(
      (int)rtSize,
      (int)rtSize,
      UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
      UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat_S8_UInt
    );

    SecondTex = new RenderTexture(
     (int)rtSize,
     (int)rtSize,
     UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
     UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat_S8_UInt
   );

    cmdBuff = new CommandBuffer();

    setupMesh(false);
    setupMesh(true);
  }

  void OnEnable()
  {
    StartCoroutine(render(false));
    StartCoroutine(render(true));
  }

  public void SetSkin(SkinDetail det, bool isPrimary)
  {
    if (isPrimary)
    {
      skinPrimary = det;
    }
    else
    {
      skinSecond = det;
    }

    setupMat(isPrimary);
  }

  void setupMesh(bool isPrimary)
  {
    Mesh? mesh = isPrimary ? primMesh : secondMesh;

    if (!mesh)
    {
      mesh = new Mesh
      {
        name = (isPrimary ? "Primary" : "Secondary") + "_Mesh",
      };
    }

    NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    attr[1] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
    attr[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

    mesh.SetVertexBufferParams(4, attr);
    attr.Dispose();
    float currHeight = rtSize / 2f;
    float currWidth = rtSize / 2f;

    NativeArray<VertexType> vertex = new NativeArray<VertexType>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

    half h0 = new half(0f), h1 = new half(1f);

    vertex[0] = new VertexType { pos = new Vector3(-currWidth, -currHeight), color = Color.white, uv = new half2(h0, h0) };
    vertex[1] = new VertexType { pos = new Vector3(currWidth, -currHeight), color = Color.white, uv = new half2(h1, h0) };
    vertex[2] = new VertexType { pos = new Vector3(-currWidth, currHeight), color = Color.white, uv = new half2(h0, h1) };
    vertex[3] = new VertexType { pos = new Vector3(currWidth, currHeight), color = Color.white, uv = new half2(h1, h1) };

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

    if (isPrimary)
    {
      primMesh = mesh;
    }
    else
    {
      secondMesh = mesh;
    }
  }

  void setupMat(bool isPrimary)
  {
    Material? mat = isPrimary ? primMat : secondMat;
    SkinDetail? skin = isPrimary ? skinPrimary : skinSecond;

    if (skin != null)
    {
      StartCoroutine(getTextureAndSetMat(skin, mat, isPrimary));
    }
    else
    {
      Shader shader = Shader.Find("Transparent/CustomSprite");
      if (mat && Application.isPlaying)
      {
        Destroy(mat);
      }

      if (isPrimary)
      {
        primMat = new Material(shader);
      }
      else
      {
        secondMat = new Material(shader);
      }
    }
  }

  IEnumerator<object> getTextureAndSetMat(SkinDetail skin, Material? mat, bool isPrimary)
  {
    ResourceRequest request = Resources.LoadAsync<Texture2D>(skin?.texture_name ?? "");

    while (!request.isDone)
    {
      yield return null;
    }
    Texture2D? loadedTexture = request.asset as Texture2D;

    if (loadedTexture != null)
    {
      Shader shader = Shader.Find(skin?.shader_name);
      if (mat && Application.isPlaying)
      {
        Destroy(mat);
      }

      if (isPrimary)
      {
        primMat = new Material(shader);
        primMat.SetTexture("_MainTex", loadedTexture);
      }
      else
      {
        secondMat = new Material(shader);
        secondMat.SetTexture("_MainTex", loadedTexture);
      }
    }
    else
    {
      Debug.LogError("Failed to load asset at path: " + skin?.texture_name);
    }
  }

  IEnumerator<object> render(bool isPrimary)
  {
    while (true)
    {
      yield return new WaitForEndOfFrame();
      Material? mat = isPrimary ? primMat : secondMat;
      Mesh? mesh = isPrimary ? primMesh : secondMesh;
      RenderTexture? rendTex = isPrimary ? PrimaryTex : SecondTex;

      if (!mat || !mesh || cmdBuff == null || !rendTex) continue;

      cmdBuff.Clear();
      var lookMatrix = Camera.main.worldToCameraMatrix;
      var orthoMatrix = Matrix4x4.Ortho(-rtSize / 2, rtSize / 2, -rtSize / 2, rtSize / 2, 0.3f, 1000f);
      cmdBuff.SetViewProjectionMatrices(lookMatrix, orthoMatrix);

      cmdBuff.SetRenderTarget(rendTex);
      cmdBuff.ClearRenderTarget(true, true, Color.clear, 1f);
      cmdBuff.DrawMesh(mesh, Matrix4x4.identity, mat, 0, 0);

      // Hack resize Web-view
      cmdBuff.SetRenderTarget(PersistentData.Instance.RenderTex);
      cmdBuff.ClearRenderTarget(false, false, Color.clear, 1f);

      Graphics.ExecuteCommandBuffer(cmdBuff);
    }
  }
}
