using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class Vortex : MonoBehaviour
{
  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
  struct VertexType
  {
    public Vector3 pos;
    public half2 uv;
  }

  [SerializeField]
  float _width = 100f;
  public float Width
  {
    get { return _width; }
    set
    {
      _width = value;
      setMeshData();
    }
  }

  [SerializeField]
  float _height = 100f;
  public float Height
  {
    get { return _height; }
    set
    {
      _height = value;
      setMeshData();
    }
  }

  [SerializeField]
  Color mainColor = Color.white;
  [SerializeField]
  Color secondaryColor = Color.white;
  [SerializeField]
  Color screenColor = Color.white;
  [SerializeField]
  int vortexCount = 2;
  [SerializeField]
  [Range(0.0f, 0.5f)]
  float width = 0f;

  float show = 0f;

  Material? mat;

  Mesh? mesh;

  MeshRenderer? meshRend;

  Coroutine? animCour;

  void OnEnable()
  {
    show = 0f;
    setMaterial();
    setMeshData();
    playShowAnim();
  }

  void OnValidate()
  {
    setMaterial();
  }

  void setMaterial()
  {
    meshRend = GetComponent<MeshRenderer>();
    if (!meshRend)
    {
      meshRend = gameObject.AddComponent<MeshRenderer>();
    }

    if (!mat)
    {
      Shader shader = Shader.Find("Transparent/VortexShader");
      mat = new Material(shader);
    }

    if (!Application.isEditor)
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
    else
    {
      meshRend.sharedMaterial = mat;
      Material tempMaterial = new Material(meshRend.sharedMaterial);
      meshRend.sharedMaterial = tempMaterial;
      mat = tempMaterial;
    }

    mat.SetInt("_VortexSize", vortexCount);
    mat.SetFloat("_VortexWidth", width);
    mat.SetFloat("_Show", show);

    mat.SetColor("_MainColor", mainColor);
    mat.SetColor("_SecondColor", secondaryColor);
    mat.SetColor("_ScreenColor", screenColor);
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
    NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    attr[1] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

    mesh.SetVertexBufferParams(4, attr);
    attr.Dispose();
    float currHeight = _height / 2f;
    float currWidth = _width / 2f;

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

    MeshFilter filter = GetComponent<MeshFilter>();
    if (!filter)
    {
      filter = gameObject.AddComponent<MeshFilter>();
    }
    filter.mesh = mesh;
  }

  void playShowAnim()
  {
    stopAnim();
    BaseTween<object> tweenData = new BaseTween<object>(
      0.5f,
      null,
      (dist, _) =>
      {
        show = 0;
        mat?.SetFloat("_Show", show);
      },
      (dist, _) =>
      {
        show = Util.EaseOut(dist, 3);
        mat?.SetFloat("_Show", show);
      },
      (dist, _) =>
      {
        show = 1;
        mat?.SetFloat("_Show", show);
        UiEvent.Instance.VortexComplete(this);
      }
    );

    IEnumerator<object> tween = Tween.Create(tweenData);
    animCour = StartCoroutine(tween);
  }

  public void PlayHideAnim()
  {
    stopAnim();
    BaseTween<object> tweenData = new BaseTween<object>(
      0.5f,
      null,
      (dist, _) =>
      {
        show = 1f;
        mat?.SetFloat("_Show", show);
      },
      (dist, _) =>
      {
        show = Util.EaseOut(1.0f - dist, 3);
        mat?.SetFloat("_Show", show);
      },
      (dist, _) =>
      {
        show = 0;
        mat?.SetFloat("_Show", show);

        gameObject.SetActive(false);
      }
    );

    IEnumerator<object> tween = Tween.Create(tweenData);
    animCour = StartCoroutine(tween);
  }

  void stopAnim()
  {
    if (animCour == null) return;

    StopCoroutine(animCour);
    animCour = null;
  }

  private void destroyMat()
  {
    if (mat)
    {
      Destroy(mat);
    }
  }
}
