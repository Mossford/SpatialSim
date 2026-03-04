namespace SpatialSim.Engine.Core
{
    public class EmptyComponent : IComponent
    {
        public EcsComponentType type => EcsComponentType.Empty;
        public int id { get; set; } = -1;

        public void Dispose()
        {
            
        }
    }
}