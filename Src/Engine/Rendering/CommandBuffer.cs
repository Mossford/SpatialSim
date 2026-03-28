using System.Numerics;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public sealed class CommandBuffer : IDisposable
    {
        public ICommandBufferDevice? commandBuffer;
        
        public void Create()
        {
            commandBuffer = AppState.appContext.DeviceFactory.CreateCommandBufferDevice();
            Debug.LogDebug("Successful command buffer creation");
        }

        public void Clean()
        {
            commandBuffer?.Clean();   
            Debug.LogDebug("Cleaned up Commandbuffer");
        }

        public void Begin()
        {
            commandBuffer?.Begin();
        }
        
        public void BeginOneUse()
        {
            commandBuffer?.BeginOneUse();
        }

        public void End()
        {
            commandBuffer?.EndCommandBuffer();
        }

        public void Submit()
        {
            commandBuffer?.Submit();
        }

        public void BindPipeLine(Pipeline pipeline)
        {
            commandBuffer?.BindPipeLine(pipeline);
        }

        public void SetViewport(Vector2 size)
        {
            commandBuffer?.SetViewport(size);
        }
        
        public void SetScissor(Vector2 size)
        {
            commandBuffer?.SetScissor(size);
        }
        
        public void BeginRendering(int frame)
        {
            commandBuffer?.BeingRendering(frame);
        }

        public void EndRendering()
        {
            commandBuffer?.EndRendering();
        }
        
        public void BindVertexUniforms(Pipeline pipeline, int binding)
        {
            commandBuffer?.BindVertexUniforms(pipeline, binding);
        }

        public void BindFragmentUniforms(Pipeline pipeline, int binding)
        {
            commandBuffer?.BindFragmentUniforms(pipeline, binding);
        }

        public void BindSamplers(Pipeline pipeline, Texture[] textures, ShaderType shaderType)
        {
            commandBuffer?.BindSamplers(pipeline, textures, shaderType);
        }

        /// <summary>
        /// Should only be used with type Vertex
        /// </summary>
        public void BindVertexBuffers<T>(IBufferDevice<T> bufferDevice) where T : unmanaged
        {
            commandBuffer?.BindVertexBuffers(bufferDevice);   
        }
        
        public void BindIndexBuffers<T>(IBufferDevice<T> bufferDevice) where T : unmanaged
        {
            commandBuffer?.BindIndexBuffers(bufferDevice);   
        }

        public void Draw(int indexCount)
        {
            commandBuffer?.Draw(indexCount);
        }

        public void ResetPipeLine(Pipeline pipeline)
        {
            commandBuffer?.ResetPipeLine(pipeline);
        }
        
        public void Dispose()
        {
            Clean();
        }
    }
}