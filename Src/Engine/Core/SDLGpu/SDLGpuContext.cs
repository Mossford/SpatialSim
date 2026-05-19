using Silk.NET.Windowing;
using SpatialSim.Engine.Core.Vulkan;
using SpatialSim.Engine.Rendering;
using SDL;
using SpatialSim.Engine.Rendering.API.SDLGpu;

namespace SpatialSim.Engine.Core.SDLGpu
{
    public class SDLGpuContext : AppContext
    {
        public GraphicsAPI graphicsApi { get; set; }
        public IDeviceFactory DeviceFactory { get; }

        public unsafe SDL_Window* window;
        
        public unsafe SDL_GPUViewport* gpuViewport;
        public unsafe SDL_GPUDevice* gpuDevice;
        
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

        public void FinishRender()
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

        public Texture GetRenderTexture()
        {
            throw new NotImplementedException();
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
