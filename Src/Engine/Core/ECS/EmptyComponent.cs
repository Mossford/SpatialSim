namespace SpatialSim.Engine.Core
{
    public class EmptyComponent : IComponent
    {
        public int type => EcsComponentType.Empty.GetId();
        public int id { get; set; } = -1;

        public void Dispose()
        {
            
        }
    }
}