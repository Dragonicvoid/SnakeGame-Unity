#nullable enable
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SkinSelectItem : MonoBehaviour
{
  struct VertexType
  {
    public Vector3 pos;
    public Color color;
    public half2 uv;
  }
  [SerializeField]
  RawImage? preview = null;

  [SerializeField]
  Text? labelName = null;

  Material? mat = null;

  public SkinDetail? SkinData = null;

  public bool IsSelected = false;

  Texture2D? tex = null;

  Texture2D? normalMap = null;

  RenderTexture? rendTex = null;

  Mesh? mesh = null;

  CommandBuffer? cmdBuffer = null;

  void Awake()
  {
    cmdBuffer = new CommandBuffer();
  }

  void OnEnable()
  {
    setName();
    getImage();

    StartCoroutine(render());
  }

  IEnumerator<object> render()
  {
    while (true)
    {
      yield return PersistentData.Instance.WaitForFrameEnd;
      drawRenderTex();
    }
  }

  public void SetSkinData(SkinDetail data)
  {
    SkinData = data;
    setName();
    getImage();
  }

  void setName()
  {
    if (!labelName || SkinData == null) return;

    labelName.text = SkinData.name;
  }

  void getImage()
  {
    if (!preview || SkinData == null) return;

    StartCoroutine(getTextureAndLoadImage());
  }

  IEnumerator<object> getTextureAndLoadImage()
  {
    if ((SkinData?.texture_name == null || SkinData?.texture_name == "")
        && (SkinData?.normal_tex_name == null || SkinData?.normal_tex_name == ""))
    {
      setTexture(null, null);
      yield return null;
      yield break;
    }

    Texture2D? loadedTexture = null;
    Texture2D? loadedNormalMap = null;

    if (SkinData != null)
    {
      AssetManager.Instance.assetsTexture.TryGetValue(SkinData.texture_name, out loadedTexture);
      AssetManager.Instance.assetsTexture.TryGetValue(SkinData.normal_tex_name, out loadedNormalMap);
    }

    if (loadedTexture == null && SkinData != null && SkinData.texture_name != "")
    {
      Debug.LogError("Failed to load Texture for: " + SkinData?.name);
      loadedTexture = null;
    }

    if (loadedNormalMap == null && SkinData != null && SkinData.normal_tex_name != "")
    {
      Debug.LogError("Failed to load Normal Map for: " + SkinData?.name);
      loadedNormalMap = null;
    }

    setTexture(loadedTexture, loadedNormalMap);
  }

  void setTexture(Texture2D? tex, Texture2D? normalMap)
  {
    if (!preview) return;

    if (this.tex)
    {
      this.tex = null;
    }

    if (this.normalMap)
    {
      this.normalMap = null;
    }

    this.tex = tex;
    this.normalMap = normalMap;

    if (!rendTex)
    {
      rendTex = new RenderTexture(
        (int)preview.rectTransform.rect.width,
        (int)preview.rectTransform.rect.height,
        Util.GetGraphicFormat(),
        Util.GetDepthFormat()
      );
    }
    Util.ClearDepthRT(rendTex, cmdBuffer, true);
    preview.texture = rendTex;
    SetMesh();
    setMat();
  }

  void setMat()
  {
    if (SkinData == null) return;

    if (mat)
    {
      if (!Application.isEditor) Destroy(mat);
    }

    Shader shader = Shader.Find(SkinData.shader_name);
    if (shader)
    {
      mat = new Material(shader);
    }

    if (tex)
    {
      mat?.SetTexture("_MainTex", tex);
    }

    if (normalMap)
    {
      mat?.SetTexture("_NormalMap", normalMap);
    }
  }

  void SetMesh()
  {
    if (!mesh)
    {
      mesh = new Mesh { name = SkinData.name + "_Mesh" };
    }

    NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    attr[1] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
    attr[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

    mesh.SetVertexBufferParams(4, attr);
    attr.Dispose();
    float currHeight = (int)(preview?.rectTransform.rect.height ?? 0) / 2f;
    float currWidth = (int)(preview?.rectTransform.rect.width ?? 0) / 2f;

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

  void drawRenderTex()
  {
    if (cmdBuffer == null || rendTex == null || mat == null || mesh == null) return;

    cmdBuffer.Clear();
    var lookMatrix = Util.CreateViewMatrix(new Vector3(0, 0, -10), Quaternion.identity, Vector3.one).inverse;
    var orthoMatrix = Matrix4x4.Ortho(-rendTex.width / 2, rendTex.width / 2, -rendTex.height / 2, rendTex.height / 2, 0.3f, 1000f);
    cmdBuffer.SetViewProjectionMatrices(lookMatrix, orthoMatrix);

    int temp1 = Shader.PropertyToID("_Temp1");
    cmdBuffer.GetTemporaryRT(temp1, rendTex.width, rendTex.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);

    cmdBuffer.SetRenderTarget(temp1);
    cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
    cmdBuffer.DrawMesh(mesh, Matrix4x4.identity, mat, 0, (int)SNAKE_RENDER_PASS.PREVIEW);
    cmdBuffer.Blit(temp1, rendTex);

    cmdBuffer.ReleaseTemporaryRT(temp1);

    // Hack resize Web-view
    cmdBuffer.SetRenderTarget(PersistentData.Instance.RenderTex);
    cmdBuffer.ClearRenderTarget(false, false, Color.clear, 1f);

    Graphics.ExecuteCommandBuffer(cmdBuffer);
  }

  public void Select()
  {
    StartCoroutine(getTextureAndLoadImage());
    UiEvent.Instance.SkinSelected(
      SkinData?.id ?? 0,
      true
    );
  }
}
