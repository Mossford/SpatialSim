using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{

    public class VkShader : IShaderDevice
    {
        public ShaderModule program;
        
        public void Create(ShaderSettings settings, in byte[] code)
        {
            if (!File.Exists(Resources.ShaderPath + settings.file))
            {
                Debug.Error("Could not find shader " + settings.file);
                return;
            }

            program = CreateShaderModule(code);
        }

        public unsafe void Clean()
        {
            AppState.appContext.GetContext<VkContext>().vk.DestroyShaderModule(VkDevices.device, program, null);
        }

        unsafe ShaderModule CreateShaderModule(byte[] code)
        {
            ShaderModuleCreateInfo createInfo = new()
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)code.Length,
            };

            ShaderModule shaderModule;

            fixed (byte* codePtr = code)
            {
                createInfo.PCode = (uint*)codePtr;

                if (AppState.appContext.GetContext<VkContext>().vk.CreateShaderModule(VkDevices.device, in createInfo, null, out shaderModule) != Result.Success)
                {
                    throw new Exception();
                }
            }

            return shaderModule;
        }
    }
}