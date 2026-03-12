using Silk.NET.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public static class VkDepthBuffer
    {
        static Image depthImage;
        static DeviceMemory depthImageMemory;
        static ImageView depthImageView;

        public static void CreateDepthBuffers()
        {
            Format depthFormat = FindDepthFormat();

            //CreateImage(swapChainExtent.Width, swapChainExtent.Height, depthFormat, ImageTiling.Optimal, ImageUsageFlags.DepthStencilAttachmentBit, MemoryPropertyFlags.DeviceLocalBit, ref depthImage, ref depthImageMemory);
            //depthImageView = CreateImageView(depthImage, depthFormat, ImageAspectFlags.DepthBit);
        }
        
        static Format FindDepthFormat()
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
            
        }
    }
}