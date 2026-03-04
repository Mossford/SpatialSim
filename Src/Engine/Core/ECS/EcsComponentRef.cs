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
        /// The id at which entity this component is attached to
        /// </summary>
        public int entity;
        /// <summary>
        /// Id for the component for the componentpool
        /// </summary>
        public int id;
        /// <summary>
        /// Id for the componentRef List in a entity
        /// </summary>
        public int refId;

        public bool CheckComponent(EcsComponentType otherType)
        {
            if (type != otherType)
            {
                Debug.Warning($"Input {type} component was not of type {otherType}");
                return false;
            }
            
            if (id == -1 || id >= EcsManager.componentPools[type.GetId()].components.Count)
            {
                Debug.Warning($"Input {type} Id:{id} component does not have a valid id");
                return false;
            }

            return true;
        }
    }
}