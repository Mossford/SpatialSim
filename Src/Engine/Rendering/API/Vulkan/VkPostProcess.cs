using Silk.NET.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public class VkPostProcess : IPostProcessDevice
    {
        public VkTexture texture;
        
        public void Create(ref Texture inputTex)
        {
            texture = new VkTexture();
            
            texture.Create(inputTex.data.info.width,
                inputTex.data.info.height,
                VkSwapChain.swapChainImageFormat,
                ImageTiling.Optimal,
                ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.SampledBit,
                MemoryPropertyFlags.DeviceLocalBit);
            texture.CreateImageView(ImageAspectFlags.ColorBit);
            texture.CreateSampler();
            
            inputTex.texture = texture;
        }
        
        public void EnableRead(CommandBuffer commandBuffer)
        {
            texture.TransitionImageLayout(
                commandBuffer,
                ImageLayout.ShaderReadOnlyOptimal,
                AccessFlags.ShaderReadBit,
                PipelineStageFlags.FragmentShaderBit,
                ImageAspectFlags.ColorBit);
        }

        public void EnableWrite(CommandBuffer commandBuffer)
        {
            texture.TransitionImageLayout(
                commandBuffer,
                ImageLayout.ColorAttachmentOptimal,
                AccessFlags.ColorAttachmentWriteBit,
                PipelineStageFlags.ColorAttachmentOutputBit,
                ImageAspectFlags.ColorBit);
        }
    }
}