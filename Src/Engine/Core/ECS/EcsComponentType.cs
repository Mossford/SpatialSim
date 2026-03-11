namespace SpatialSim.Engine.Core
{
    public enum EcsComponentType
    {
        Pipeline,
        Mesh,
        MeshRenderer,
        Material,
        Transform,
        Camera,
        
        //always keep last
        Empty,
    }

    public static class EcsComponentTypeExtensions
    {
        public static int GetId(this EcsComponentType type)
        {
            return (int)type;
        }
    }
}