using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public interface ICommandBufferDevice
    {
        public void Create();
        public void Clean();

        public void BindVertexBuffers<T>(IBufferDevice<T> bufferDevice) where T : unmanaged;
        public void BeginCommandBuffer();
        public void EndCommandBuffer();
        public void BeginRenderPass();
        public void EndRenderPass();
    }

    public sealed class CommandBuffer : IDisposable
    {
        public ICommandBufferDevice? commandBuffer;
        
        public void Create()
        {
            commandBuffer = AppState.appContext.renderer.CreateCommandBufferDevice();
        }

        public void Clean()
        {
            commandBuffer?.Clean();   
        }

        public void BeginCommandBuffer()
        {
            commandBuffer?.BeginCommandBuffer();
        }

        public void EndCommandBuffer()
        {
            commandBuffer?.EndCommandBuffer();
        }

        public void BeginRenderPass()
        {
            commandBuffer?.BeginRenderPass();
        }

        public void BindPipeLine()
        {
            
        }

        public void BindVertexBuffers<T>(IBufferDevice<T> bufferDevice) where T : unmanaged
        {
            commandBuffer?.BindVertexBuffers(bufferDevice);   
        }

        public void EndRenderPass()
        {
            commandBuffer?.EndRenderPass();
        }
        
        public void Dispose()
        {
            Clean();
        }
    }
}