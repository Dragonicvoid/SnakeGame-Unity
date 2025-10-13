
using System;
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

    public half2 uv;
  }

  struct VertexQuadType
  {
    public Vector3 pos;
    public Vector4 color;
    public half2 uv;
  }

  [SerializeField]
  [Range(0, 10)]
  int blurIntensity = 4;

  [SerializeField]
  [Range(0, 500)]
  float playerRangeToReact = 100f;

  [SerializeField]
  float spikeDistance = 5f;
  [SerializeField]
  Color normalColor = Color.white;
  [SerializeField]
  Color activateColor = Color.red;

  [Range(0f, 1f)]
  float show = 1f;

  List<SnakeConfig> snakes;

  List<ObstacleData> spikes;

  Material? quadMat;

  Material? spikeMat;

  Material? blurMat;

  Mesh? quadMesh;

  Mesh? spikeMesh;

  CommandBuffer? cmdBuffer;

  RenderTexture? quadTex;

  Coroutine? renderCou;

  Coroutine? showAnimCour;

  void Awake()
  {
    snakes = new List<SnakeConfig>();
    spikes = new List<ObstacleData>();
    cmdBuffer = new CommandBuffer();

    if (!quadTex)
    {
      quadTex = new RenderTexture(
        (int)ARENA_DEFAULT_SIZE.WIDTH,
        (int)ARENA_DEFAULT_SIZE.HEIGHT,
        Util.GetGraphicFormat(),
        Util.GetDepthFormat()
        );

      Util.ClearDepthRT(quadTex, cmdBuffer, true);
    }

    setMaterial();
    setQuadMesh();
  }

  public void StartRendering()
  {
    renderCou = StartCoroutine(renderSpike());
  }

  public void SetSpikeData(List<ObstacleData> spikes)
  {
    this.spikes = spikes;
    updateSpikeMesh();
    playSpikeShowAnim();
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
      Shader shader = Shader.Find("Custom/SpikePostFx");
      quadMat = new Material(shader);
    }

    if (!Application.isEditor)
    {
      if (renderer.materials.Length > 0)
      {
        renderer.materials[0] = quadMat;
      }
      else
      {
        renderer.materials.Append(quadMat);
      }
      renderer.material = quadMat;
    }
    else
    {
      renderer.sharedMaterial = quadMat;
      Material tempMaterial = new Material(renderer.sharedMaterial);
      renderer.sharedMaterial = tempMaterial;
      quadMat = tempMaterial;
    }

    quadMat.SetTexture("_MainTex", quadTex);

    if (!spikeMat)
    {
      Shader shader = Shader.Find("Custom/SpikeVfx");
      spikeMat = new Material(shader);
    }
    spikeMat.SetFloat("_MaxDistance", playerRangeToReact);
    spikeMat.SetFloat("_SpikeHeight", spikeDistance);
    spikeMat.SetFloat("_Show", show);

    spikeMat.SetColor("_NormalColor", normalColor);
    spikeMat.SetColor("_ActivateColor", activateColor);
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
    quadMesh.RecalculateBounds();
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

    MeshFilter filter = GetComponent<MeshFilter>();
    if (!filter)
    {
      filter = gameObject.AddComponent<MeshFilter>();
    }
    filter.mesh = quadMesh;
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

    int totalAttribute = 2;
    int vertexPerSpike = 12;
    int vertexCount = vertexPerSpike * spikes.Count;
    NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(totalAttribute, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    attr[1] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

    spikeMesh.SetVertexBufferParams(vertexCount, attr);
    attr.Dispose();
    float currHeight = ARENA_DEFAULT_SIZE.TILE / 2f;
    float currWidth = ARENA_DEFAULT_SIZE.TILE / 2f;
    float currDepth = 0.25f;

    NativeArray<VertexSpikeType> vertex = new NativeArray<VertexSpikeType>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    for (int i = 0; i < spikes.Count; i++)
    {
      int padding = i * vertexPerSpike;
      Vector2 pos = spikes[i].Position;

      half h0 = new half(0f), h1 = new half(1f), h05 = new half(0.5f);

      vertex[padding] = new VertexSpikeType { pos = new Vector3(pos.x - currWidth, pos.y - currHeight, -currDepth), uv = new half2(h0, h0) };
      vertex[padding + 1] = new VertexSpikeType { pos = new Vector3(pos.x + currWidth, pos.y - currHeight, -currDepth), uv = new half2(h1, h0) };
      vertex[padding + 2] = new VertexSpikeType { pos = new Vector3(pos.x + currWidth, pos.y + currHeight, -currDepth), uv = new half2(h1, h1) };
      vertex[padding + 3] = new VertexSpikeType { pos = new Vector3(pos.x - currWidth, pos.y + currHeight, -currDepth), uv = new half2(h0, h1) };
      vertex[padding + 4] = new VertexSpikeType { pos = new Vector3(pos.x - currWidth, pos.y - currHeight, currDepth), uv = new half2(h0, h0) };
      vertex[padding + 5] = new VertexSpikeType { pos = new Vector3(pos.x + currWidth, pos.y - currHeight, currDepth), uv = new half2(h1, h0) };
      vertex[padding + 6] = new VertexSpikeType { pos = new Vector3(pos.x + currWidth, pos.y + currHeight, currDepth), uv = new half2(h1, h1) };
      vertex[padding + 7] = new VertexSpikeType { pos = new Vector3(pos.x - currWidth, pos.y + currHeight, currDepth), uv = new half2(h0, h1) };

      // Back
      Vector3 posToCheck = new Vector3(pos.x, pos.y + currHeight, 0);
      float heightDist = getSpikeHeightDist(posToCheck);
      posToCheck = new Vector3(posToCheck.x, posToCheck.y + heightDist * spikeDistance, posToCheck.z);
      vertex[padding + 8] = new VertexSpikeType { pos = posToCheck, uv = new half2(h05, h05) };

      // Forward
      posToCheck = new Vector3(pos.x, pos.y - currHeight, 0);
      heightDist = getSpikeHeightDist(posToCheck);
      posToCheck = new Vector3(posToCheck.x, posToCheck.y - heightDist * spikeDistance, posToCheck.z);
      vertex[padding + 9] = new VertexSpikeType { pos = posToCheck, uv = new half2(h05, h05) };

      // Left
      posToCheck = new Vector3(pos.x - currWidth, pos.y, 0);
      heightDist = getSpikeHeightDist(posToCheck);
      posToCheck = new Vector3(posToCheck.x - heightDist * spikeDistance, posToCheck.y, posToCheck.z);
      vertex[padding + 10] = new VertexSpikeType { pos = posToCheck, uv = new half2(h05, h05) };

      // Right
      posToCheck = new Vector3(pos.x + currWidth, pos.y, 0);
      heightDist = getSpikeHeightDist(posToCheck);
      posToCheck = new Vector3(posToCheck.x + heightDist * spikeDistance, posToCheck.y, posToCheck.z);
      vertex[padding + 11] = new VertexSpikeType { pos = posToCheck, uv = new half2(h05, h05) };
    }

    spikeMesh.SetVertexBufferData(vertex, 0, 0, vertexCount);
    vertex.Dispose();

    int indexPerSpike = 60;
    int indexCount = indexPerSpike * spikes.Count;
    spikeMesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

    NativeArray<int> indices = new NativeArray<int>(indexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    for (int i = 0; i < spikes.Count; i++)
    {
      int idxPadding = i * indexPerSpike;
      int indicesPadding = i * vertexPerSpike;

      // Front
      indices[idxPadding] = indicesPadding + 1;
      indices[idxPadding + 1] = indicesPadding;
      indices[idxPadding + 2] = indicesPadding + 9;

      indices[idxPadding + 3] = indicesPadding;
      indices[idxPadding + 4] = indicesPadding + 3;
      indices[idxPadding + 5] = indicesPadding + 9;

      indices[idxPadding + 6] = indicesPadding + 3;
      indices[idxPadding + 7] = indicesPadding + 2;
      indices[idxPadding + 8] = indicesPadding + 9;

      indices[idxPadding + 9] = indicesPadding + 2;
      indices[idxPadding + 10] = indicesPadding + 1;
      indices[idxPadding + 11] = indicesPadding + 9;

      // Right
      indices[idxPadding + 12] = indicesPadding + 5;
      indices[idxPadding + 13] = indicesPadding + 1;
      indices[idxPadding + 14] = indicesPadding + 11;

      indices[idxPadding + 15] = indicesPadding + 1;
      indices[idxPadding + 16] = indicesPadding + 2;
      indices[idxPadding + 17] = indicesPadding + 11;

      indices[idxPadding + 18] = indicesPadding + 2;
      indices[idxPadding + 19] = indicesPadding + 6;
      indices[idxPadding + 20] = indicesPadding + 11;

      indices[idxPadding + 21] = indicesPadding + 6;
      indices[idxPadding + 22] = indicesPadding + 5;
      indices[idxPadding + 23] = indicesPadding + 11;


      // Back
      indices[idxPadding + 24] = indicesPadding + 1;
      indices[idxPadding + 25] = indicesPadding + 5;
      indices[idxPadding + 26] = indicesPadding + 8;

      indices[idxPadding + 27] = indicesPadding + 5;
      indices[idxPadding + 28] = indicesPadding + 6;
      indices[idxPadding + 29] = indicesPadding + 8;

      indices[idxPadding + 30] = indicesPadding + 6;
      indices[idxPadding + 31] = indicesPadding + 7;
      indices[idxPadding + 32] = indicesPadding + 8;

      indices[idxPadding + 33] = indicesPadding + 7;
      indices[idxPadding + 34] = indicesPadding + 1;
      indices[idxPadding + 35] = indicesPadding + 8;

      // Left
      indices[idxPadding + 36] = indicesPadding;
      indices[idxPadding + 37] = indicesPadding + 4;
      indices[idxPadding + 38] = indicesPadding + 10;

      indices[idxPadding + 39] = indicesPadding + 1;
      indices[idxPadding + 40] = indicesPadding + 7;
      indices[idxPadding + 41] = indicesPadding + 10;

      indices[idxPadding + 42] = indicesPadding + 7;
      indices[idxPadding + 43] = indicesPadding + 3;
      indices[idxPadding + 44] = indicesPadding + 10;

      indices[idxPadding + 45] = indicesPadding + 3;
      indices[idxPadding + 46] = indicesPadding;
      indices[idxPadding + 47] = indicesPadding + 10;

      // Top
      indices[idxPadding + 48] = indicesPadding + 6;
      indices[idxPadding + 49] = indicesPadding + 2;
      indices[idxPadding + 50] = indicesPadding + 3;
      indices[idxPadding + 51] = indicesPadding + 3;
      indices[idxPadding + 52] = indicesPadding + 7;
      indices[idxPadding + 53] = indicesPadding + 6;

      // Bot
      indices[idxPadding + 54] = indicesPadding + 1;
      indices[idxPadding + 55] = indicesPadding + 5;
      indices[idxPadding + 56] = indicesPadding + 4;
      indices[idxPadding + 57] = indicesPadding + 4;
      indices[idxPadding + 58] = indicesPadding;
      indices[idxPadding + 59] = indicesPadding + 1;

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
      yield return PersistentData.Instance.WaitForFrameEnd;

      if (!spikeMat || cmdBuffer == null || !spikeMesh) continue;

      updateSpikeMesh();
      cmdBuffer.Clear();

      if (snakes != null)
      {
        // Prevent Shader out of bound and activated before playing
        List<Vector4> snakesPos = new List<Vector4>()
        {
          new Vector4(ARENA_DEFAULT_SIZE.WIDTH + 999, ARENA_DEFAULT_SIZE.HEIGHT + 999, 0, 0),
          new Vector4(ARENA_DEFAULT_SIZE.WIDTH + 999, ARENA_DEFAULT_SIZE.HEIGHT + 999, 0, 0)
        };
        for (int i = 0; i < 2 && i < snakes.Count; i++)
        {
          if (snakes[i].State.Body.Count <= 0) break;
          Vector2 pos = snakes[i].State.Body[0].Position;
          snakesPos[i] = new Vector4(pos.x, pos.y, 0, 0);
        }
        spikeMat.SetVectorArray("_PlayerPos", snakesPos);
      }
      spikeMat.SetVector("_CamPos", new Vector3(0, 0, -10));

      Matrix4x4 lookMatrix = Util.CreateViewMatrix(new Vector3(0, 0, -600), Quaternion.identity, Vector3.one).inverse;
      Matrix4x4 orthoMatrix = Matrix4x4.Perspective(60, ARENA_DEFAULT_SIZE.WIDTH / ARENA_DEFAULT_SIZE.HEIGHT, 0.03f, 1000f);
      cmdBuffer.SetViewProjectionMatrices(lookMatrix, orthoMatrix);

      int tempID = Shader.PropertyToID("_temp1");

      cmdBuffer.GetTemporaryRT(tempID, (int)ARENA_DEFAULT_SIZE.WIDTH, (int)ARENA_DEFAULT_SIZE.HEIGHT, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
      cmdBuffer.SetRenderTarget(tempID);
      cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
      cmdBuffer.DrawMesh(spikeMesh, Matrix4x4.identity, spikeMat, 0, 0);
      cmdBuffer.Blit(tempID, quadTex);

      cmdBuffer.ReleaseTemporaryRT(tempID);

      Util.ClearWebViewScreen(cmdBuffer);

      Graphics.ExecuteCommandBuffer(cmdBuffer);
    }
  }

  public void ClearRender()
  {
    if (renderCou != null) StopCoroutine(renderCou);
    if (cmdBuffer == null || !quadTex) return;

    cmdBuffer.Clear();
    cmdBuffer.SetRenderTarget(quadTex);
    cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);

    Util.ClearWebViewScreen(cmdBuffer);

    Graphics.ExecuteCommandBuffer(cmdBuffer);
  }

  void playSpikeShowAnim()
  {
    stopSpikeShowAnim();
    BaseTween<object> tweenData = new BaseTween<object>(
      1.5f,
      null,
      (dist, _) =>
      {
        show = 0;
        spikeMat?.SetFloat("_Show", show);
      },
      (dist, _) =>
      {
        show = dist;
        spikeMat?.SetFloat("_Show", show);
      },
      (dist, _) =>
      {
        show = 1;
        spikeMat?.SetFloat("_Show", show);
        UiEvent.Instance.SpikeAnimationComplete();
      }
    );

    IEnumerator<object> tween = Tween.Create(tweenData);
    showAnimCour = StartCoroutine(tween);
  }

  public RenderTexture GetTexture()
  {
    if (!quadTex)
    {
      quadTex = new RenderTexture(
        (int)ARENA_DEFAULT_SIZE.WIDTH,
        (int)ARENA_DEFAULT_SIZE.HEIGHT,
        Util.GetGraphicFormat(),
        Util.GetDepthFormat()
        );

      Util.ClearDepthRT(quadTex, cmdBuffer ?? new CommandBuffer(), true);
    }
    return quadTex;
  }

  void stopSpikeShowAnim()
  {
    if (showAnimCour == null) return;

    StopCoroutine(showAnimCour);
    showAnimCour = null;
  }

  float getSpikeHeightDist(Vector3 currPos)
  {
    float closest = float.MaxValue;

    for (int i = 0; i < snakes.Count; i++)
    {
      if (snakes[i].State.Body.Count <= 0) break;

      Vector2 pos = snakes[i].State.Body[0].Position;
      float dist = Vector2.Distance(pos, currPos);
      closest = dist < closest ? dist : closest;
    }

    float heightDist = Mathf.Max(0f, 1.0f - (closest / playerRangeToReact));

    return heightDist;
  }
}
