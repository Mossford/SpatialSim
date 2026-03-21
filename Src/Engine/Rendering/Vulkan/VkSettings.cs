using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public static class VkSettings
    {
        #region SwapChain

        public static Format SwapchainColorFormat = Format.B8G8R8A8Unorm;
        public static ColorSpaceKHR SwapchainColorSpaceFormat = ColorSpaceKHR.SpaceSrgbNonlinearKhr;
        //immediate but no tearing for mailbox
        public static PresentModeKHR SwapchainPresentMode = PresentModeKHR.MailboxKhr;

        #endregion

        #region Uniforms
        
        //This is what sdlgpu has set to the maxuniform size
        public const uint MaxUniformSize = 1 << 15;
        //max of 8 shaders for the pipeline
        public const int MaxUniformsPerStage = 1;
        /// <summary>
        /// Max uniform section size of 4096 bytes
        /// </summary>
        public const uint MaxBlockUniformMemory = 1 << 12;

        #endregion

        #region Descriptors

        public const int MaxDescriptorsInPool = 1000;

        #endregion

        #region Extensions and Features

        public static readonly string[] deviceExtensions = new[]
        {
            KhrSwapchain.ExtensionName,
            KhrDeferredHostOperations.ExtensionName,
            KhrDynamicRendering.ExtensionName,
            KhrAccelerationStructure.ExtensionName,
            "VK_KHR_ray_query" //will need to find the actual class referencing this but this works for now
        };
        
        public static readonly PhysicalDeviceFeatures physicalDeviceFeatures = new PhysicalDeviceFeatures() with
        {
            SamplerAnisotropy = true,
        };
        
        public static readonly string[] validationLayers = new[]
        {
            "VK_LAYER_KHRONOS_validation",
        };
        
        public static readonly ValidationFeatureEnableEXT[] validationFeatures =
        {
            ValidationFeatureEnableEXT.BestPracticesExt,
            ValidationFeatureEnableEXT.SynchronizationValidationExt
        };

        #endregion
    }
}