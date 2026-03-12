using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering
{
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
            buffer = AppState.appContext.DeviceFactory.CreateBufferDevice(data, usage, memoryUsage);
            size = (ulong)(data.Length * sizeof(T));
            
            Debug.LogDebug($"Created buffer of type {typeof(T).Name} of size {sizeof(T) * data.Length}");
            Ticks.bufferCount++;
        }

        /// <summary>
        /// Wont copy data (No data to copy..) but will allocate the array size specified
        /// </summary>
        public unsafe void Create(uint dataLength, BufferUsage usage, BufferMemoryUsage memoryUsage)
        {
            this.usage = usage;
            this.memoryUsage = memoryUsage;
            buffer = AppState.appContext.DeviceFactory.CreateBufferDevice<T>(dataLength, usage, memoryUsage);
            size = (ulong)(dataLength * sizeof(T));
            
            Debug.LogDebug($"Created buffer of type {typeof(T).Name} of size {sizeof(T) * dataLength}");
            Ticks.bufferCount++;
        }

        public void Bind(in CommandBuffer commandBuffer)
        {
            
        }

        public void CopyTo(Buffer<T> dest)
        {
            buffer?.CopyTo(dest.buffer!);
        }

        public void CopyToTexture(ITextureDevice dest, in TextureData destData)
        {
            buffer?.CopyToTexture(dest, in destData);
        }

        public void UpdateData(in Span<T> data)
        {
            buffer?.UpdateData(data);
        }

        public void UpdateUniformData(in Span<T> data)
        {
            buffer?.UpdateUniformData(data);
        }

        public void Clean()
        {
            buffer?.Clean();
            Debug.LogDebug($"Cleaned up Buffer of type {typeof(T).Name}");
            Ticks.bufferCount--;
        }
        
        public void Dispose()
        {
            Clean();
        }
    }
}