
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class Background : MonoBehaviour
{
  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
  struct VertexType
  {
    public Vector3 pos;
    public Color color;
    public half2 uv;
  }

  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
  struct VertexTypeBlock
  {
    public Vector3 pos;
    public Vector3 uv;
  }

  struct BlockData
  {
    public Vector3 pos;
    public float speed;
    public float length;
  }

  [SerializeField] public Vector2 SpeedBound = new Vector2(200f, 350f);
  [SerializeField] public Vector2 LengthBound = new Vector2(50f, 400f);
  [SerializeField] public float BlockThickness = 2f;
  [SerializeField] public float QuadWidth = 700f;
  [SerializeField] public float QuadHeight = 700f;
  [SerializeField] Color color = Color.white;
  [SerializeField] Color fillColor = Color.white;
  [SerializeField] Color blockColor = Color.black;
  [SerializeField] int totalBlock = 7;
  [SerializeField] Camera camForMatrix;
  [Range(0, 10)][SerializeField] int blurIteration = 0;

  float currDist = 0f;

  Vector2 posBound = new Vector2();

  List<BlockData> blocks;

  Material? quadMat;
  Material? blockMat;
  Material? blurMat;

  Mesh? quadMeshes;
  Mesh? blockMesh;

  RenderTexture? blockRendTex;

  CommandBuffer? cmdBuffer;

  Coroutine? shakeCour;
  Coroutine? distChangedCour;

  void Awake()
  {
    if (Application.isPlaying)
    {
      cmdBuffer = new CommandBuffer();
      posBound.Set(-(QuadWidth / 2f) - (LengthBound.y / 2f), (QuadWidth / 2f) + (LengthBound.y / 2f));
      blocks = new List<BlockData>();
      generateRandomBlock();
      setQuadMeshData();

      blockRendTex = new RenderTexture(
        (int)QuadWidth,
        (int)QuadHeight,
        Util.GetGraphicFormat(),
        Util.GetDepthFormat()
      );
      Util.ClearDepthRT(blockRendTex, cmdBuffer, true);

      setMaterial();
      setTexture();

      GameEvent.Instance.onMainPlayerEat -= onMainPlayerEat;
      GameEvent.Instance.onMainPlayerEat += onMainPlayerEat;

      GameEvent.Instance.onGameOver -= onGameOver;
      GameEvent.Instance.onGameOver += onGameOver;

      GameEvent.Instance.onMainPlayerFire -= onMainPlayerFire;
      GameEvent.Instance.onMainPlayerFire += onMainPlayerFire;
    }
  }

  void OnEnable()
  {
    StartCoroutine(updateBlockPos());
    StartCoroutine(renderBlock());
  }

  void generateRandomBlock()
  {
    for (int i = 0; i < totalBlock; i++)
    {
      blocks.Add(new BlockData
      {
        pos = new Vector3(UnityEngine.Random.Range(posBound.x, posBound.y), QuadHeight / totalBlock * i - QuadHeight / 2f, UnityEngine.Random.Range(-100, 100)),
        speed = UnityEngine.Random.Range(SpeedBound.x, SpeedBound.y),
        length = UnityEngine.Random.Range(LengthBound.x, LengthBound.y),
      });
    }
  }

  void setMaterial()
  {
    MeshRenderer? meshRender = GetComponent<MeshRenderer>();
    if (!meshRender)
    {
      meshRender = gameObject.AddComponent<MeshRenderer>();
    }

    if (!quadMat)
    {
      Shader shader = Shader.Find("Transparent/Background");
      quadMat = new Material(shader);
    }

    if (!blurMat)
    {
      Shader shader = Shader.Find("Transparent/Blur");
      blurMat = new Material(shader);
    }

    if (!blockMat)
    {
      Shader shader = Shader.Find("Transparent/BackgroundBlock");
      blockMat = new Material(shader);
    }


    if (!Application.isEditor)
    {
      if (meshRender.materials.Length > 0)
      {
        meshRender.materials[0] = quadMat;
      }
      else
      {
        meshRender.materials.Append(quadMat);
      }
      meshRender.material = quadMat;
    }
    else
    {
      meshRender.sharedMaterial = quadMat;
      Material tempMaterial = new Material(meshRender.sharedMaterial);
      meshRender.sharedMaterial = tempMaterial;
      quadMat = tempMaterial;
    }

    blockMat.SetColor("_Color", blockColor);

    blurMat.SetFloat("_Intensity", 1f);

    quadMat.SetFloat("_Dist", 0f);
    quadMat.SetColor("_EmptyColor", color);
    quadMat.SetColor("_FillColor", fillColor);
  }

  void setTexture()
  {
    if (quadMat && blockRendTex)
      quadMat.SetTexture("_MainTex", blockRendTex);
  }

  void setQuadMeshData()
  {
    if (!quadMeshes)
    {
      quadMeshes = new Mesh
      {
        name = gameObject.name
      };
    }
    NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    attr[1] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
    attr[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

    quadMeshes.SetVertexBufferParams(4, attr);
    attr.Dispose();
    float currHeight = QuadHeight / 2f;
    float currWidth = QuadWidth / 2f;

    NativeArray<VertexType> vertex = new NativeArray<VertexType>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

    half h0 = new half(0f), h1 = new half(1f);

    vertex[0] = new VertexType { pos = new Vector3(-currWidth, -currHeight), color = color, uv = new half2(h0, h0) };
    vertex[1] = new VertexType { pos = new Vector3(currWidth, -currHeight), color = color, uv = new half2(h1, h0) };
    vertex[2] = new VertexType { pos = new Vector3(-currWidth, currHeight), color = color, uv = new half2(h0, h1) };
    vertex[3] = new VertexType { pos = new Vector3(currWidth, currHeight), color = color, uv = new half2(h1, h1) };

    quadMeshes.SetVertexBufferData(vertex, 0, 0, 4);
    vertex.Dispose();

    int indexCount = 6;
    quadMeshes.SetIndexBufferParams(indexCount, IndexFormat.UInt16);
    quadMeshes.SetIndexBufferData(new short[6] { 0, 2, 1, 1, 2, 3 }, 0, 0, indexCount);

    quadMeshes.subMeshCount = 1;
    quadMeshes.bounds = new Bounds
    {
      center = transform.localPosition,
      extents = new Vector3(currWidth, currHeight)
    };
    quadMeshes.SetSubMesh(0, new SubMeshDescriptor
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
    filter.mesh = quadMeshes;
  }

  private void setBlockMesh()
  {
    if (!blockMesh)
    {
      blockMesh = new Mesh
      {
        name = gameObject.name
      };
    }
    blockMesh.Clear();

    int totalAttribute = 2;
    int vertexPerBlock = 8;
    int vertexCount = vertexPerBlock * blocks.Count;
    NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(totalAttribute, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    attr[1] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 3);

    blockMesh.SetVertexBufferParams(vertexCount, attr);
    attr.Dispose();
    float currHeight = BlockThickness / 2f;
    float currDepth = BlockThickness / 2f;

    NativeArray<VertexTypeBlock> vertex = new NativeArray<VertexTypeBlock>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    for (int i = 0; i < blocks.Count; i++)
    {
      float currWidth = blocks[i].length / 2f;
      int padding = i * vertexPerBlock;
      Vector3 pos = blocks[i].pos;
      vertex[padding] = new VertexTypeBlock { pos = new Vector3(pos.x - currWidth, pos.y - currHeight, pos.z - currDepth), uv = new Vector3(0, 0, 0) };
      vertex[padding + 1] = new VertexTypeBlock { pos = new Vector3(pos.x + currWidth, pos.y - currHeight, pos.z - currDepth), uv = new Vector3(1, 0, 0) };
      vertex[padding + 2] = new VertexTypeBlock { pos = new Vector3(pos.x + currWidth, pos.y + currHeight, pos.z - currDepth), uv = new Vector3(1, 1, 0) };
      vertex[padding + 3] = new VertexTypeBlock { pos = new Vector3(pos.x - currWidth, pos.y + currHeight, pos.z - currDepth), uv = new Vector3(0, 1, 0) };
      vertex[padding + 4] = new VertexTypeBlock { pos = new Vector3(pos.x - currWidth, pos.y - currHeight, pos.z + currDepth), uv = new Vector3(0, 0, 1) };
      vertex[padding + 5] = new VertexTypeBlock { pos = new Vector3(pos.x + currWidth, pos.y - currHeight, pos.z + currDepth), uv = new Vector3(1, 0, 1) };
      vertex[padding + 6] = new VertexTypeBlock { pos = new Vector3(pos.x + currWidth, pos.y + currHeight, pos.z + currDepth), uv = new Vector3(1, 1, 1) };
      vertex[padding + 7] = new VertexTypeBlock { pos = new Vector3(pos.x - currWidth, pos.y + currHeight, pos.z + currDepth), uv = new Vector3(0, 1, 1) };
    }

    blockMesh.SetVertexBufferData(vertex, 0, 0, vertexCount);
    vertex.Dispose();

    int indexPerBlock = 36;
    int indexCount = indexPerBlock * blocks.Count;
    blockMesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

    NativeArray<int> indices = new NativeArray<int>(indexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    for (int i = 0; i < blocks.Count; i++)
    {
      int idxPadding = i * indexPerBlock;
      int indicesPadding = i * vertexPerBlock;

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
    blockMesh.SetIndexBufferData(indices, 0, 0, indexCount);

    blockMesh.subMeshCount = 1;
    blockMesh.SetSubMesh(0, new SubMeshDescriptor
    {
      indexStart = 0,
      indexCount = indexCount,
      topology = MeshTopology.Triangles,
      baseVertex = 0,
    });
  }

  IEnumerator<object> updateBlockPos()
  {
    // Right to left
    while (true)
    {
      yield return PersistentData.Instance.WaitForFrameEnd;

      float delta = Time.deltaTime;

      for (int i = 0; i < blocks.Count; i++)
      {
        BlockData block = blocks[i];
        float nextX = block.pos.x - delta * block.speed;
        float nextZ = block.pos.z;
        if (nextX < posBound.x)
        {
          nextX = posBound.y;
          block.speed = UnityEngine.Random.Range(SpeedBound.x, SpeedBound.y);
          nextZ = UnityEngine.Random.Range(-100, 100);
          block.length = UnityEngine.Random.Range(LengthBound.x, LengthBound.y);
        }
        block.pos = new Vector3(nextX, block.pos.y, nextZ);
        blocks[i] = block;
      }
    }
  }

  IEnumerator<object> renderBlock()
  {
    while (true)
    {
      yield return PersistentData.Instance.WaitForFrameEnd;
      if (blockMesh)
      {
        blockMesh.Clear();
      }

      setBlockMesh();

      if (!blockMesh || cmdBuffer == null || !blockRendTex) continue;

      Vector3 camPos = camForMatrix.transform.position;

      camPos.Set(camPos.x, camPos.y, camPos.z);

      blockMat?.SetVector("_CameraPos", new Vector4(camPos.x, camPos.y, camPos.z, camForMatrix.farClipPlane));
      cmdBuffer.Clear();
      Matrix4x4 lookMatrix = Util.CreateViewMatrix(camPos, camForMatrix.transform.rotation, camForMatrix.transform.localScale);
      Matrix4x4 orthoMatrix = Matrix4x4.Perspective(60f, QuadWidth / QuadHeight, 0.03f, 700f);
      cmdBuffer.SetViewProjectionMatrices(lookMatrix, orthoMatrix);

      cmdBuffer.SetRenderTarget(blockRendTex);
      cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
      cmdBuffer.DrawMesh(blockMesh, Matrix4x4.identity, blockMat, 0, 0);

      int blur1 = Shader.PropertyToID("_Temp1");
      int blur2 = Shader.PropertyToID("_Temp2");

      cmdBuffer.GetTemporaryRT(blur1, (int)QuadWidth, (int)QuadHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
      cmdBuffer.GetTemporaryRT(blur2, (int)QuadWidth, (int)QuadHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);

      cmdBuffer.Blit(blockRendTex, blur1);

      for (var i = 0; i < blurIteration; i++)
      {
        cmdBuffer.Blit(blur1, blur2, blurMat, 0);
        cmdBuffer.Blit(blur2, blur1, blurMat, 0);
      }

      cmdBuffer.Blit(blur1, blockRendTex);

      cmdBuffer.ReleaseTemporaryRT(blur1);
      cmdBuffer.ReleaseTemporaryRT(blur2);

      Util.ClearWebViewScreen(cmdBuffer);

      Graphics.ExecuteCommandBuffer(cmdBuffer);
    }
  }

  public void ClearRender()
  {
    if (cmdBuffer == null || !blockRendTex) return;
    cmdBuffer.Clear();
    cmdBuffer.SetRenderTarget(blockRendTex);
    cmdBuffer.ClearRenderTarget(true, true, color, 1f);

    Util.ClearWebViewScreen(cmdBuffer);

    Graphics.ExecuteCommandBuffer(cmdBuffer);
  }

  public void GoToMainMenuPos()
  {
    Vector3 target = new Vector3(-250, 0, 0);
    Vector3 targetEuler = new Vector3(0, 90, 90);

    Vector3 currPos = new Vector3(camForMatrix.transform.position.x, camForMatrix.transform.position.y, camForMatrix.transform.position.z);
    Vector3 currEuler = new Vector3(camForMatrix.transform.eulerAngles.x, camForMatrix.transform.eulerAngles.y, camForMatrix.transform.eulerAngles.z);

    BaseTween<Camera> baseTween = new BaseTween<Camera>(
      0.6f,
      camForMatrix,
      (dist, cam) =>
      {

      },
      (dist, cam) =>
      {
        float easeDist = Util.EaseOut(dist, 3);
        Vector3 distDelta = (target - currPos) * easeDist;
        Vector3 distEuler = (targetEuler - currEuler) * easeDist;

        camForMatrix.transform.position = new Vector3(distDelta.x + currPos.x, distDelta.y + currPos.y, distDelta.z + currPos.z);
        camForMatrix.transform.eulerAngles = new Vector3(distEuler.x + currEuler.x, distEuler.y + currEuler.y, distEuler.z + currEuler.z);
      },
      (dist, cam) =>
      {
        UiEvent.Instance.GameEndAnimFinish();
        setBlur(false);
      }
    );

    IEnumerator<object> tween = Tween.Create(baseTween);
    StartCoroutine(tween);
  }

  public void GoToGameplayPos()
  {
    Vector3 target = new Vector3(0, 0, -600);
    Vector3 targetEuler = new Vector3(0, 0, 0);

    Vector3 currPos = new Vector3(camForMatrix.transform.position.x, camForMatrix.transform.position.y, camForMatrix.transform.position.z);
    Vector3 currEuler = new Vector3(camForMatrix.transform.eulerAngles.x, camForMatrix.transform.eulerAngles.y, camForMatrix.transform.eulerAngles.z);

    BaseTween<Camera> baseTween = new BaseTween<Camera>(
      0.6f,
      camForMatrix,
      (dist, cam) =>
      {

      },
      (dist, cam) =>
      {
        float easeDist = Util.EaseOut(dist, 3);
        Vector3 distDelta = (target - currPos) * dist;
        Vector3 distEuler = (targetEuler - currEuler) * easeDist;

        camForMatrix.transform.position = new Vector3(distDelta.x + currPos.x, distDelta.y + currPos.y, distDelta.z + currPos.z);
        camForMatrix.transform.eulerAngles = new Vector3(distEuler.x + currEuler.x, distEuler.y + currEuler.y, distEuler.z + currEuler.z);
      },
      (dist, cam) =>
      {
        setBlur(true);
      }
    );

    IEnumerator<object> tween = Tween.Create(baseTween);
    StartCoroutine(tween);
  }

  public void setBlur(bool isOn)
  {
    BaseTween<object> baseTween = new BaseTween<object>(
      0.3f,
      null,
      (dist, _) => { },
      (dist, _) =>
      {
        blurIteration = Mathf.FloorToInt((!isOn ? (1 - dist) : dist) * 10f);
      },
      (dist, _) =>
      {
        blurIteration = Mathf.FloorToInt((!isOn ? (1 - dist) : dist) * 10f);

        if (isOn)
        {
          UiEvent.Instance.CameraMoveFinish();
        }
      }
    );

    IEnumerator<object> tween = Tween.Create(baseTween);
    StartCoroutine(tween);
  }

  void onMainPlayerEat(float dist)
  {
    changeDist(dist);
  }

  void onMainPlayerFire(float dist)
  {
    changeDist(dist, false);
  }

  void onGameOver(GameOverData _)
  {
    changeDist(0, false);
  }

  void changeDist(float bgDist, bool shouldFlash = true)
  {
    if (PersistentData.Instance.isPaused) bgDist = 0;

    if (distChangedCour != null)
    {
      StopCoroutine(distChangedCour);
    }

    float startDist = currDist;
    BaseTween<object> tweenData = new BaseTween<object>(
      1f,
      null,
      (dist, obj) =>
      {
        currDist = startDist;
        if (quadMat)
        {
          quadMat.SetFloat("_Dist", currDist);
          quadMat.SetFloat("_EatRatio", 0);
        }
      },
      (dist, obj) =>
      {
        float easeOutDist = Util.EaseOut(dist, 3);
        float eatRatio = -4 * Mathf.Pow(easeOutDist - 0.5f, 2) + 1;

        currDist = startDist + (bgDist - startDist) * easeOutDist;
        if (quadMat)
        {
          quadMat.SetFloat("_Dist", currDist);
          if (shouldFlash) quadMat.SetFloat("_EatRatio", eatRatio);
        }
      },
      (dist, obj) =>
      {
        currDist = bgDist;
        if (quadMat)
        {
          quadMat.SetFloat("_Dist", currDist);
          quadMat.SetFloat("_EatRatio", 0);
        }
      }
    );
    IEnumerator<object> tween = Tween.Create(tweenData);

    distChangedCour = StartCoroutine(tween);
  }

  void OnDestroy()
  {
    if (quadMat)
    {
      Destroy(quadMat);
    }

    GameEvent.Instance.onGameOver -= onGameOver;
    GameEvent.Instance.onMainPlayerEat -= onMainPlayerEat;
    GameEvent.Instance.onMainPlayerFire -= onMainPlayerFire;
  }
}
