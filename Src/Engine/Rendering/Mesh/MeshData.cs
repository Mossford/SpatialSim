using System.Numerics;

namespace SpatialSim.Engine.Rendering
{
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
        
        public Vertex[] GetVertexes()
        {
            Vertex[] vertexes = new Vertex[vertexData.vertices.Length];
            
            for (int i = 0; i < vertexes.Length; i++)
            {
                vertexes[i] = new Vertex(vertexData.vertices[i], vertexData.normals[i], vertexData.uvs[i]);
            }

            return vertexes;
        }
    }
}