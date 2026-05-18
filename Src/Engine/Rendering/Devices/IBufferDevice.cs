namespace SpatialSim.Engine.Rendering
{
    public interface IBufferDevice<T> where T : unmanaged
    {
        public void Create(in Span<T> data, BufferUsage usage, BufferMemoryUsage memoryUsage);
        public void Create(uint dataLength, BufferUsage usage, BufferMemoryUsage memoryUsage);
        public void BindVertexBuffer(ICommandBufferDevice commandBufferDevice);
        public void BindBuffer(ICommandBufferDevice commandBufferDevice);
        public void CopyTo(IBufferDevice<T> dest);
        public void CopyToTexture(ITextureDevice dest, in TextureData srcData);
        public void TextureToCopy(ITextureDevice src, in TextureData destData);
        public T[] CopyToArray();
        public void UpdateData(in Span<T> data);
        public void UpdateUniformData(in Span<T> data);
        public void Clean();
    }
}