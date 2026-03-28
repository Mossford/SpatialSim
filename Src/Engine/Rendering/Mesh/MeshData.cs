using System.Numerics;

namespace SpatialSim.Engine.Rendering
{
    public struct MeshData
    {
        public struct VertexData
        {
            public Vector3[] vertices;
            public Vector3[] normals;
            public Vector3[] tangents;
            public Vector3[] biTangents;
            public Vector2[] uvs;

            public VertexData()
            {
                vertices = new Vector3[0];
                normals = new Vector3[0];
                tangents = new Vector3[0];
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
        
        public Vertex[] GetVertexes()
        {
            Vertex[] vertexes = new Vertex[vertexData.vertices.Length];
            
            for (int i = 0; i < vertexes.Length; i++)
            {
                vertexes[i] = new Vertex(
                    vertexData.vertices[i], 
                    vertexData.normals[i], 
                    vertexData.tangents[i],
                    vertexData.biTangents[i],
                    vertexData.uvs[i]);
            }

            return vertexes;
        }
    }
}