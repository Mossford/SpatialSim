namespace SpatialSim.Engine.Rendering
{
    public interface ITextureDevice
    {
        public void Create(in TextureData data);
        public void Clean();
    }
}