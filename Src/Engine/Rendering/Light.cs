using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public class Light : IComponent
    {
        public int type => EcsComponentType.Light.GetId();
        public int id { get; set; } = -1;
        
        public EcsComponentRef transform
        {
            get;
            set
            {
                //make sure we update the transform reference if we update it
                field = value;
                transformRef = EcsManager.GetComponent<Transform>(transform);
            }
        }

        public Transform transformRef { get; private set; }
        
        
        
        public void Dispose()
        {
            
        }
    }
}