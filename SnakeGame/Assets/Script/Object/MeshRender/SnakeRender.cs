#nullable enable
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

enum SnakeRenderPass
{
    Body = 1 << 0,
    TextureSampling = 1 << 1,
    Shinny = 1 << 2,
}

public class SnakeRender : MonoBehaviour, ISnakeRenderable
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct SnakeVertex
    {
        public Vector3 pos;
        public uint bodyCount;
        public Vector2 uv;
        public Vector2 center;
        public Vector3 nextPos;
        public Vector3 prevPos;
    }

    [SerializeField]
    public RenderTexture RendTex;
    [SerializeField]
    Material? _mat;
    public Material? Mat
    {
        get
        {
            return _mat;
        }
        set
        {
            _mat = value;
        }
    }
    [SerializeField]
    float tileSize = 20;

    public SNAKE_TYPE SnakeType { get; set; } = SNAKE_TYPE.NORMAL;
    public SkinDetail? SkinData { get; set; }

    public List<SnakeBody> SnakeBodies { set; get; }

    Mesh? mesh;

    MeshRenderer? meshRend;

    CommandBuffer? cmdBuffer;

    int snakePass = (int)SnakeRenderPass.Body;

    void Awake()
    {
        SkinData = new SkinDetail();
        cmdBuffer = new CommandBuffer();
    }

    public void SetSnakeBody(List<SnakeBody> bodies)
    {
        SnakeBodies = bodies;
        Render();
    }

    void randomizeBody()
    {
        Vector2 lastPos = new Vector2(0, 0);
        for (int i = 0; i < SnakeBodies.Count; i++)
        {
            SnakeBodies[i].Position = lastPos;
            Vector2 newDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            newDir.Normalize();
            lastPos = new Vector2(lastPos.x + newDir.x * tileSize / 2, lastPos.y + newDir.y * tileSize / 2);
        }
        Render();
    }

    public void Render()
    {
        setMaterial();
        setMeshData();
        setRenderPass();
    }

    void setMaterial()
    {
        meshRend = GetComponent<MeshRenderer>();
        if (!meshRend)
        {
            meshRend = gameObject.AddComponent<MeshRenderer>();
        }

        if (!_mat)
        {
            Shader shader = Shader.Find("Transparent/SnakeRender");
            _mat = new Material(shader);
            meshRend.material = _mat;
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

        int descrCount = 6;
        NativeArray<VertexAttributeDescriptor> attr = new NativeArray<VertexAttributeDescriptor>(descrCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        attr[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
        attr[1] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.UInt32, 1);
        attr[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2);
        attr[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2);
        attr[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, 3);
        attr[5] = new VertexAttributeDescriptor(VertexAttribute.TexCoord3, VertexAttributeFormat.Float32, 3);

        int vertexPerSnake = 4;
        int vertexCount = vertexPerSnake * SnakeBodies.Count;
        mesh.SetVertexBufferParams(vertexCount, attr);
        attr.Dispose();

        NativeArray<SnakeVertex> vertex = new NativeArray<SnakeVertex>(vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        for (int i = 0; i < SnakeBodies.Count; i++)
        {
            float x = SnakeBodies[i].Position.x;
            float y = SnakeBodies[i].Position.y;
            float r = tileSize;

            float? prevXNorm = null;
            float? prevYNorm = null;
            float prevR = tileSize;
            if (i - 1 >= 0 && i - 1 < SnakeBodies.Count)
            {
                Vector2 delta = new Vector2((SnakeBodies[i - 1].Position.x - x) / tileSize, (SnakeBodies[i - 1].Position.y - y) / tileSize);
                prevXNorm = delta.x;
                prevYNorm = delta.y;
            }

            float? nextXNorm = null;
            float? nextYNorm = null;
            float nextR = tileSize;
            if (i + 1 >= 0 && i + 1 < SnakeBodies.Count)
            {
                Vector2 delta = new Vector2((SnakeBodies[i + 1].Position.x - x) / tileSize, (SnakeBodies[i + 1].Position.y - y) / tileSize);
                nextXNorm = delta.x;
                nextYNorm = delta.y;
            }

            if (prevXNorm == null || prevYNorm == null)
            {
                Vector2 norm = new Vector2(x - SnakeBodies[i + 1].Position.x, y - SnakeBodies[i + 1].Position.y);
                norm.Normalize();

                prevXNorm = norm.x * 0.1f;
                prevYNorm = norm.y * 0.1f;
            }
            else if (nextXNorm == null || nextYNorm == null)
            {
                Vector2 norm = new Vector2(x - SnakeBodies[i - 1].Position.x, y - SnakeBodies[i - 1].Position.y);
                norm.Normalize();

                nextXNorm = norm.x * 0.1f;
                nextYNorm = norm.y * 0.1f;
            }

            float snakeSize = tileSize;
            int padding = i * vertexPerSnake;

            vertex[padding] = new SnakeVertex
            {
                pos = new Vector3(x - snakeSize, y - snakeSize, r),
                bodyCount = (uint)SnakeBodies.Count - (uint)i,
                uv = new Vector2(0f, 0f),
                center = new Vector2(x, y),
                nextPos = new Vector3(nextXNorm ?? 0, nextYNorm ?? 0, nextR),
                prevPos = new Vector3(prevXNorm ?? 0, prevYNorm ?? 0, prevR),
            };
            vertex[padding + 1] = new SnakeVertex
            {
                pos = new Vector3(x - snakeSize, y + snakeSize, r),
                bodyCount = (uint)SnakeBodies.Count - (uint)i,
                uv = new Vector2(0f, 1f),
                center = new Vector2(x, y),
                nextPos = new Vector3(nextXNorm ?? 0, nextYNorm ?? 0, nextR),
                prevPos = new Vector3(prevXNorm ?? 0, prevYNorm ?? 0, prevR),
            };
            vertex[padding + 2] = new SnakeVertex
            {
                pos = new Vector3(x + snakeSize, y + snakeSize, r),
                bodyCount = (uint)SnakeBodies.Count - (uint)i,
                uv = new Vector2(1f, 1f),
                center = new Vector2(x, y),
                nextPos = new Vector3(nextXNorm ?? 0, nextYNorm ?? 0, nextR),
                prevPos = new Vector3(prevXNorm ?? 0, prevYNorm ?? 0, prevR),
            };
            vertex[padding + 3] = new SnakeVertex
            {
                pos = new Vector3(x + snakeSize, y - snakeSize, r),
                bodyCount = (uint)SnakeBodies.Count - (uint)i,
                uv = new Vector2(1f, 0f),
                center = new Vector2(x, y),
                nextPos = new Vector3(nextXNorm ?? 0, nextYNorm ?? 0, nextR),
                prevPos = new Vector3(prevXNorm ?? 0, prevYNorm ?? 0, prevR),
            };
        }

        mesh.SetVertexBufferData(vertex, 0, 0, vertexCount);
        vertex.Dispose();

        int indexPerSnake = 6;
        int indexCount = indexPerSnake * SnakeBodies.Count;
        mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

        NativeArray<int> indices = new NativeArray<int>(indexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        for (int i = 0; i < SnakeBodies.Count; i++)
        {
            int padding = i * indexPerSnake;
            int vertPad = i * vertexPerSnake;

            indices[padding] = vertPad;
            indices[padding + 1] = vertPad + 1;
            indices[padding + 2] = vertPad + 2;
            indices[padding + 3] = vertPad;
            indices[padding + 4] = vertPad + 2;
            indices[padding + 5] = vertPad + 3;
        }

        mesh.SetIndexBufferData(indices, 0, 0, indexCount);
        indices.Dispose();

        mesh.bounds = new Bounds
        {
            center = transform.localPosition,
            extents = new Vector3(350, 350)
        };
        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor
        {
            indexStart = 0,
            indexCount = indexCount,
            topology = MeshTopology.Triangles,
            baseVertex = 0,
        });
    }

    void setRenderPass()
    {
        if (!mesh || !meshRend || cmdBuffer == null) return;

        cmdBuffer.Clear();
        var lookMatrix = Camera.main.worldToCameraMatrix;
        var orthoMatrix = Matrix4x4.Ortho(-RendTex.width / 2, RendTex.width / 2, -RendTex.height / 2, RendTex.height / 2, 0.3f, 1000f);
        cmdBuffer.SetViewProjectionMatrices(lookMatrix, orthoMatrix);

        if ((snakePass & (int)SnakeRenderPass.Body) != 0)
        {
            int bodyID = Shader.PropertyToID("_BodyTexture");

            cmdBuffer.GetTemporaryRT(bodyID, RendTex.width, RendTex.height, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
            cmdBuffer.SetRenderTarget(RendTex);
            cmdBuffer.ClearRenderTarget(true, true, Color.clear, 1f);
            cmdBuffer.DrawMesh(mesh, Matrix4x4.identity, _mat, 0, 0);
            cmdBuffer.Blit(RendTex, bodyID);
        }

        if ((snakePass & (int)SnakeRenderPass.TextureSampling) != 0)
        {
            int textureSamp = Shader.PropertyToID("_Temp1");

            cmdBuffer.GetTemporaryRT(textureSamp, RendTex.width, RendTex.height, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
            cmdBuffer.SetRenderTarget(RendTex);
            cmdBuffer.DrawMesh(mesh, Matrix4x4.identity, _mat, 0, 1);
            cmdBuffer.Blit(textureSamp, RendTex);
        }

        Graphics.ExecuteCommandBuffer(cmdBuffer);
    }

    public void SetMatByType()
    {

    }

    public void SetSnakeSkin()
    {

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

#if !UNITY_EDITOR
void OnDestroy()
  {
    destroyMat();
  }
#endif
}
