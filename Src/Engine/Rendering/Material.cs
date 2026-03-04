using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class Material : IComponent
    {
        public EcsComponentType type => EcsComponentType.Material;
        public int id { get; set; } = -1;

        public int materialId;

        public void Dispose()
        {
            
        }
    }
}