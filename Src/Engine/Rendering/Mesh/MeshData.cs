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
    }
}