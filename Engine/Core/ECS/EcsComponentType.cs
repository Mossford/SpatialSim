namespace SpatialSim.Engine.Core
{
    public enum EcsComponentType
    {
        Empty,
        Pipeline,
        Mesh,
        MeshRenderer,
        Material
    }

    public static class EcsComponentTypeExtensions
    {
        public static int GetId(this EcsComponentType type)
        {
            return (int)type - 1;
        }
    }
}