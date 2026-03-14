using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public static class VkValidationLayers
    {
        
        public static ExtDebugUtils debugUtils;
        public static DebugUtilsMessengerEXT debugMessenger;
        
        public static unsafe bool CheckValidationLayerSupport()
        {
            uint layerCount = 0;
            AppState.appContext.GetContext<VkContext>().vk.EnumerateInstanceLayerProperties(ref layerCount, null);
            LayerProperties[] availableLayers = new LayerProperties[layerCount];
            fixed (LayerProperties* availableLayersPtr = availableLayers)
            {
                AppState.appContext.GetContext<VkContext>().vk.EnumerateInstanceLayerProperties(ref layerCount, availableLayersPtr);
            }

            HashSet<string?> availableLayerNames = availableLayers.Select(layer => Marshal.PtrToStringAnsi((IntPtr)layer.LayerName)).ToHashSet();

            return VkSettings.validationLayers.All(availableLayerNames.Contains);
        }
        
        public static unsafe void PopulateDebugMessengerCreateInfo(ref DebugUtilsMessengerCreateInfoEXT createInfo)
        {
            createInfo.SType = StructureType.DebugUtilsMessengerCreateInfoExt;
            createInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                                         DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt;
            createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt |
                                     DebugUtilsMessageTypeFlagsEXT.ValidationBitExt;
            createInfo.PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT)DebugCallback;
        }

        public static unsafe void SetupDebugMessenger()
        {
            if (!AppState.EnableVkValidationLayers) return;

            //TryGetInstanceExtension equivilant to method CreateDebugUtilsMessengerEXT from original tutorial.
            if (!AppState.appContext.GetContext<VkContext>().vk.TryGetInstanceExtension(AppState.appContext.GetContext<VkContext>().instance, out debugUtils))
                return;

            DebugUtilsMessengerCreateInfoEXT createInfo = new();
            PopulateDebugMessengerCreateInfo(ref createInfo);

            Result result = debugUtils!.CreateDebugUtilsMessenger(AppState.appContext.GetContext<VkContext>().instance,
                in createInfo, null, out debugMessenger);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to set up vulkan debug messenger {result}");
                throw new Exception($"Failed to set up vulkan debug messenger {result}");
            }
        }

        static unsafe uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageTypes, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
        {
            if (messageSeverity == DebugUtilsMessageSeverityFlagsEXT.WarningBitExt)
            {
                Debug.Warning("" + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));
            }
            else if (messageSeverity == DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt)
            {
                Debug.Error("" + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));
            }
            else
            {
                Debug.LogDebug("" + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));
            }

            return Vk.False;
        }
    }
}