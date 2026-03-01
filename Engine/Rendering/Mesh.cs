using System.Numerics;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public struct MeshData
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector2[] uvs;
        public int[] indices;

        public MeshData()
        {
            vertices = new Vector3[0];
            normals = new Vector3[0];
            uvs = new Vector2[0];
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
    }
}