namespace SpatialSim.Engine.Core
{
    public class Entity
    {
        public int id;
        public StableList<EcsComponentRef> componentRefs;

        public Entity(int id)
        {
            this.id = id;
            componentRefs = new StableList<EcsComponentRef>();
        }

        public EcsComponentRef AddComponent(in IComponent component)
        {
            int refId = componentRefs.PeekNextId();
            componentRefs.Add(EcsManager.AddComponent(component, id) with
            {
                id = refId
            });
            
            return componentRefs[refId];
        }

        public void RemoveComponent(in EcsComponentRef componentRef)
        {
            if (EcsManager.RemoveComponent(componentRef))
            {
                componentRefs.RemoveAt(componentRef.id);
            }
        }

        public IComponent GetComponent(in EcsComponentRef componentRef)
        {
            return EcsManager.GetComponent(componentRef);
        }

        public IComponent GetFirstComponentOfType(EcsComponentType type)
        {
            for (int i = 0; i < componentRefs.ValueCount; i++)
            {
                if (componentRefs.Get(i).type == type)
                    return EcsManager.GetComponent(componentRefs[i]);
            }

            return new EmptyComponent();
        }
    }
}