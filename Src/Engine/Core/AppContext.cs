using Silk.NET.Windowing;
using SpatialSim.Engine.Core.SDLGpu;
using SpatialSim.Engine.Core.Vulkan;
using SpatialSim.Engine.Rendering;

namespace SpatialSim.Engine.Core
{
    public interface AppContext
    {
        public GraphicsAPI graphicsApi { get; set; }
        public IDeviceFactory DeviceFactory { get; }
        public Texture renderTexture { get; set;  }
        
        public void Init();
        public void Update(float delta);
        public void Render();
        public void WindowResize();
        public unsafe void CleanObjects();
        public unsafe void CleanContext();
        
        public T GetContext<T>() where T : AppContext;
    }
}
