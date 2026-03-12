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

        public void BeginCommandBuffer()
        {
            commandBuffer?.BeginCommandBuffer();
        }

        public void EndCommandBuffer()
        {
            commandBuffer?.EndCommandBuffer();
        }

        public void SubmitCommandBuffer()
        {
            commandBuffer?.SubmitCommandBuffer();
        }

        public void BeginRenderPass(int frame)
        {
            commandBuffer?.BeginRenderPass(frame);
        }

        public void BindPipeLine(Pipeline pipeline)
        {
            commandBuffer?.BindPipeLine(pipeline);
        }

        public void BindUniforms(Pipeline pipeline)
        {
            commandBuffer?.BindUniforms(pipeline);
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

        public void EndRenderPass()
        {
            commandBuffer?.EndRenderPass();
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