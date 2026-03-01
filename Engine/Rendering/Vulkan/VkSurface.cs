using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public static class VkSurface
    {
        public static KhrSurface khrSurface;
        public static SurfaceKHR surface;
        public static Queue presentQueue;
        
        public static unsafe void CreateSurface()
        {
            if (!AppState.appContext.GetContext<VkContext>().vk.TryGetInstanceExtension(AppState.appContext.GetContext<VkContext>().instance, out khrSurface))
            {
                Debug.Error("KHR_surface extension not found");
                throw new NotSupportedException("KHR_surface extension not found");
            }

            surface = AppState.window.VkSurface!.Create<AllocationCallbacks>(AppState.appContext.GetContext<VkContext>().instance.ToHandle(), null).ToSurface();
            
            Debug.LogInfo("Successful surface creation");
        }

    }
}