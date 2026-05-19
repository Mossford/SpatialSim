using SDL;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.SDLGpu;

namespace SpatialSim.Engine.Rendering.API.SDLGpu
{
    public static class SDLGpuCreation
    {
        public static unsafe void CreateInstance()
        {
            SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO);

            AppState.appContext.GetContext<SDLGpuContext>().window = SDL3.SDL_CreateWindow("test", 1920, 1080, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            if (AppState.appContext.GetContext<SDLGpuContext>().window == null)
            {
                string error = SDL3.SDL_GetError();
                Debug.Error($"SDL could not create window {error}");
                throw new Exception($"SDL could not create window {error}");
            }

            AppState.appContext.GetContext<SDLGpuContext>().gpuDevice =
                SDL3.SDL_CreateGPUDevice(SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV, AppState.EnableValidationLayers, "");

            if (AppState.appContext.GetContext<SDLGpuContext>().gpuDevice == null)
            {
                string error = SDL3.SDL_GetError();
                Debug.Error($"SDL could not get gpu device {error}");
                throw new Exception($"SDL could not get gpu device {error}");
            }

            if (!SDL3.SDL_ClaimWindowForGPUDevice(AppState.appContext.GetContext<SDLGpuContext>().gpuDevice, AppState.appContext.GetContext<SDLGpuContext>().window))
            {
                string error = SDL3.SDL_GetError();
                Debug.Error($"SDL could not claim window for gpu device {error}");
                throw new Exception($"SDL could not claim window for gpu device {error}");
            }
        }

        public static unsafe void CleanInstance()
        {
            SDL3.SDL_ReleaseWindowFromGPUDevice(AppState.appContext.GetContext<SDLGpuContext>().gpuDevice, AppState.appContext.GetContext<SDLGpuContext>().window);
        }
    }
}
