using SpatialSim.Engine.Rendering;
using SpatialSim.Engine.Rendering.Vulkan;

namespace SpatialSim.Engine.Core
{
    /// <summary>
    /// This works off the case where we have lists of each component (Mesh, Shader, Texture, Mesh Renderer) This is the Ecs Component
    /// We then have an identifier that points to that component with stable indexes 
    /// This identifier should then live in 
    /// </summary>
    public static class EcsManager
    {
        public static List<ComponentPool> componentPools;
        public static StableList<Entity> entities;

        public static int totalComponents;
        
        public static void Init()
        {
            componentPools = new List<ComponentPool>();
            entities = new StableList<Entity>();

            EcsComponentType[] types = (EcsComponentType[])Enum.GetValuesAsUnderlyingType<EcsComponentType>();
            //remove the empty component type
            for (int i = 0; i < types.Length - 1; i++)
            {
                Debug.LogInfo($"Created {types[i]} ECS component pool");
                componentPools.Add(new ComponentPool() with
                {
                    poolType = types[i]
                });
            }
            
            Debug.LogInfo($"Set up {componentPools.Count} ECS component pools");
        }

        public static void Update()
        {
            
        }

        public static void Render(CommandBuffer commandBuffer)
        {
            for (int i = 0; i < componentPools[EcsComponentType.MeshRenderer.GetId()].components.ValueCount; i++)
            {
                MeshRenderer renderer = (MeshRenderer)componentPools[EcsComponentType.MeshRenderer.GetId()].components.Get(i);
                renderer.Draw(commandBuffer);
            }
        }

        public static void Clean()
        {
            for (int i = 0; i < componentPools.Count; i++)
            {
                for (int j = 0; j < componentPools[i].components.ValueCount; j++)
                {
                    componentPools[i].components.Get(j).Dispose();
                }
            }
            
            Debug.LogInfo("Cleaned all ECS and entity instances");
        }

        public static Entity AddEntity()
        {
            int id = entities.Add(new Entity(entities.PeekNextId()));
            return entities[id];
        }

        /// <summary>
        /// Gets an entity at the specified id
        /// </summary>
        /// <returns>Will be null if no entity exists at the id</returns>
        public static Entity? GetEntity(int id)
        {
            if (id < 0 || id > entities.Count)
                return null;
            
            return entities[id];
        }

        public static void RemoveEntity(int id)
        {
            entities.RemoveAt(id);
        }

        public static EcsComponentRef AddComponent(in IComponent component, int entityId)
        {
            int poolId = component.type.GetId();
            int id = componentPools[poolId].components.Add(component);
            componentPools[poolId].components[id].id = id;
            Debug.LogInfo($"Added component of type {component.type}");
            
            totalComponents++;
            return new EcsComponentRef() with
            {
                type = component.type,
                id = id,
                entity = entityId
            };
        }

        public static bool RemoveComponent(in EcsComponentRef componentRef)
        {
            int poolId = componentRef.type.GetId();
            int id = componentRef.id;
            if (id < 0 || id >= componentPools[poolId].components.Count)
                return false;
            
            componentPools[poolId].components.RemoveAt(id);

            totalComponents--;

            return true;
        }
        
        public static IComponent GetComponent(in EcsComponentRef componentRef)
        {
            if (componentRef.type == EcsComponentType.Empty)
                return new EmptyComponent();
            
            int poolId = componentRef.type.GetId();
            int id = componentRef.id;

            if (id < 0 || id >= componentPools[poolId].components.Count)
                return new EmptyComponent();

            return componentPools[poolId].components[id];
        }
    }
}