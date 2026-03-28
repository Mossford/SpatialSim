namespace SpatialSim.Engine.Core
{
    public interface IComponent : IDisposable
    {
        public int type { get; }
        public int id { get; set; }
    }
}