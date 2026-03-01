using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
    public interface IBufferDevice<T> where T : unmanaged
    {
        public void Create(in Span<T> data, BufferUsage usage, BufferMemoryUsage memoryUsage);
        public void Create(int dataLength, BufferUsage usage, BufferMemoryUsage memoryUsage);
        public void BindVertexBuffer(ICommandBufferDevice commandBufferDevice);
        public void BindBuffer(ICommandBufferDevice commandBufferDevice);
        public void CopyTo(IBufferDevice<T> dest);
        public void Clean();
    }

    public enum BufferUsage
    {
        Vertex,
        Index,
        Storage
    }

    public enum BufferMemoryUsage
    {
        Cpu,
        Gpu
    }

    // TODO Make a function that will copy to gpu memory or not as, high frequency changes should use cpu memory, but static be gpu memory
    /// <summary>
    /// By default will store memory on the cpu side
    /// Copy to gpu memory if needed on gpu local memory
    /// </summary>
    public class Buffer<T> : IDisposable where T : unmanaged
    {
        public IBufferDevice<T>? buffer;
        public BufferUsage usage;
        public BufferMemoryUsage memoryUsage;
        public ulong size;
        
        public unsafe void Create(in Span<T> data, BufferUsage usage, BufferMemoryUsage memoryUsage)
        {
            this.usage = usage;
            this.memoryUsage = memoryUsage;
            buffer = AppState.appContext.renderer.CreateBufferDevice(data, usage, memoryUsage);
            size = (ulong)(data.Length * sizeof(T));
        }

        /// <summary>
        /// Wont copy data (No data to copy..) but will allocate the array size specified
        /// </summary>
        public unsafe void Create(int dataLength, BufferUsage usage, BufferMemoryUsage memoryUsage)
        {
            this.usage = usage;
            this.memoryUsage = memoryUsage;
            buffer = AppState.appContext.renderer.CreateBufferDevice<T>(dataLength, usage, memoryUsage);
            size = (ulong)(dataLength * sizeof(T));
        }

        public void Bind(in CommandBuffer commandBuffer)
        {
            
        }

        public void CopyTo(Buffer<T> dest)
        {
            
        }

        public void Clean()
        {
            buffer?.Clean();
        }
        
        public void Dispose()
        {
            Clean();
        }
    }
}