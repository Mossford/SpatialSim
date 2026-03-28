namespace SpatialSim.Engine.Rendering
{
    public struct TextureData
    {
        public uint width;
        public uint height;
        public TextureFormat format;
        public TextureMemoryUsage memoryUsage;
        public TextureUsage usage;
        public TextureFilter filter;
        public int binding;
        public byte[] data;
    }
}