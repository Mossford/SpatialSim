using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class Mesh : IComponent
    {
        public EcsComponentType type => EcsComponentType.Mesh;
        public int id { get; set; } = -1;

        public EcsComponentRef transform
        {
            get;
            set
            {
                //make sure we update the transform reference if we update it
                field = value;
                transformRef = EcsManager.GetComponent<Transform>(transform);
            }
        }
        public MeshData meshData;
        
        public Transform transformRef { get; private set; }
        
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
            return meshData.GetVertexes();
        }

        public void Dispose()
        {
            
        }
    }
}