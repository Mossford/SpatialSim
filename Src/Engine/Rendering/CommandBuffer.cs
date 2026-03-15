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

        public void BeginRenderPass(int frame)
        {
            commandBuffer?.BeginRenderPass(frame);
        }

        public void BindPipeLine(Pipeline pipeline)
        {
            commandBuffer?.BindPipeLine(pipeline);
        }

        public void BindVertexUniforms(Pipeline pipeline, int binding)
        {
            commandBuffer?.BindVertexUniforms(pipeline, binding);
        }

        public void BindFragmentUniforms(Pipeline pipeline, int binding)
        {
            commandBuffer?.BindFragmentUniforms(pipeline, binding);
        }

        public void BindTexture(Pipeline pipeline, Texture texture)
        {
            commandBuffer?.BindTexture(pipeline, texture);
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