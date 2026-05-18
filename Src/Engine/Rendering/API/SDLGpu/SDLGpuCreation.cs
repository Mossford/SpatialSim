using SDL3;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.SDLGpu;

namespace SpatialSim.Engine.Rendering.API.SDLGpu
{
    public static class SDLGpuCreation
    {
        public static void CreateInstance()
        {
            SDL.Init(SDL.InitFlags.Video);

            AppState.appContext.GetContext<SDLGpuContext>().window = SDL.CreateWindow("test", 1920, 1080, SDL.WindowFlags.Resizable);
            if (AppState.appContext.GetContext<SDLGpuContext>().window == IntPtr.Zero)
            {
                string error = SDL.GetError();
                Debug.Error($"SDL could not create window {error}");
                throw new Exception($"SDL could not create window {error}");
            }

            AppState.appContext.GetContext<SDLGpuContext>().gpuDevice =
                SDL.CreateGPUDevice(SDL.GPUShaderFormat.SPIRV, AppState.EnableValidationLayers, null);

            if (AppState.appContext.GetContext<SDLGpuContext>().gpuDevice == IntPtr.Zero)
            {
                string error = SDL.GetError();
                Debug.Error($"SDL could not get gpu device {error}");
                throw new Exception($"SDL could not get gpu device {error}");
            }

            if (!SDL.ClaimWindowForGPUDevice(AppState.appContext.GetContext<SDLGpuContext>().gpuDevice, AppState.appContext.GetContext<SDLGpuContext>().window))
            {
                string error = SDL.GetError();
                Debug.Error($"SDL could not claim window for gpu device {error}");
                throw new Exception($"SDL could not claim window for gpu device {error}");
            }
        }

        public static void CleanInstance()
        {
            SDL.ReleaseWindowFromGPUDevice(AppState.appContext.GetContext<SDLGpuContext>().gpuDevice, AppState.window.Handle);
        }
    }
}
