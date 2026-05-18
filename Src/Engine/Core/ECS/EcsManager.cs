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

        //Engine defined components have a maximum of 99, user defined is above
        public const int MaxEngineComponentType = 99;
        
        public static int totalComponents;
        
        public static void Init()
        {
            componentPools = new List<ComponentPool>();
            entities = new StableList<Entity>();

            EcsComponentType[] types = (EcsComponentType[])Enum.GetValuesAsUnderlyingType<EcsComponentType>();
            //remove the empty component type
            for (int i = 0; i < types.Length - 1; i++)
            {
                Debug.LogDebug($"Created {types[i]} ECS component pool");
                RegisterComponentType(types[i].GetId());
            }
            
            EcsRendererManager.Init();
            
            Debug.LogDebug($"Set up {componentPools.Count} ECS component pools");
        }

        public static void Update()
        {
            for (int i = 0; i < componentPools[EcsComponentType.Camera.GetId()].components.ValueCount; i++)
            {
                Camera camera = (Camera)componentPools[EcsComponentType.Camera.GetId()].components.Get(i);
                camera.GenerateTransforms();
            }
            
            EcsRendererManager.UpdateOrder();
        }

        /// <summary>
        /// Render to swapchain
        /// TODO Remove once swapchain gets textures instead
        /// </summary>
        public static void Render(CommandBuffer commandBuffer, int frame)
        {
            EcsRendererManager.Render(commandBuffer, frame);
        }
        
        /// <summary>
        /// Render to a texture
        /// </summary>
        public static void Render(CommandBuffer commandBuffer, Texture texture)
        {
            EcsRendererManager.Render(commandBuffer, texture);
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
            int poolId = component.type;
            
            if (!CheckComponentTypeBounds(poolId))
                return new EcsComponentRef();
            
            int id = componentPools[poolId].components.Add(component);
            componentPools[poolId].components[id].id = id;
            Debug.LogDebug($"Added component of type {component.type} at {id} to Entity [{entityId}]");
            
            totalComponents++;
            return new EcsComponentRef() with
            {
                type = component.type,
                id = id,
                entity = entityId
            };
        }
        
        public static EcsComponentRef AddComponentThr(in IComponent component, int entityId)
        {
            int poolId = component.type;
            
            lock (componentPools)
            {
                if (!CheckComponentTypeBounds(poolId))
                    return new EcsComponentRef();
                
                lock (componentPools[poolId].components)
                {
                    int id = componentPools[poolId].components.Add(component);
                    componentPools[poolId].components[id].id = id;
                    Debug.LogDebug($"Added component of type {component.type} at {id} to Entity [{entityId}]");

                    totalComponents++;
                    return new EcsComponentRef() with
                    {
                        type = component.type,
                        id = id,
                        entity = entityId
                    };
                }
            }
        }

        public static bool RemoveComponent(in EcsComponentRef componentRef)
        {
            int poolId = componentRef.type;
            if (!CheckComponentTypeBounds(poolId))
                return false;
            
            int id = componentRef.id;
            if (id < 0 || id >= componentPools[poolId].components.Count)
                return false;
            
            componentPools[poolId].components.RemoveAt(id);

            totalComponents--;

            return true;
        }
        
        public static bool RemoveComponentThr(in EcsComponentRef componentRef)
        {
            int poolId = componentRef.type;
            
            lock (componentPools)
            {
                if (!CheckComponentTypeBounds(poolId))
                    return false;
                
                lock (componentPools[poolId].components)
                {
                    int id = componentRef.id;
                    if (id < 0 || id >= componentPools[poolId].components.Count)
                        return false;

                    componentPools[poolId].components.RemoveAt(id);
                }
            }

            totalComponents--;

            return true;
        }
        
        public static IComponent GetComponent(in EcsComponentRef componentRef)
        {
            if (componentRef.type == EcsComponentType.Empty.GetId())
            {
                Debug.Warning("Get Component type was empty, returning an empty component");
                return new EmptyComponent();
            }
            
            int poolId = componentRef.type;
            if (!CheckComponentTypeBounds(poolId))
                return new EmptyComponent();
            
            int id = componentRef.id;

            if (id < 0 || id >= componentPools[poolId].components.Count)
            {
                int range = id < 0 ? 0 : componentPools[poolId].components.Count;
                Debug.Warning($"Get Component id:{id} was not in range [0,{range - 1}], returning an empty component");
                return new EmptyComponent();
            }

            return componentPools[poolId].components[id];
        }
        
        public static T GetComponent<T>(in EcsComponentRef componentRef) where T : IComponent
        {
            if (componentRef.type == EcsComponentType.Empty.GetId())
            {
                Debug.Error("Get Component type was empty on auto cast");
                throw new InvalidCastException("Get Component type was empty on auto cast");
            }
            
            int poolId = componentRef.type;
            if (!CheckComponentTypeBounds(poolId))
                throw new InvalidCastException();
            
            int id = componentRef.id;

            if (id < 0 || id >= componentPools[poolId].components.Count)
            {
                int range = id < 0 ? 0 : componentPools[poolId].components.Count;
                Debug.Error($"Get Component id:{id} was not in range [0,{range - 1}]");
                throw new IndexOutOfRangeException($"Get Component id:{id} was not in range [0,{range - 1}]");
            }

            return (T)componentPools[poolId].components[id];
        }

        public static void RegisterComponentType(int type)
        {
            if (type < componentPools.Count)
            {
                Debug.Error($"Tried to register component type {type} that is already registered, skipping");
                return;
            }
            
            componentPools.Add(new ComponentPool() with
            {
                poolType = type
            });
        }

        static bool CheckComponentTypeBounds(int type)
        {
            //TODO maybe only run on debug builds?
            if (type >= componentPools.Count)
            {
                Debug.Error($"Tried to access component of type {type} that is not registered");
                return false;
            }

            return true;
        }

        public static IComponent GetFirstComponentOfType(Entity entity, int type)
        {
            if (CheckComponentTypeBounds(type))
            {
                return new EmptyComponent();
            }
            
            for (int i = 0; i < entity.componentRefs.ValueCount; i++)
            {
                if (entity.componentRefs.Get(i).type == type)
                    return GetComponent(entity.componentRefs[i]);
            }

            Debug.Warning($"Get First Component type {type} did not match any stored type, returning empty component");
            return new EmptyComponent();
        }
        
        public static T GetFirstComponent<T>(Entity entity, int type) where T : IComponent
        {
            if (CheckComponentTypeBounds(type))
            {
                throw new InvalidCastException();
            }
            
            for (int i = 0; i < entity.componentRefs.ValueCount; i++)
            {
                if (entity.componentRefs.Get(i).type == type)
                    return (T)GetComponent(entity.componentRefs[i]);
            }

            Debug.Error($"Get First Component type {type} did not match any stored type");
            throw new InvalidCastException($"Get First Component type {type} did not match any stored type");
        }
    }
}