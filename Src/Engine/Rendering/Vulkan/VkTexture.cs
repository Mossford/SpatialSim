using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public class VkTexture : ITextureDevice
    {
        public Image image;
        public DeviceMemory memory;
        public Format format;
        
        public unsafe void Create(in TextureData data)
        {
            //create some staging buffer
            //upload to gpu side to store texture memory
            //copy buffer to image

            format = Format.R8G8B8A8Unorm;
            switch (data.format)
            {
                case TextureFormat.R8G8B8A8Unorm:
                {
                    format = Format.R8G8B8A8Unorm;
                    break;
                }
                case TextureFormat.R8G8B8A8Srgb:
                {
                    format = Format.R8G8B8A8Srgb;
                    break;
                }
                case TextureFormat.R8G8B8Uint:
                {
                    format = Format.R8G8B8Uint;
                    break;
                }
                case TextureFormat.R8G8B8A8Uint:
                {
                    format = Format.R8G8B8A8Uint;
                    break;
                }
            }

            // TODO make it possible that there might be multiple usages attached?
            //Texture is set as the destination as we wont need to read back from it most of the time
            ImageUsageFlags usage = ImageUsageFlags.TransferDstBit;
            switch (data.usage)
            {
                case TextureUsage.Sampler:
                {
                    usage |= ImageUsageFlags.SampledBit;
                    break;
                }
                case TextureUsage.Storage:
                {
                    usage |= ImageUsageFlags.StorageBit;
                    break;
                }
                case TextureUsage.ColorFramebuffer:
                {
                    usage |= ImageUsageFlags.ColorAttachmentBit;
                    break;
                }
            }
            
            MemoryPropertyFlags memUsage = MemoryPropertyFlags.DeviceLocalBit;
            switch (data.memoryUsage)
            {
                case TextureMemoryUsage.cpu:
                {
                    memUsage = MemoryPropertyFlags.HostVisibleBit;
                    break;
                }
                case TextureMemoryUsage.gpu:
                {
                    memUsage = MemoryPropertyFlags.DeviceLocalBit;
                    break;
                }
            }
            
            ImageCreateInfo imageInfo = new()
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Extent =
                {
                    Width = data.width,
                    Height = data.height,
                    Depth = 1,
                },
                MipLevels = 1,
                ArrayLayers = 1,
                Format = format,
                // TODO check what this does
                Tiling = ImageTiling.Optimal,
                InitialLayout = ImageLayout.Undefined,
                Usage = usage,
                Samples = SampleCountFlags.Count1Bit,
                SharingMode = SharingMode.Exclusive,
            };

            fixed (Image* imagePtr = &image)
            {
                Result result = AppState.appContext.GetContext<VkContext>().vk
                    .CreateImage(VkDevices.device, in imageInfo, null, imagePtr);
                if (result != Result.Success)
                {
                    Debug.Error($"Failed to create image {result}");
                    throw new Exception($"Failed to create image {result}");
                }
            }

            AppState.appContext.GetContext<VkContext>().vk.GetImageMemoryRequirements(VkDevices.device, image, out MemoryRequirements memRequirements);

            MemoryAllocateInfo allocInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memRequirements.Size,
                MemoryTypeIndex = VkDevices.FindMemoryType(memRequirements.MemoryTypeBits, memUsage),
            };

            fixed (DeviceMemory* imageMemoryPtr = &memory)
            {
                Result result = AppState.appContext.GetContext<VkContext>().vk
                    .AllocateMemory(VkDevices.device, in allocInfo, null, imageMemoryPtr);
                if (result != Result.Success)
                {
                    Debug.Error($"Failed to allocate image memory {result}");
                    throw new Exception($"Failed to allocate image memory {result}");
                }
            }

            AppState.appContext.GetContext<VkContext>().vk.BindImageMemory(VkDevices.device, image, memory, 0);
            
            //create the staging buffer
            Buffer<byte> stagingBuffer = new Buffer<byte>();
            stagingBuffer.Create(new Span<byte>(data.data), BufferUsage.Transfer, BufferMemoryUsage.Cpu);

            TransitionImageLayout(ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
            
            stagingBuffer.CopyToTexture(this, data);
            
            TransitionImageLayout(ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);
            
            stagingBuffer.Clean();
        }

        unsafe void TransitionImageLayout(ImageLayout oldLayout, ImageLayout newLayout)
        {
            CommandBuffer commandBuffer = new CommandBuffer();
            commandBuffer.Create();
            commandBuffer.BeginCommandBuffer();

            ImageMemoryBarrier barrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = oldLayout,
                NewLayout = newLayout,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = image,
                SubresourceRange =
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }
            };

            PipelineStageFlags sourceStage;
            PipelineStageFlags destinationStage;

            if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
            {
                barrier.SrcAccessMask = 0;
                barrier.DstAccessMask = AccessFlags.TransferWriteBit;

                sourceStage = PipelineStageFlags.TopOfPipeBit;
                destinationStage = PipelineStageFlags.TransferBit;
            }
            else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
                barrier.DstAccessMask = AccessFlags.ShaderReadBit;

                sourceStage = PipelineStageFlags.TransferBit;
                destinationStage = PipelineStageFlags.FragmentShaderBit;
            }
            else
            {
                throw new Exception("unsupported layout transition!");
            }

            AppState.appContext.GetContext<VkContext>().vk.CmdPipelineBarrier(
                ((VkCommandBuffer)commandBuffer.commandBuffer!).commandBuffer,
                sourceStage, 
                destinationStage, 
                0, 
                0, 
                null, 
                0, 
                null, 
                1, 
                in barrier);

            commandBuffer.EndCommandBuffer();
            commandBuffer.SubmitCommandBuffer();
            commandBuffer.Clean();
        }

        public unsafe void Clean()
        {
            AppState.appContext.GetContext<VkContext>().vk.DestroyImage(VkDevices.device, image, null);
            AppState.appContext.GetContext<VkContext>().vk.FreeMemory(VkDevices.device, memory, null);
        }
    }
}