namespace SpatialSim.Engine.Rendering
{
    public interface ITextureDevice
    {
        public void Create(in TextureData data);
        public void WriteGpuToCpu(ref TextureData data);
        public void CopyImageToImage(in ITextureDevice src, in TextureData srcData);
        public void Clean();
    }
}