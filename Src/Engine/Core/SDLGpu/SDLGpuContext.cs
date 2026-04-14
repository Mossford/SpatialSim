using Silk.NET.Windowing;
using SpatialSim.Engine.Core.Vulkan;
using SpatialSim.Engine.Rendering;
using SDL3;
using SpatialSim.Engine.Rendering.API.SDLGpu;

namespace SpatialSim.Engine.Core.SDLGpu
{
    public class SDLGpuContext : AppContext
    {
        public GraphicsAPI graphicsApi { get; set; }
        public IDeviceFactory DeviceFactory { get; }

        public IntPtr window;
        
        public SDL.GPUViewport gpuViewport;
        public IntPtr gpuDevice;
        
        public void Init()
        {
            SDLGpuCreation.CreateInstance();
        }

        public void Update(float delta)
        {
            
        }

        public void Render()
        {
            
        }

        public void WindowResize()
        {
            
        }

        public unsafe void CleanObjects()
        {
            
        }

        public unsafe void CleanContext()
        {
            SDLGpuCreation.CleanInstance();
        }

        T AppContext.GetContext<T>()
        {
            if (typeof(T) != typeof(SDLGpuContext))
            {
                Debug.Error($"Type {typeof(T).Name} does not match contained type {nameof(SDLGpuContext)}");
                throw new Exception($"Type {typeof(T)} does not match contained type {nameof(SDLGpuContext)}");
            }
            
            return (T)(object)this;
        }
    }
}