using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public class VkRenderPass : RenderPass
    {
        public Silk.NET.Vulkan.RenderPass renderPass;
        
        public override unsafe void Create()
        {
            AttachmentDescription colorAttachment = new()
            {
                Format = VkSwapChain.swapChainImageFormat,
                Samples = SampleCountFlags.Count1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr,
            };

            AttachmentReference colorAttachmentRef = new()
            {
                Attachment = 0,
                Layout = ImageLayout.ColorAttachmentOptimal,
            };

            SubpassDescription subpass = new()
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = &colorAttachmentRef,
            };
            
            SubpassDependency dependency = new SubpassDependency
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                SrcAccessMask = 0,
                DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                DstAccessMask = AccessFlags.ColorAttachmentReadBit | AccessFlags.ColorAttachmentWriteBit
            };
            
            RenderPassCreateInfo renderPassInfo = new()
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = 1,
                PAttachments = &colorAttachment,
                SubpassCount = 1,
                PSubpasses = &subpass,
                DependencyCount = 1,
                PDependencies = &dependency
            };

            if (AppState.appContext.GetContext<VkContext>().vk.CreateRenderPass(VkDevices.device, in renderPassInfo, null, out renderPass) != Result.Success)
            {
                Debug.Error("Failed to create render pass");
                throw new Exception("Failed to create render pass");
            }
            
            Debug.LogInfo("Successful renderpass creation");
        }

        public override unsafe void Clean()
        {
            AppState.appContext.GetContext<VkContext>().vk.DestroyRenderPass(VkDevices.device, renderPass, null);
            Debug.LogInfo("Cleaned up Renderpass");
        }
    }   
}