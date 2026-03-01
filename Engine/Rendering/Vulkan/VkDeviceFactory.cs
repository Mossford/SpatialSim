

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public class VkDeviceFactory : IDeviceFactory
    {
        public IShaderDevice CreateShaderDevice(ShaderSettings settings, in byte[] code)
        {
            VkShader shader = new VkShader();
            shader.Create(settings, in code);
            return shader;
        }

        public IPipelineDevice CreatePipelineDevice(in Shader vertex, in Shader fragment)
        {
            VkPipeline pipeline = new VkPipeline();
            pipeline.Create(vertex, fragment);
            return pipeline;
        }

        public ICommandBufferDevice CreateCommandBufferDevice()
        {
            VkCommandBuffer commandBuffer = new VkCommandBuffer();
            commandBuffer.CreateCommandPool();
            commandBuffer.Create();
            return commandBuffer;
        }

        public IBufferDevice<T> CreateBufferDevice<T>(in Span<T> data, BufferUsage usage, BufferMemoryUsage memoryUsage) where T : unmanaged
        {
            VkBuffer<T> buffer = new VkBuffer<T>();
            buffer.Create(data, usage, memoryUsage);
            return buffer;
        }
        
        public IBufferDevice<T> CreateBufferDevice<T>(int dataLength, BufferUsage usage, BufferMemoryUsage memoryUsage) where T : unmanaged
        {
            VkBuffer<T> buffer = new VkBuffer<T>();
            buffer.Create(dataLength, usage, memoryUsage);
            return buffer;
        }

        public void DrawMesh(in CommandBuffer commandBuffer, in Pipeline pipeline, Buffer<Vertex> vertexBuffer, Buffer<int> indexBuffer)
        {
            
        }
    }
}