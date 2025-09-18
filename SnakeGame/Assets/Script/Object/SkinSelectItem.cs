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

  [SerializeField]
  GameObject? selectSprite = null;

  [SerializeField]
  CustomSprite? sprite = null;

  Material? mat = null;

  public SkinDetail? SkinData = null;

  bool _isSelected = false;
  public bool IsSelected
  {
    get
    {
      return _isSelected;
    }
    set
    {
      _isSelected = value;
      setBackground();
    }
  }

  Texture2D? tex = null;

  RenderTexture? rendTex = null;

  Mesh? mesh = null;

  CommandBuffer? cmdBuffer = null;

  void Awake()
  {
    cmdBuffer = new CommandBuffer();
  }

  void OnEnable()
  {
    GameObject targetObj = GameObject.FindGameObjectWithTag("Draw");
    if (targetObj)
    {
      sprite = targetObj.GetComponent<CustomSprite>();
    }
    setName();
    getImage();

    StartCoroutine(render());
  }

  IEnumerator<object> render()
  {
    while (true)
    {
      yield return new WaitForEndOfFrame();
      drawRenderTex();
    }
  }

  public void SetSkinData(SkinDetail data)
  {
    SkinData = data;
    if (tex == null)
    {
      setName();
      getImage();
    }
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
    ResourceRequest request = Resources.LoadAsync<Texture2D>(SkinData?.texture_name ?? "");

    while (!request.isDone)
    {
      yield return null;
    }
    Texture2D? loadedTexture = request.asset as Texture2D;

    if (loadedTexture != null)
    {
      setTexture(loadedTexture);
    }
    else
    {
      Debug.LogError("Failed to load asset at path: " + SkinData?.texture_name);
    }
  }

  void setTexture(Texture2D tex)
  {
    if (!preview) return;

    if (tex)
    {
      this.tex = tex;
      rendTex = new RenderTexture(
      (int)preview.rectTransform.rect.width,
      (int)preview.rectTransform.rect.height,
      UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
      UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat_S8_UInt
    );
      rendTex.enableRandomWrite = true;
    }

    if (sprite)
    {
      sprite.Texture = rendTex;
    }

    preview.texture = rendTex;
    SetMesh();
    setMat();
  }

  void setMat()
  {
    if (!preview || SkinData == null) return;

    if (!mat)
    {
      Shader shader = Shader.Find(SkinData.shader_name);
      if (shader)
      {
        mat = new Material(shader);
        mat.SetTexture("_MainTex", tex);
      }
    }
  }

  void SetMesh()
  {
    if (!tex) return;

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
    float currHeight = 100 / 2f;
    float currWidth = 100 / 2f;

    NativeArray<VertexType> vertex = new NativeArray<VertexType>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

    half h0 = new half(0f), h1 = new half(1f);

    vertex[0] = new VertexType { pos = new Vector3(-currWidth, -currHeight, 10), color = Color.white, uv = new half2(h0, h0) };
    vertex[1] = new VertexType { pos = new Vector3(currWidth, -currHeight, 10), color = Color.white, uv = new half2(h1, h0) };
    vertex[2] = new VertexType { pos = new Vector3(-currWidth, currHeight, 10), color = Color.white, uv = new half2(h0, h1) };
    vertex[3] = new VertexType { pos = new Vector3(currWidth, currHeight, 10), color = Color.white, uv = new half2(h1, h1) };

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
    if (cmdBuffer == null || rendTex == null || mat == null) return;

    cmdBuffer.Clear();
    var lookMatrix = Camera.main.worldToCameraMatrix;
    var orthoMatrix = Matrix4x4.Ortho(-rendTex.width / 2, rendTex.width / 2, -rendTex.height / 2, rendTex.height / 2, 0.3f, 1000f);
    cmdBuffer.SetViewProjectionMatrices(lookMatrix, orthoMatrix);

    cmdBuffer.SetRenderTarget(rendTex);
    cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
    cmdBuffer.DrawMesh(mesh, Matrix4x4.identity, mat, 0, 0);

    Graphics.ExecuteCommandBuffer(cmdBuffer);
  }

  void setBackground()
  {
    if (!selectSprite) return;

    selectSprite.SetActive(_isSelected);
  }

  public void Select()
  {
    StartCoroutine(getTextureAndLoadImage());
    UiEvent.Instance.SkinSelected(
      SkinData?.id ?? 0
    );
  }
}
