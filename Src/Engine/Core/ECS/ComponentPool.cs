using SpatialSim.Engine.Rendering;

namespace SpatialSim.Engine.Core
{
    /// <summary>
    /// Will store a pool of a components
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ComponentPool
    {
        public StableList<IComponent> components;
        public EcsComponentType poolType;

        public ComponentPool()
        {
            components = new StableList<IComponent>();
            poolType = EcsComponentType.Empty;
        }
    }
}