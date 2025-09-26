#nullable enable
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class SpikeVfx : MonoBehaviour
{
  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
  struct VertexType
  {
    public Vector3 pos;
    public Vector3 uv;
  }

  [SerializeField] MeshToSDF? meshToSdf;
  [SerializeField] VolumeRender? volumeRender;

  List<SnakeConfig> snakes;

  List<ObstacleData> spikes;

  SDFTexture? sdfTex;

  Mesh? mesh;

  void Awake()
  {
    snakes = new List<SnakeConfig>();
    spikes = new List<ObstacleData>();

    setTexture();
  }

  public void SetSpikeData(List<ObstacleData> spikes)
  {
    this.spikes = spikes;
    setMesh();
  }

  public void SetSnakes(List<SnakeConfig> snakes)
  {
    this.snakes = snakes;
  }

  void setTexture()
  {
    sdfTex = gameObject.GetComponentInChildren<SDFTexture>();
    if (!sdfTex)
    {
      sdfTex = gameObject.AddComponent<SDFTexture>();
    }

    meshToSdf = gameObject.GetComponent<MeshToSDF>();
    if (!meshToSdf)
    {
      return;
    }

    meshToSdf.sdfTexture = sdfTex;
    volumeRender?.SetTexture(sdfTex.sdf);
  }


  void setMesh()
  {
    MeshRenderer renderer = GetComponent<MeshRenderer>();

    if (!renderer)
    {
      gameObject.AddComponent<MeshRenderer>();
    }

    if (!mesh)
    {
      mesh = new Mesh
      {
        name = gameObject.name
      };
    }
    mesh.Clear();

    int totalAttribute = 2;
    int vertexPerSpike = 8;
    int vertexCount = vertexPerSpike * spikes.Count;
    NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(totalAttribute, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    attr[1] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 3);

    mesh.SetVertexBufferParams(vertexCount, attr);
    attr.Dispose();
    float currHeight = ARENA_DEFAULT_SIZE.TILE / 20f;
    float currWidth = ARENA_DEFAULT_SIZE.TILE / 20f;
    float currDepth = 1f;

    NativeArray<VertexType> vertex = new NativeArray<VertexType>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    for (int i = 0; i < spikes.Count; i++)
    {
      int padding = i * vertexPerSpike;
      Vector2 pos = spikes[i].Position;
      pos /= 10f;
      vertex[padding] = new VertexType { pos = new Vector3(pos.x - currWidth, pos.y - currHeight, -currDepth), uv = new Vector3(0, 0, 0) };
      vertex[padding + 1] = new VertexType { pos = new Vector3(pos.x + currWidth, pos.y - currHeight, -currDepth), uv = new Vector3(1, 0, 0) };
      vertex[padding + 2] = new VertexType { pos = new Vector3(pos.x + currWidth, pos.y + currHeight, -currDepth), uv = new Vector3(1, 1, 0) };
      vertex[padding + 3] = new VertexType { pos = new Vector3(pos.x - currWidth, pos.y + currHeight, -currDepth), uv = new Vector3(0, 1, 0) };
      vertex[padding + 4] = new VertexType { pos = new Vector3(pos.x - currWidth, pos.y - currHeight, currDepth), uv = new Vector3(0, 0, 1) };
      vertex[padding + 5] = new VertexType { pos = new Vector3(pos.x + currWidth, pos.y - currHeight, currDepth), uv = new Vector3(1, 0, 1) };
      vertex[padding + 6] = new VertexType { pos = new Vector3(pos.x + currWidth, pos.y + currHeight, currDepth), uv = new Vector3(1, 1, 1) };
      vertex[padding + 7] = new VertexType { pos = new Vector3(pos.x - currWidth, pos.y + currHeight, currDepth), uv = new Vector3(0, 1, 1) };
    }

    mesh.SetVertexBufferData(vertex, 0, 0, vertexCount);
    vertex.Dispose();

    int indexPerSpike = 36;
    int indexCount = indexPerSpike * spikes.Count;
    mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

    NativeArray<int> indices = new NativeArray<int>(indexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    for (int i = 0; i < spikes.Count; i++)
    {
      int idxPadding = i * indexPerSpike;
      int indicesPadding = i * vertexPerSpike;

      // Front
      indices[idxPadding] = indicesPadding;
      indices[idxPadding + 1] = indicesPadding + 2;
      indices[idxPadding + 2] = indicesPadding + 1;
      indices[idxPadding + 3] = indicesPadding;
      indices[idxPadding + 4] = indicesPadding + 3;
      indices[idxPadding + 5] = indicesPadding + 2;

      // Right
      indices[idxPadding + 6] = indicesPadding + 1;
      indices[idxPadding + 7] = indicesPadding + 6;
      indices[idxPadding + 8] = indicesPadding + 5;
      indices[idxPadding + 9] = indicesPadding + 1;
      indices[idxPadding + 10] = indicesPadding + 2;
      indices[idxPadding + 11] = indicesPadding + 6;

      // Back
      indices[idxPadding + 12] = indicesPadding + 5;
      indices[idxPadding + 13] = indicesPadding + 7;
      indices[idxPadding + 14] = indicesPadding + 4;
      indices[idxPadding + 15] = indicesPadding + 5;
      indices[idxPadding + 16] = indicesPadding + 6;
      indices[idxPadding + 17] = indicesPadding + 7;

      // Left
      indices[idxPadding + 18] = indicesPadding + 4;
      indices[idxPadding + 19] = indicesPadding + 3;
      indices[idxPadding + 20] = indicesPadding + 0;
      indices[idxPadding + 21] = indicesPadding + 4;
      indices[idxPadding + 22] = indicesPadding + 7;
      indices[idxPadding + 23] = indicesPadding + 3;

      // Top
      indices[idxPadding + 24] = indicesPadding + 3;
      indices[idxPadding + 25] = indicesPadding + 6;
      indices[idxPadding + 26] = indicesPadding + 2;
      indices[idxPadding + 27] = indicesPadding + 3;
      indices[idxPadding + 28] = indicesPadding + 7;
      indices[idxPadding + 29] = indicesPadding + 6;

      // Bot
      indices[idxPadding + 30] = indicesPadding + 4;
      indices[idxPadding + 31] = indicesPadding + 1;
      indices[idxPadding + 32] = indicesPadding + 5;
      indices[idxPadding + 33] = indicesPadding + 4;
      indices[idxPadding + 34] = indicesPadding;
      indices[idxPadding + 35] = indicesPadding + 1;

    }
    mesh.SetIndexBufferData(indices, 0, 0, indexCount);
    mesh.RecalculateNormals();
    mesh.RecalculateBounds();

    mesh.subMeshCount = 1;
    mesh.SetSubMesh(0, new SubMeshDescriptor
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
}
