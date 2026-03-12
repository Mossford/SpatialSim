using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class Mesh : IComponent
    {
        public EcsComponentType type => EcsComponentType.Mesh;
        public int id { get; set; } = -1;

        public EcsComponentRef transform;
        public MeshData meshData;
        
        public Mesh()
        {
            
        }

        public Mesh(MeshData meshData, EcsComponentRef transform)
        {
            transform.CheckComponent(EcsComponentType.Transform);
            this.meshData = meshData;
            this.transform = transform;
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