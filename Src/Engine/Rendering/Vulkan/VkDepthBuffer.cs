using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public static class VkDepthBuffer
    {
        public static VkTexture texture;

        public static void CreateDepthBuffers()
        {
            Format depthFormat = FindDepthFormat();

            texture = new VkTexture();
            texture.Create(
                VkSwapChain.swapChainExtent.Width, 
                VkSwapChain.swapChainExtent.Height, 
                FindDepthFormat(), 
                ImageTiling.Optimal, 
                ImageUsageFlags.DepthStencilAttachmentBit, 
                MemoryPropertyFlags.DeviceLocalBit);
            texture.CreateImageView(ImageAspectFlags.DepthBit);
            
            Debug.LogInfo("Successful vulkan depth buffer creation");
        }
        
        public static Format FindDepthFormat()
        {
            return VkDevices.FindSupportedFormat(
                [
                    Format.D32Sfloat, 
                    Format.D32SfloatS8Uint, 
                    Format.D24UnormS8Uint
                        ], 
                ImageTiling.Optimal, 
                FormatFeatureFlags.DepthStencilAttachmentBit);
        }

        public static void Clean()
        {
            texture.Clean();
        }
    }
}