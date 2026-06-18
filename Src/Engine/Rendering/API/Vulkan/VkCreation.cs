using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan.Extensions.EXT;
using SpatialSim.Engine.Core.Vulkan;
using System.Runtime.CompilerServices;
using SDL;
using SpatialSim.Engine.Rendering.ImGui;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public static class VkCreation
    {
        public static unsafe void CreateInstance()
        {
            AppState.appContext.GetContext<VkContext>().vk = Vk.GetApi();
            
            //if we request validation layers, check for support and keep enabled if we have validation support
            if (AppState.EnableValidationLayers)
            {
                AppState.EnableValidationLayers = VkValidationLayers.CheckValidationLayerSupport();
            }

            ApplicationInfo appInfo = new()
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("SpatialSim"),
                ApplicationVersion = new Version32(0, 1, 0),
                PEngineName = (byte*)Marshal.StringToHGlobalAnsi("SpatialSim Engine"),
                EngineVersion = new Version32(0, 1, 0),
                ApiVersion = Vk.Version13
            };

            InstanceCreateInfo createInfo = new()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo
            };

            string[] extensions = GetRequiredExtensions();
            createInfo.EnabledExtensionCount = (uint)extensions.Length;
            createInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions); ;

            if (AppState.EnableValidationLayers)
            {
                createInfo.EnabledLayerCount = (uint)VkSettings.validationLayers.Length;
                createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(VkSettings.validationLayers);
                
                DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new();
                VkValidationLayers.PopulateDebugMessengerCreateInfo(ref debugCreateInfo);

                fixed (ValidationFeatureEnableEXT* enablesPtr = VkSettings.validationFeatures)
                {
                    ValidationFeaturesEXT validationFeatures = new();
                    validationFeatures.SType = StructureType.ValidationFeaturesExt;
                    validationFeatures.EnabledValidationFeatureCount = (uint)VkSettings.validationFeatures.Length;
                    validationFeatures.PEnabledValidationFeatures = enablesPtr;

                    debugCreateInfo.PNext = &validationFeatures;
                }

                createInfo.PNext = &debugCreateInfo;
                
            }
            else
            {
                createInfo.EnabledLayerCount = 0;
                createInfo.PNext = null;
            }

            Result result = AppState.appContext.GetContext<VkContext>().vk.CreateInstance(in createInfo, null,
                out AppState.appContext.GetContext<VkContext>().instance);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to create instance {result}");
                throw new Exception($"Failed to create instance {result}");
            }

            Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);
            SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);

            if (AppState.EnableValidationLayers)
            {
                SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
            }
            
            Debug.LogInfo("Successful vulkan instance creation");
        }

        public static void CreateImGui()
        {
            AppState.appContext.GetContext<VkContext>().imGuiController = new VkImGuiController(
                AppState.appContext.GetContext<VkContext>().vk,
                VkDevices.physicalDevice,
                VkDevices.graphicsFamilyIndex,
                VkSwapChain.swapChainImages.Length,
                VkTexture.ConvertToVkFormat(TextureFormat.R8G8B8A8Unorm),
                VkDepthBuffer.FindDepthFormat()
            );
            
            Debug.LogInfo("Successful imgui creation");
        }
        
        static unsafe string[] GetRequiredExtensions()
        {
            uint surfaceExtensionCount;
            byte** surfaceExtensions = SDL3.SDL_Vulkan_GetInstanceExtensions(&surfaceExtensionCount);
            string[] extensions = SilkMarshal.PtrToStringArray((nint)surfaceExtensions, (int)surfaceExtensionCount);

            if (AppState.EnableValidationLayers)
            {
                return extensions.Append(ExtDebugUtils.ExtensionName).ToArray();
            }

            return extensions;
        }
    }
}