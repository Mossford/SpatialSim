namespace SpatialSim.Engine.Core
{
    /// <summary>
    /// Stored by entities and references a component stored by the Ecs Manager
    /// </summary>
    public struct EcsComponentRef
    {
        /// <summary>
        /// Will index into which EcsComponent based on the type
        /// </summary>
        public EcsComponentType type;
        /// <summary>
        /// Index for the component from the Ecs Component, component list. This should always be stable
        /// </summary>
        public int componentId;
        /// <summary>
        /// The id at which entity this component is attached to
        /// </summary>
        public int entity;
        /// <summary>
        /// Id for the componentRef List in a entity
        /// </summary>
        public int id;
    }
}