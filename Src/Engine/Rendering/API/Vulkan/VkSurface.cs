using SDL;
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

            surface = new SurfaceKHR();
            
            ulong handleValue;
            VkSurfaceKHR_T* surfacePtr = (VkSurfaceKHR_T*)&handleValue;
            if (!SDL3.SDL_Vulkan_CreateSurface(AppState.window,
                    (VkInstance_T*)AppState.appContext.GetContext<VkContext>().instance.ToHandle().Handle, null,
                    &surfacePtr))
            {
                string error = "" + SDL3.SDL_GetError();
                Debug.Error($"SDL could not create vulkan surface {error}");
                throw new Exception($"SDL could not create vulkan surface {error}");
            }
            
            surface.Handle = (ulong)surfacePtr;
            
            
            Debug.LogInfo("Successful surface creation");
        }

    }
}