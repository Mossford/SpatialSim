namespace SpatialSim.Engine.Rendering
{
    public struct TextureInfo
    {
        public uint width;
        public uint height;
        public uint depth;
        public TextureFormat format;
        public TextureMemoryUsage memoryUsage;
        public TextureUsage usage;
        public TextureFilter filter;
        public TextureType type;

        public TextureInfo()
        {
            width = 0;
            height = 0;
            depth = 0;
            format = default;
            memoryUsage = default;
            usage = default;
            filter = default;
            type = default;
        }
    }
    
    public struct TextureData
    {
        public TextureInfo info;
        public byte[] data;

        public TextureData()
        {
            info = new TextureInfo();
            data = new byte[0];
        }
    }
}