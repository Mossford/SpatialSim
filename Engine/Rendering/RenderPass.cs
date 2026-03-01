namespace SpatialSim.Engine.Rendering
{
    public abstract class RenderPass : IDisposable
    {
        public abstract void Create();
        public abstract void Clean();
        
        public void Dispose()
        {
            Clean();
        }
    }
}   