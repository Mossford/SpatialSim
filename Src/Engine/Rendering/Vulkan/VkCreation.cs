using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan.Extensions.EXT;
using SpatialSim.Engine.Core.Vulkan;
using System.Runtime.CompilerServices;
using SpatialSim.Engine.Rendering.ImGui;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public static class VkCreation
    {
        public static unsafe void CreateInstance()
        {
            AppState.appContext.GetContext<VkContext>().vk = Vk.GetApi();

            if (AppState.EnableVkValidationLayers && !VkValidationLayers.CheckValidationLayerSupport())
            {
                Debug.Error("Validation layers requested, but not available");
                throw new Exception("Validation layers requested, but not available");
            }

            ApplicationInfo appInfo = new()
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("SpatialSim"),
                ApplicationVersion = new Version32(0, 1, 0),
                PEngineName = (byte*)Marshal.StringToHGlobalAnsi("SpatialSim Engine"),
                EngineVersion = new Version32(0, 1, 0),
                ApiVersion = Vk.Version12
            };

            InstanceCreateInfo createInfo = new()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo
            };

            string[] extensions = GetRequiredExtensions();
            createInfo.EnabledExtensionCount = (uint)extensions.Length;
            createInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions); ;

            if (AppState.EnableVkValidationLayers)
            {
                createInfo.EnabledLayerCount = (uint)VkValidationLayers.validationLayers.Length;
                createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(VkValidationLayers.validationLayers);

                DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new();
                VkValidationLayers.PopulateDebugMessengerCreateInfo(ref debugCreateInfo);
                createInfo.PNext = &debugCreateInfo;
            }
            else
            {
                createInfo.EnabledLayerCount = 0;
                createInfo.PNext = null;
            }

            if (AppState.appContext.GetContext<VkContext>().vk.CreateInstance(in createInfo, null, out AppState.appContext.GetContext<VkContext>().instance) != Result.Success)
            {
                Debug.Error("Failed to create instance");
                throw new Exception("Failed to create instance");
            }

            Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
            Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);
            SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);

            if (AppState.EnableVkValidationLayers)
            {
                SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
            }
            
            Debug.LogInfo("Successful vulkan instance creation");
        }

        public static void CreateImGui()
        {
            AppState.appContext.GetContext<VkContext>().imGuiController = new VkImGuiController(
                AppState.appContext.GetContext<VkContext>().vk,
                AppState.window,
                Input.input,
                VkDevices.physicalDevice,
                VkDevices.graphicsFamilyIndex,
                VkSwapChain.swapChainImages.Length,
                VkSwapChain.swapChainImageFormat,
                null
            );
            
            ImGuiNET.ImGui.StyleColorsDark();
            
            Debug.LogInfo("Successful imgui creation");
        }
        
        static unsafe string[] GetRequiredExtensions()
        {
            byte** glfwExtensions = AppState.window.VkSurface!.GetRequiredExtensions(out uint glfwExtensionCount);
            string[] extensions = SilkMarshal.PtrToStringArray((nint)glfwExtensions, (int)glfwExtensionCount);

            if (AppState.EnableVkValidationLayers)
            {
                return extensions.Append(ExtDebugUtils.ExtensionName).ToArray();
            }

            return extensions;
        }
    }
}