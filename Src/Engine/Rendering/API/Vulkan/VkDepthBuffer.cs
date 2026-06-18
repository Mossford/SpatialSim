using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

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
                1,
                depthFormat,
                ImageTiling.Optimal, 
                ImageUsageFlags.DepthStencilAttachmentBit | ImageUsageFlags.TransferSrcBit, 
                MemoryPropertyFlags.DeviceLocalBit,
                ImageType.Type2D);
            TransitionDepthLayout();
            texture.CreateImageView(ImageAspectFlags.DepthBit, ImageViewType.Type2D);
            
            Debug.LogInfo($"Successful vulkan depth buffer creation on format {depthFormat}");
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

        static unsafe void TransitionDepthLayout()
        {
            CommandBuffer commandBuffer = new CommandBuffer();
            commandBuffer.Create();
            commandBuffer.BeginOneUse();

            ImageMemoryBarrier barrier = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = ImageLayout.Undefined,
                NewLayout = ImageLayout.DepthStencilAttachmentOptimal,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = texture.image,
                SubresourceRange =
                {
                    AspectMask = ImageAspectFlags.DepthBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                },
                SrcAccessMask = 0,
                DstAccessMask = AccessFlags.DepthStencilAttachmentReadBit | AccessFlags.DepthStencilAttachmentWriteBit
            };

            AppState.appContext.GetContext<VkContext>().vk.CmdPipelineBarrier(
                ((VkCommandBuffer)commandBuffer.commandBuffer!).commandBuffer,
                PipelineStageFlags.TopOfPipeBit,
                PipelineStageFlags.EarlyFragmentTestsBit,
                0, 
                0, 
                null, 
                0, 
                null, 
                1, 
                in barrier
            );

            commandBuffer.End();
            commandBuffer.Submit();
            commandBuffer.Clean();
        }

        public static void Clean()
        {
            texture.Clean();
        }
    }
}