namespace SpatialSim.Engine.Core
{
    public enum EcsComponentType
    {
        Mesh,
        MeshRenderer,
        Material,
        Transform,
        Camera,
        Light,
        
        //always keep last
        Empty = EcsManager.MaxEngineComponentType
    }

    public static class EcsComponentTypeExtensions
    {
        public static int GetId(this EcsComponentType type)
        {
            return (int)type;
        }
    }
}