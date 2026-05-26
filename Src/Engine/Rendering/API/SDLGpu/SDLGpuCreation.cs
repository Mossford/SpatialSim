using SDL;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.SDLGpu;

namespace SpatialSim.Engine.Rendering.API.SDLGpu
{
    public static class SDLGpuCreation
    {
        public static unsafe void CreateInstance()
        {
            AppState.appContext.GetContext<SDLGpuContext>().gpuDevice =
                SDL3.SDL_CreateGPUDevice(SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV, AppState.EnableValidationLayers, (byte*)null);

            if (AppState.appContext.GetContext<SDLGpuContext>().gpuDevice == null)
            {
                string error = "" + SDL3.SDL_GetError();
                Debug.Error($"SDL could not get gpu device {error}");
                throw new Exception($"SDL could not get gpu device {error}");
            }

            AppState.Api = "" + SDL3.SDL_GetGPUDeviceDriver(AppState.appContext.GetContext<SDLGpuContext>().gpuDevice);

            if (!SDL3.SDL_ClaimWindowForGPUDevice(AppState.appContext.GetContext<SDLGpuContext>().gpuDevice, AppState.window))
            {
                string error = "" + SDL3.SDL_GetError();
                Debug.Error($"SDL could not claim window for gpu device {error}");
                throw new Exception($"SDL could not claim window for gpu device {error}");
            }
        }

        public static unsafe void CleanInstance()
        {
            SDL3.SDL_ReleaseWindowFromGPUDevice(AppState.appContext.GetContext<SDLGpuContext>().gpuDevice, AppState.window);
            SDL3.SDL_DestroyGPUDevice(AppState.appContext.GetContext<SDLGpuContext>().gpuDevice);
        }
    }
}
