
using UnityEngine;

// This code is from Volume-Rendering Code
// by github.com/mattatz
// credit: https://github.com/mattatz/unity-volume-rendering
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class VolumeRender : MonoBehaviour
{
  [SerializeField] protected Shader shader;
  protected Material material;
  [SerializeField] Color color = Color.white;

  Texture? tex;

  protected void Awake()
  {
    GetComponent<MeshFilter>().mesh = Build();
    setMaterial();

    gameObject.transform.localScale = new Vector3(ARENA_DEFAULT_SIZE.WIDTH, ARENA_DEFAULT_SIZE.WIDTH, 2);
  }

  public void SetTexture(Texture? tex)
  {
    this.tex = tex;
    setMaterial();
  }

  void setMaterial()
  {
    if (!material)
    {
      material = new Material(shader);
      GetComponent<MeshRenderer>().material = material;
    }

    material.SetTexture("_MainTex", tex);
    material.SetColor("_Color", color);
  }

  Mesh Build()
  {
    var vertices = new Vector3[] {
      new Vector3 (-0.5f, -0.5f, -0.5f),
      new Vector3 ( 0.5f, -0.5f, -0.5f),
      new Vector3 ( 0.5f,  0.5f, -0.5f),
      new Vector3 (-0.5f,  0.5f, -0.5f),
      new Vector3 (-0.5f,  0.5f,  0.5f),
      new Vector3 ( 0.5f,  0.5f,  0.5f),
      new Vector3 ( 0.5f, -0.5f,  0.5f),
      new Vector3 (-0.5f, -0.5f,  0.5f),
    };
    var triangles = new int[] {
      0, 2, 1,
      0, 3, 2,
      2, 3, 4,
      2, 4, 5,
      1, 2, 5,
      1, 5, 6,
      0, 7, 4,
      0, 4, 3,
      5, 4, 7,
      5, 7, 6,
      0, 6, 7,
      0, 1, 6
    };

    var mesh = new Mesh();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
    mesh.hideFlags = HideFlags.HideAndDontSave;
    return mesh;
  }

  void OnDestroy()
  {
    Destroy(material);
  }
}
