

using SpatialSim.Engine.Core;

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

        public IPipelineDevice CreatePipelineDevice(in Shader[] shaders, in PipelineSettings settings)
        {
            VkPipeline pipeline = new VkPipeline();
            switch (settings.type)
            {
                case PipelineType.Raster:
                {
                    if (shaders.Length != 2)
                    {
                        Debug.Error($"Tried to create {settings.type} pipeline with insufficient amount of shaders {shaders.Length}, returning empty pipeline");
                        return pipeline;
                    }
                    pipeline.CreateRaster(shaders[0], shaders[1], settings);
                    break;
                }
                case PipelineType.Compute:
                {
                    if (shaders.Length != 1)
                    {
                        Debug.Error($"Tried to create {settings.type} pipeline with insufficient amount of shaders {shaders.Length}, returning empty pipeline");
                        return pipeline;
                    }
                    pipeline.CreateCompute(shaders[0], settings);
                    break;
                }
            }
            return pipeline;
        }

        public ICommandBufferDevice CreateCommandBufferDevice()
        {
            VkCommandBuffer commandBuffer = new VkCommandBuffer();
            commandBuffer.Create();
            return commandBuffer;
        }

        public IBufferDevice<T> CreateBufferDevice<T>(in Span<T> data, BufferUsage usage, BufferMemoryUsage memoryUsage) where T : unmanaged
        {
            VkBuffer<T> buffer = new VkBuffer<T>();
            buffer.Create(data, usage, memoryUsage);
            return buffer;
        }
        
        public IBufferDevice<T> CreateBufferDevice<T>(uint dataLength, BufferUsage usage, BufferMemoryUsage memoryUsage) where T : unmanaged
        {
            VkBuffer<T> buffer = new VkBuffer<T>();
            buffer.Create(dataLength, usage, memoryUsage);
            return buffer;
        }

        public ITextureDevice CreateTextureDevice(in TextureData data)
        {
            VkTexture texture = new VkTexture();
            texture.Create(data);
            return texture;
        }

        public IPostProcessDevice CreatePostProcessDevice(ref Texture texture)
        {
            VkPostProcess postProcess = new VkPostProcess();
            postProcess.Create(ref texture);
            return postProcess;
        }
    }
}