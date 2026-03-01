namespace SpatialSim.Engine.Core
{
    public interface IComponent : IDisposable
    {
        public EcsComponentType type { get; }
        public int id { get; set; }
    }
}