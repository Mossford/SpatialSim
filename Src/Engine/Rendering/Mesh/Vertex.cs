using System.Numerics;

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
}