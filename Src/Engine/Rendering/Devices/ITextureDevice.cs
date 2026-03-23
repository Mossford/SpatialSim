namespace SpatialSim.Engine.Rendering
{
    public interface ITextureDevice
    {
        public void Create(in TextureData data, string pipeline);
        public void Clean();
    }
}