using System.Text;
using SDL;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.SDLGpu;

namespace SpatialSim.Engine.Rendering.API.SDLGpu
{
    public class SDLGpuShader : IShaderDevice
    {
        public unsafe SDL_GPUShader* program;
        
        public unsafe void Create(ShaderSettings settings, in byte[] code)
        {
            SDL_GPUShaderStage stage = SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_VERTEX;
            if (settings.type == ShaderType.Fragment)
            {
                stage = SDL_GPUShaderStage.SDL_GPU_SHADERSTAGE_FRAGMENT;
            }
            SDL_GPUShaderFormat format = SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV;
            //for glsl this will only be main

            /*SDL_GPUShaderCreateInfo shaderInfo;
            fixed (byte* codePtr = code)
            {
                byte[] entryPoint = Encoding.UTF8.GetBytes("main");
                fixed (byte* entrypoint = entryPoint)
                {
                    shaderInfo = new SDL_GPUShaderCreateInfo()
                    {
                        code_size = (UIntPtr)code.Length,
                        code = codePtr,
                        entrypoint = entrypoint,
                        format = format,
                        stage = stage,
                        num_samplers = settings.,
                        num_storage_textures = settings.storageTextureCount,
                        num_storage_buffers = settings.storageBufferCount,
                        num_uniform_buffers = settings.uniformBufferCount,
                    };
                }
            }

            SDL_GPUDevice* device = context->GetSDLGpuContext().gpuDevice;

            SDL_GPUShader* shader = SDL_CreateGPUShader(device, &shaderInfo);
            if (shader == NULL)
            {
                LogError("Failed to create shader!");
                LogError("%s", SDL_GetError());
                return;
            }

            GetSDLGpuShader() = shader;*/
        }

        public void Clean()
        {
            
        }
    }
}