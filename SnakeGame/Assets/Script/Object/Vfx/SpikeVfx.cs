#nullable enable
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class SpikeVfx : MonoBehaviour
{
  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
  struct VertexSpikeType
  {
    public Vector3 pos;
    public Vector4 color;

    public Vector3 uv;
  }

  struct VertexQuadType
  {
    public Vector3 pos;
    public Vector4 color;
    public half2 uv;
  }

  List<SnakeConfig> snakes;

  List<ObstacleData> spikes;

  Material? quadMat;

  Material? spikeMat;

  Mesh? quadMesh;

  Mesh? spikeMesh;

  CommandBuffer? cmdBuffer;

  RenderTexture? quadTex;

  void Awake()
  {
    snakes = new List<SnakeConfig>();
    spikes = new List<ObstacleData>();

    quadTex = new RenderTexture(
      (int)ARENA_DEFAULT_SIZE.WIDTH,
      (int)ARENA_DEFAULT_SIZE.HEIGHT,
      UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
      UnityEngine.Experimental.Rendering.GraphicsFormat.S8_UInt
    );

    cmdBuffer = new CommandBuffer();

    setMaterial();
    setQuadMesh();
  }

  void OnEnable()
  {
    StartCoroutine(renderSpike());
  }

  public void SetSpikeData(List<ObstacleData> spikes)
  {
    this.spikes = spikes;
    updateSpikeMesh();
  }

  public void SetSnakes(List<SnakeConfig> snakes)
  {
    this.snakes = snakes;
  }

  void setMaterial()
  {
    MeshRenderer renderer = GetComponent<MeshRenderer>();

    if (!renderer)
    {
      renderer = gameObject.AddComponent<MeshRenderer>();
    }

    if (!quadMat)
    {
      Shader shader = Shader.Find("Transparent/CustomSprite");
      quadMat = new Material(shader);
    }

    if (renderer.materials.Length > 0)
    {
      renderer.materials[0] = quadMat;
    }
    else
    {
      renderer.materials.Append(quadMat);
    }
    renderer.material = quadMat;
    quadMat.SetTexture("_MainTex", quadTex);

    if (!spikeMat)
    {
      Shader shader = Shader.Find("Custom/SpikeVfx");
      spikeMat = new Material(shader);
    }
    spikeMat.SetFloat("_MaxDistance", 100f);
    spikeMat.SetFloat("_SpikeHeight", 5f);
  }

  void setQuadMesh()
  {
    if (!quadMesh)
    {
      quadMesh = new Mesh
      {
        name = gameObject.name
      };
    }
    NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    attr[1] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
    attr[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

    quadMesh.SetVertexBufferParams(4, attr);
    attr.Dispose();
    float currHeight = ARENA_DEFAULT_SIZE.HEIGHT / 2f;
    float currWidth = ARENA_DEFAULT_SIZE.WIDTH / 2f;

    NativeArray<VertexQuadType> vertex = new NativeArray<VertexQuadType>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

    half h0 = new half(0f), h1 = new half(1f);
    vertex[0] = new VertexQuadType { pos = new Vector3(-currWidth, -currHeight), color = Color.white, uv = new half2(h0, h0) };
    vertex[1] = new VertexQuadType { pos = new Vector3(currWidth, -currHeight), color = Color.white, uv = new half2(h1, h0) };
    vertex[2] = new VertexQuadType { pos = new Vector3(-currWidth, currHeight), color = Color.white, uv = new half2(h0, h1) };
    vertex[3] = new VertexQuadType { pos = new Vector3(currWidth, currHeight), color = Color.white, uv = new half2(h1, h1) };

    quadMesh.SetVertexBufferData(vertex, 0, 0, 4);
    vertex.Dispose();

    int indexCount = 6;
    quadMesh.SetIndexBufferParams(indexCount, IndexFormat.UInt16);
    quadMesh.SetIndexBufferData(new short[6] { 0, 2, 1, 1, 2, 3 }, 0, 0, indexCount);

    quadMesh.subMeshCount = 1;
    quadMesh.bounds = new Bounds
    {
      center = transform.localPosition,
      extents = new Vector3(currWidth, currHeight)
    };
    quadMesh.SetSubMesh(0, new SubMeshDescriptor
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
      filter.mesh = quadMesh;
    }
  }

  void updateSpikeMesh()
  {
    if (!spikeMesh)
    {
      spikeMesh = new Mesh
      {
        name = gameObject.name
      };
    }
    spikeMesh.Clear();

    int totalAttribute = 3;
    int vertexPerSpike = 8;
    int vertexCount = vertexPerSpike * spikes.Count;
    NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(totalAttribute, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    attr[1] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
    attr[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 3);

    spikeMesh.SetVertexBufferParams(vertexCount, attr);
    attr.Dispose();
    float currHeight = ARENA_DEFAULT_SIZE.TILE / 2f;
    float currWidth = ARENA_DEFAULT_SIZE.TILE / 2f;
    float currDepth = 1f;

    NativeArray<VertexSpikeType> vertex = new NativeArray<VertexSpikeType>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    for (int i = 0; i < spikes.Count; i++)
    {
      int padding = i * vertexPerSpike;
      Vector2 pos = spikes[i].Position;
      vertex[padding] = new VertexSpikeType { pos = new Vector3(pos.x - currWidth, pos.y - currHeight, -currDepth), color = Color.black, uv = new Vector3(0, 0, 0) };
      vertex[padding + 1] = new VertexSpikeType { pos = new Vector3(pos.x + currWidth, pos.y - currHeight, -currDepth), color = Color.black, uv = new Vector3(1, 0, 0) };
      vertex[padding + 2] = new VertexSpikeType { pos = new Vector3(pos.x + currWidth, pos.y + currHeight, -currDepth), color = Color.black, uv = new Vector3(1, 1, 0) };
      vertex[padding + 3] = new VertexSpikeType { pos = new Vector3(pos.x - currWidth, pos.y + currHeight, -currDepth), color = Color.black, uv = new Vector3(0, 1, 0) };
      vertex[padding + 4] = new VertexSpikeType { pos = new Vector3(pos.x - currWidth, pos.y - currHeight, currDepth), color = Color.black, uv = new Vector3(0, 0, 1) };
      vertex[padding + 5] = new VertexSpikeType { pos = new Vector3(pos.x + currWidth, pos.y - currHeight, currDepth), color = Color.black, uv = new Vector3(1, 0, 1) };
      vertex[padding + 6] = new VertexSpikeType { pos = new Vector3(pos.x + currWidth, pos.y + currHeight, currDepth), color = Color.black, uv = new Vector3(1, 1, 1) };
      vertex[padding + 7] = new VertexSpikeType { pos = new Vector3(pos.x - currWidth, pos.y + currHeight, currDepth), color = Color.black, uv = new Vector3(0, 1, 1) };
    }

    spikeMesh.SetVertexBufferData(vertex, 0, 0, vertexCount);
    vertex.Dispose();

    int indexPerSpike = 36;
    int indexCount = indexPerSpike * spikes.Count;
    spikeMesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

    NativeArray<int> indices = new NativeArray<int>(indexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    for (int i = 0; i < spikes.Count; i++)
    {
      int idxPadding = i * indexPerSpike;
      int indicesPadding = i * vertexPerSpike;

      // Front
      indices[idxPadding] = indicesPadding + 2;
      indices[idxPadding + 1] = indicesPadding + 1;
      indices[idxPadding + 2] = indicesPadding;
      indices[idxPadding + 3] = indicesPadding;
      indices[idxPadding + 4] = indicesPadding + 3;
      indices[idxPadding + 5] = indicesPadding + 2;

      // Right
      indices[idxPadding + 6] = indicesPadding + 6;
      indices[idxPadding + 7] = indicesPadding + 5;
      indices[idxPadding + 8] = indicesPadding + 1;
      indices[idxPadding + 9] = indicesPadding + 1;
      indices[idxPadding + 10] = indicesPadding + 2;
      indices[idxPadding + 11] = indicesPadding + 6;

      // Back
      indices[idxPadding + 12] = indicesPadding + 7;
      indices[idxPadding + 13] = indicesPadding + 4;
      indices[idxPadding + 14] = indicesPadding + 5;
      indices[idxPadding + 15] = indicesPadding + 5;
      indices[idxPadding + 16] = indicesPadding + 6;
      indices[idxPadding + 17] = indicesPadding + 7;

      // Left
      indices[idxPadding + 18] = indicesPadding + 3;
      indices[idxPadding + 19] = indicesPadding + 0;
      indices[idxPadding + 20] = indicesPadding + 4;
      indices[idxPadding + 21] = indicesPadding + 4;
      indices[idxPadding + 22] = indicesPadding + 7;
      indices[idxPadding + 23] = indicesPadding + 3;

      // Top
      indices[idxPadding + 24] = indicesPadding + 6;
      indices[idxPadding + 25] = indicesPadding + 2;
      indices[idxPadding + 26] = indicesPadding + 3;
      indices[idxPadding + 27] = indicesPadding + 3;
      indices[idxPadding + 28] = indicesPadding + 7;
      indices[idxPadding + 29] = indicesPadding + 6;

      // Bot
      indices[idxPadding + 30] = indicesPadding + 1;
      indices[idxPadding + 31] = indicesPadding + 5;
      indices[idxPadding + 32] = indicesPadding + 4;
      indices[idxPadding + 33] = indicesPadding + 4;
      indices[idxPadding + 34] = indicesPadding;
      indices[idxPadding + 35] = indicesPadding + 1;

    }
    spikeMesh.SetIndexBufferData(indices, 0, 0, indexCount);
    spikeMesh.RecalculateNormals();
    spikeMesh.RecalculateBounds();

    spikeMesh.subMeshCount = 1;
    spikeMesh.SetSubMesh(0, new SubMeshDescriptor
    {
      indexStart = 0,
      indexCount = indexCount,
      topology = MeshTopology.Triangles,
      baseVertex = 0,
      bounds = new Bounds
      {
        center = transform.localPosition,
        extents = new Vector3(currWidth, currHeight, currDepth)
      }
    });
  }

  IEnumerator<object> renderSpike()
  {
    while (true)
    {
      yield return new WaitForEndOfFrame();

      if (!spikeMat || cmdBuffer == null || !spikeMesh) continue;

      cmdBuffer.Clear();

      if (snakes != null)
      {
        List<Vector4> snakesPos = new List<Vector4>()
        {
          new Vector4(ARENA_DEFAULT_SIZE.WIDTH / 2, ARENA_DEFAULT_SIZE.HEIGHT / 2, 0, 0),
          new Vector4(ARENA_DEFAULT_SIZE.WIDTH / 2, ARENA_DEFAULT_SIZE.HEIGHT / 2, 0, 0)
        };
        for (int i = 0; i < 2 && i < snakes.Count; i++)
        {
          if (snakes[i].State.Body.Count <= 0) break;
          Vector2 pos = snakes[i].State.Body[0].Position;
          snakesPos[i] = new Vector4(pos.x, pos.y, 0, 0);
        }
        cmdBuffer.SetGlobalVectorArray("_PlayerPos", snakesPos);
      }
      cmdBuffer.SetGlobalVector("_CamPos", new Vector3(0, 0, -10));

      Matrix4x4 lookMatrix = Util.CreateViewMatrix(new Vector3(0, 0, -600), Quaternion.identity, Vector3.one).inverse;
      Matrix4x4 orthoMatrix = Matrix4x4.Perspective(60, ARENA_DEFAULT_SIZE.WIDTH / ARENA_DEFAULT_SIZE.HEIGHT, 0.03f, 1000f);
      cmdBuffer.SetViewProjectionMatrices(lookMatrix, orthoMatrix);

      int depthID = Shader.PropertyToID("_ParticleDepthTexture");

      cmdBuffer.GetTemporaryRT(depthID, (int)ARENA_DEFAULT_SIZE.WIDTH, (int)ARENA_DEFAULT_SIZE.HEIGHT, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
      cmdBuffer.SetRenderTarget(depthID);
      cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
      cmdBuffer.DrawMesh(spikeMesh, Matrix4x4.identity, spikeMat, 0, 0);
      cmdBuffer.SetGlobalTexture("_ParticleDepthTexture", depthID);

      cmdBuffer.Blit(depthID, quadTex);

      cmdBuffer.ReleaseTemporaryRT(depthID);

      Graphics.ExecuteCommandBuffer(cmdBuffer);
    }
  }
}
