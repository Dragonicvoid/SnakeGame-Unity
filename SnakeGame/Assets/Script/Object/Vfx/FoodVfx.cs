using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class FoodVfx : MonoBehaviour
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct VertexType
    {
        public Vector3 pos;
        public Color color;
        public half2 uv;
        public float isMain;
    }

    [SerializeField]
    Color _color = Color.white;
    Color color
    {
        get { return _color; }
        set
        {
            _color = value;
            setMeshData();
        }
    }

    [SerializeField]
    Vector2 size = new Vector2();

    Vector2 rotationDeg = new Vector2(0, 0);

    Material? mat;

    Mesh? mesh;

    MeshRenderer? meshRend;

    Coroutine? renderCour;

    void OnEnable()
    {
        renderCour = StartCoroutine(Render());
    }

    void Awake()
    {
        Debug.Log("Create VFX");
        setMaterial();
        setMeshData();
    }

    IEnumerator<object> Render()
    {
        yield return null;

        while (true)
        {
            yield return null;
            updateRotPos();
            rotate();
            setMeshData();
        }
    }

    void updateRotPos()
    {
        float delta = Time.deltaTime * 360f;
        rotationDeg.Set((rotationDeg.x - delta * 2) % 360f, (rotationDeg.y + delta) % 360f);
    }

    void rotate()
    {
        float delta = Time.deltaTime * 160f;
        float eulerAngles = gameObject.transform.eulerAngles.z;
        gameObject.transform.eulerAngles = new Vector3(0, 0, (eulerAngles + delta) % 360);
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
            Shader shader = Shader.Find("Transparent/FoodShader");
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
        NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
        attr[1] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
        attr[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);
        attr[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 1);

        mesh.SetVertexBufferParams(8, attr);
        attr.Dispose();
        float currHeight = size.y / 2f;
        float currWidth = size.x / 2f;

        NativeArray<VertexType> vertex = new NativeArray<VertexType>(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        half h0 = new half(0f), h1 = new half(1f);

        vertex[0] = new VertexType { pos = new Vector3(-currWidth, -currHeight, 0), color = _color, uv = new half2(h0, h0), isMain = 1f };
        vertex[1] = new VertexType { pos = new Vector3(currWidth, -currHeight, 0), color = _color, uv = new half2(h1, h0), isMain = 1f };
        vertex[2] = new VertexType { pos = new Vector3(-currWidth, currHeight, 0), color = _color, uv = new half2(h0, h1), isMain = 1f };
        vertex[3] = new VertexType { pos = new Vector3(currWidth, currHeight, 0), color = _color, uv = new half2(h1, h1), isMain = 1f };

        List<Vector3> neuronRot = getRotPos();
        currHeight = size.y / 6f;
        currWidth = size.x / 6f;

        vertex[4] = new VertexType { pos = new Vector3(-currWidth + neuronRot[0].x, -currHeight + neuronRot[0].y, neuronRot[0].z), color = _color, uv = new half2(h0, h0), isMain = 0f };
        vertex[5] = new VertexType { pos = new Vector3(currWidth + neuronRot[0].x, -currHeight + neuronRot[0].y, neuronRot[0].z), color = _color, uv = new half2(h1, h0), isMain = 0f };
        vertex[6] = new VertexType { pos = new Vector3(-currWidth + neuronRot[0].x, currHeight + neuronRot[0].y, neuronRot[0].z), color = _color, uv = new half2(h0, h1), isMain = 0f };
        vertex[7] = new VertexType { pos = new Vector3(currWidth + neuronRot[0].x, currHeight + neuronRot[0].y, neuronRot[0].z), color = _color, uv = new half2(h1, h1), isMain = 0f };

        // vertex[8] = new VertexType { pos = new Vector3(-currWidth + neuronRot[1].x, -currHeight + neuronRot[1].y, neuronRot[1].z), color = _color, uv = new half2(h0, h0), isMain = 0f };
        // vertex[9] = new VertexType { pos = new Vector3(currWidth + neuronRot[1].x, -currHeight + neuronRot[1].y, neuronRot[1].z), color = _color, uv = new half2(h1, h0), isMain = 0f };
        // vertex[10] = new VertexType { pos = new Vector3(-currWidth + neuronRot[1].x, currHeight + neuronRot[1].y, neuronRot[1].z), color = _color, uv = new half2(h0, h1), isMain = 0f };
        // vertex[11] = new VertexType { pos = new Vector3(currWidth + neuronRot[1].x, currHeight + neuronRot[1].y, neuronRot[1].z), color = _color, uv = new half2(h1, h1), isMain = 0f };

        mesh.SetVertexBufferData(vertex, 0, 0, 8);
        vertex.Dispose();

        int indexCount = 12;
        mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt16);
        mesh.SetIndexBufferData(new short[12] {
            0, 2, 1,
            1, 2, 3,

            4, 6, 5,
            5, 6, 7

            // 8, 10, 9,
            // 9, 10, 11
            },
        0, 0, indexCount);

        mesh.subMeshCount = 1;
        mesh.bounds = new Bounds
        {
            center = transform.localPosition,
            extents = new Vector3(currWidth, currHeight)
        };
        mesh.SetSubMesh(0, new SubMeshDescriptor
        {
            indexStart = 0,
            indexCount = 12,
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

    private List<Vector3> getRotPos()
    {
        List<Vector3> rotPos = new List<Vector3>();

        float currRot = rotationDeg.x * Mathf.Deg2Rad;
        Vector3 startRot = Vector3.right * size.x * 0.75f;
        rotPos.Add(
            new Vector3(
                startRot.x * Mathf.Cos(currRot) + startRot.z * Mathf.Sin(currRot),
                startRot.y,
                Mathf.Clamp(startRot.x * -Mathf.Sin(currRot) + startRot.z * Mathf.Cos(currRot), -0.5f, 0.5f)
            )
        );

        currRot = rotationDeg.y * Mathf.Deg2Rad;
        startRot = Vector3.up * size.y * 0.5f;
        rotPos.Add(
            new Vector3(
                startRot.x,
                startRot.y * Mathf.Cos(currRot) + startRot.z * -Mathf.Sin(currRot),
                Mathf.Clamp(startRot.y * Mathf.Sin(currRot) + startRot.z * Mathf.Cos(currRot), -0.5f, 0.5f)
            )
        );

        return rotPos;
    }

    private void destroyMat()
    {
        if (mat)
        {
            Destroy(mat);
        }
    }
}
