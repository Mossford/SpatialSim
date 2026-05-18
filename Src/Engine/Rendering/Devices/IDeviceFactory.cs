
namespace SpatialSim.Engine.Rendering
{
    /// <summary>
    /// Methods to perform api agnostic functions on shit like shaders and whatever the hell
    /// </summary>
    public interface IDeviceFactory
    {
        public IShaderDevice CreateShaderDevice(ShaderSettings settings, in byte[] code);
        public IPipelineDevice CreatePipelineDevice(in Shader vertex, in Shader fragment, in PipelineSettings settings);
        public ICommandBufferDevice CreateCommandBufferDevice();
        public IBufferDevice<T> CreateBufferDevice<T>(in Span<T> data, BufferUsage usage, BufferMemoryUsage memoryUsage) where T : unmanaged;
        public IBufferDevice<T> CreateBufferDevice<T>(uint dataLength, BufferUsage usage, BufferMemoryUsage memoryUsage) where T : unmanaged;
        public ITextureDevice CreateTextureDevice(in TextureData data);
        public IPostProcessDevice CreatePostProcessDevice(ref Texture texture);
    }
}