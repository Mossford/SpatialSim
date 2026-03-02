using System.Numerics;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public struct Vertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;

        public Vertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
        }
    }

    public struct MeshData
    {
        public struct VertexData
        {
            public Vector3[] vertices;
            public Vector3[] normals; //offsetted by whole length of vertices
            public Vector2[] uvs; //offseteed by whole length of vertices and normals

            public VertexData()
            {
                vertices = new Vector3[0];
                normals = new Vector3[0];
                uvs = new Vector2[0];
            }
        }

        public VertexData vertexData;
        public int[] indices;

        public MeshData()
        {
            vertexData = new VertexData();
            indices = new int[0];
        }
    }

    public class Mesh : IComponent
    {
        public EcsComponentType type => EcsComponentType.Mesh;
        public int id { get; set; } = -1;

        public MeshData meshData;
        public Matrix4x4 modelMat;
        
        public Mesh()
        {
            
        }

        public Mesh(MeshData meshData)
        {
            this.meshData = meshData;
        }

        public void CreateModelMatrix()
        {
            
        }

        public Vertex[] GetVertexes()
        {
            Vertex[] vertexes = new Vertex[meshData.vertexData.vertices.Length];

            for (int i = 0; i < vertexes.Length; i++)
            {
                vertexes[i] = new Vertex(meshData.vertexData.vertices[i], meshData.vertexData.normals[i], meshData.vertexData.uvs[i]);
            }

            return vertexes;
        }

        public void Dispose()
        {
            
        }
    }
}