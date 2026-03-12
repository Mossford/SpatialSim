namespace SpatialSim.Engine.Rendering
{
    // TODO make this an interface instead of abstract
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