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
        
        public void Init();
        public void Update(float delta);
        public void Render();
        public void FinishRender();
        public void WindowResize();
        public unsafe void CleanObjects();
        public unsafe void CleanContext();

        /// <summary>
        /// Manual cleanup is needed
        /// </summary>
        public Texture GetRenderTexture();
        
        public T GetContext<T>() where T : AppContext;
    }
}
