using System.Numerics;

namespace SpatialSim.Engine.Rendering
{
    public struct Vertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector3 tangent;
        public Vector3 biTangent;
        public Vector2 uv;

        public Vertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector3 biTangent, Vector2 uv)
        {
            this.position = position;
            this.normal = normal;
            this.tangent = tangent;
            this.biTangent = biTangent;
            this.uv = uv;
        }
    }
}