using Silk.NET.Windowing;
using SpatialSim.Engine.Core.Vulkan;
using SpatialSim.Engine.Rendering;

namespace SpatialSim.Engine.Core
{
    public interface AppContext
    {
        public GraphicsAPI graphicsApi { get; set; }
        public IDeviceFactory DeviceFactory { get; }

        //should be removed into a pipeline manager
        public Pipeline defaultPipeline { get; set; }
        public RenderPass renderPass { get; set; }
        
        public void Init();
        public void Update(float delta);
        public void Render();
        public void WindowResize();
        public unsafe void Clean();
        
        public T GetContext<T>() where T : VkContext;
    }
}
