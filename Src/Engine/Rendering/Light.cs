using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class Light : IComponent
    {
        public int type => EcsComponentType.Light.GetId();
        public int id { get; set; } = -1;
        
        
        public void Dispose()
        {
            
        }
    }
}