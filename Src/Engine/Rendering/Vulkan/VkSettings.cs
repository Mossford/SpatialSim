using Silk.NET.Vulkan;

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
        public const int MaxUniformsPerStage = 4;

        #endregion
    }
}