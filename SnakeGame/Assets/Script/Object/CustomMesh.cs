#nullable enable
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomMesh : MonoBehaviour
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct VertexType
    {
        public Vector3 pos;

        public Vector3 uv;
    }
    [SerializeField]
    float size = 200f;

    [SerializeField]
    Material? mat = null;

    Mesh? mesh = null;

    MeshRenderer? meshRender = null;


    void Awake()
    {
        transform.localScale = new Vector3(size * 0.5f, size * 0.5f, size * 0.5f);
        setMeshRender();
        setMesh();
    }

    void setMeshRender()
    {
        meshRender = GetComponent<MeshRenderer>();
        if (meshRender == null)
        {
            meshRender = gameObject.AddComponent<MeshRenderer>();
        }

        meshRender.material = mat;
    }


    void setMesh()
    {
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
        int vertexCount = vertexPerSpike;
        NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(totalAttribute, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
        attr[1] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 3);

        mesh.SetVertexBufferParams(vertexCount, attr);
        attr.Dispose();
        float currHeight = 0.5f;
        float currWidth = 0.5f;
        float currDepth = 0.5f;

        NativeArray<VertexType> vertex = new NativeArray<VertexType>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        half h0 = new half(0f), h1 = new half(1f), h05 = new half(0.5f);

        vertex[0] = new VertexType { pos = new Vector3(-currWidth, -currHeight, -currDepth), uv = new Vector3(0, 0, 0) };
        vertex[1] = new VertexType { pos = new Vector3(+currWidth, -currHeight, -currDepth), uv = new Vector3(1, 0, 0) };
        vertex[2] = new VertexType { pos = new Vector3(+currWidth, +currHeight, -currDepth), uv = new Vector3(1, 1, 0) };
        vertex[3] = new VertexType { pos = new Vector3(-currWidth, +currHeight, -currDepth), uv = new Vector3(0, 1, 0) };
        vertex[4] = new VertexType { pos = new Vector3(-currWidth, -currHeight, currDepth), uv = new Vector3(0, 0, 1) };
        vertex[5] = new VertexType { pos = new Vector3(+currWidth, -currHeight, currDepth), uv = new Vector3(1, 0, 1) };
        vertex[6] = new VertexType { pos = new Vector3(+currWidth, +currHeight, currDepth), uv = new Vector3(1, 1, 1) };
        vertex[7] = new VertexType { pos = new Vector3(-currWidth, +currHeight, currDepth), uv = new Vector3(0, 1, 1) };

        mesh.SetVertexBufferData(vertex, 0, 0, vertexCount);
        vertex.Dispose();

        int indexPerSpike = 36;
        int indexCount = indexPerSpike;
        mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

        NativeArray<int> indices = new NativeArray<int>(indexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        // Front
        indices[0] = 2;
        indices[1] = 1;
        indices[2] = 0;

        indices[3] = 0;
        indices[4] = 3;
        indices[5] = 2;

        // Right
        indices[6] = 6;
        indices[7] = 5;
        indices[8] = 1;

        indices[9] = 1;
        indices[10] = 2;
        indices[11] = 6;


        // Back
        indices[12] = 7;
        indices[13] = 4;
        indices[14] = 5;

        indices[15] = 5;
        indices[16] = 6;
        indices[17] = 7;

        // Left
        indices[18] = 3;
        indices[19] = 0;
        indices[20] = 4;

        indices[21] = 4;
        indices[22] = 7;
        indices[23] = 3;

        // Top
        indices[24] = 6;
        indices[25] = 2;
        indices[26] = 3;

        indices[27] = 3;
        indices[28] = 7;
        indices[29] = 6;

        // Bot
        indices[30] = 1;
        indices[31] = 5;
        indices[32] = 4;

        indices[33] = 4;
        indices[34] = 0;
        indices[35] = 1;

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
