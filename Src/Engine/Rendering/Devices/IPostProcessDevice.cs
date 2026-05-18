namespace SpatialSim.Engine.Rendering
{
    public interface IPostProcessDevice
    {
        public void Create(ref Texture texture);
        public void EnableWrite(CommandBuffer commandBuffer);
        public void EnableRead(CommandBuffer commandBuffer);
    }
}