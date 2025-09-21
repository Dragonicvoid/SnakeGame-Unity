#nullable enable
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using System.Linq;

[ExecuteInEditMode]
public class CustomSprite : MonoBehaviour
{
  [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
  struct VertexType
  {
    public Vector3 pos;
    public Color color;
    public half2 uv;
  }

  [SerializeField]
  Texture? _texture;
  public Texture? Texture
  {
    get { return _texture; }
    set
    {
      _texture = value;
      Render();
    }
  }

  [SerializeField]
  float _width = 100f;
  public float Width
  {
    get { return _width; }
    set
    {
      _width = value;
      updateMesh();
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
      updateMesh();
    }
  }

  [SerializeField]
  Color _color = Color.white;
  Color color
  {
    get { return _color; }
    set
    {
      _color = value;
      updateMesh();
    }
  }

  [SerializeField]
  int repeat = 1;

  [SerializeField]
  Vector2 tiling = new Vector2(1f, 1f);

  [SerializeField]
  Vector2 offset = new Vector2(0f, 0f);

  Material? _mat;

  Mesh? mesh;

  MeshRenderer? meshRend;

  void OnEnable()
  {
    setMaterial();
    setTexture();
    updateMesh();
  }

  void OnValidate()
  {
    setMaterial();
    setTexture();
    updateMesh();
  }

  public void Render()
  {
    setMaterial();
    setTexture();
    updateMesh();
  }

  void setMaterial()
  {
    meshRend = GetComponent<MeshRenderer>();
    if (!meshRend)
    {
      meshRend = gameObject.AddComponent<MeshRenderer>();
    }

    if (Application.isPlaying && meshRend.materials.Length > 0)
    {
      _mat = meshRend.materials[0];
    }

    if (!_mat)
    {
      Shader shader = Shader.Find("Transparent/CustomSprite");
      _mat = new Material(shader);

      if (Application.isPlaying)
      {
        if (meshRend.materials.Length > 0)
        {
          meshRend.materials[0] = _mat;
        }
        else
        {
          meshRend.materials.Append(_mat);
        }
      }
    }
    _mat.SetTextureOffset("_MainTex", offset);
    _mat.SetTextureScale("_MainTex", tiling);
    _mat.SetInt("_Repeat", repeat);
  }

  void setTexture()
  {
    if (_mat && _texture)
      _mat.SetTexture("_MainTex", _texture);
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
    NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
    attr[1] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
    attr[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

    mesh.SetVertexBufferParams(4, attr);
    attr.Dispose();
    float currHeight = _height / 2f;
    float currWidth = _width / 2f;

    NativeArray<VertexType> vertex = new NativeArray<VertexType>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

    half h0 = new half(0f), h1 = new half(1f);

    vertex[0] = new VertexType { pos = new Vector3(-currWidth, -currHeight), color = _color, uv = new half2(h0, h0) };
    vertex[1] = new VertexType { pos = new Vector3(currWidth, -currHeight), color = _color, uv = new half2(h1, h0) };
    vertex[2] = new VertexType { pos = new Vector3(-currWidth, currHeight), color = _color, uv = new half2(h0, h1) };
    vertex[3] = new VertexType { pos = new Vector3(currWidth, currHeight), color = _color, uv = new half2(h1, h1) };

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

  private void updateMesh()
  {
    if (mesh)
    {
      mesh.Clear();
    }

    setMeshData();
  }

  private void destroyMat()
  {
    if (_mat)
    {
      Destroy(_mat);
    }
  }
}
