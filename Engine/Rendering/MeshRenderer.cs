using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class MeshRenderer : IComponent
    {
        public EcsComponentType type => EcsComponentType.MeshRenderer;
        public int id { get; set; } = -1;

        public EcsComponentRef mesh;
        public EcsComponentRef material;

        public MeshRenderer()
        {
            
        }
    }
}