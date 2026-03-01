namespace SpatialSim.Engine.Core
{
    public interface IComponent
    {
        public EcsComponentType type { get; }
        public int id { get; set; }
    }
}