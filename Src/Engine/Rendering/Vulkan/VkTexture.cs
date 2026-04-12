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
        public Filter filter;
        public ImageUsageFlags usage;
        
        //to be able to access the image
        public ImageView imageView;
        //to be able to sample texture in shader
        public Sampler sampler;
        
        public void Create(in TextureData data)
        {
            //create some staging buffer
            //upload to gpu side to store texture memory
            //copy buffer to image

            format = Format.R8G8B8A8Unorm;
            switch (data.info.format)
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
            usage = ImageUsageFlags.TransferDstBit;
            switch (data.info.usage)
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
            switch (data.info.memoryUsage)
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

            Create(data.info.width, data.info.height, format, ImageTiling.Optimal, usage, memUsage);
            
            //create the staging buffer
            Buffer<byte> stagingBuffer = new Buffer<byte>();
            stagingBuffer.Create(new Span<byte>(data.data), BufferUsage.Transfer, BufferMemoryUsage.Cpu);

            TransitionImageLayout(ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
            
            stagingBuffer.CopyToTexture(this, data);
            
            TransitionImageLayout(ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);
            
            stagingBuffer.Clean();
            
            CreateImageView(ImageAspectFlags.ColorBit);

            filter = Filter.Linear;
            switch (data.info.filter)
            {
                case TextureFilter.Linear:
                {
                    filter = Filter.Linear;
                    break;
                }
                case TextureFilter.Nearest:
                {
                    filter = Filter.Nearest;
                    break;
                }
            }
            
            CreateSampler();
        }

        /// <summary>
        /// Internal vulkan use
        /// </summary>
        public unsafe void Create(uint width, uint height, Format format, ImageTiling tiling, ImageUsageFlags usage, MemoryPropertyFlags properties)
        {
            this.format = format;
            
            //TODO If extent is bigger than maximum split up image into multiple parts
            AppState.appContext.GetContext<VkContext>().vk.GetPhysicalDeviceImageFormatProperties(
                    VkDevices.physicalDevice,
                    format,
                    ImageType.Type2D,
                    tiling,
                    usage,
                    ImageCreateFlags.Create2DArrayCompatibleBit,
                    out ImageFormatProperties formatProperties);

            if (formatProperties.MaxExtent.Width <= width)
            {
                Debug.Error($"Image width {width} is greater than max extent width {formatProperties.MaxExtent.Width} for format {format}");
            }
            if (formatProperties.MaxExtent.Height <= height)
            {
                Debug.Error($"Image height {height} is greater than max extent height {formatProperties.MaxExtent.Height} for format {format}");
            }
            
            ImageCreateInfo imageInfo = new()
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Extent =
                {
                    Width = width,
                    Height = height,
                    Depth = 1,
                },
                MipLevels = 1,
                ArrayLayers = 1,
                Format = format,
                Tiling = tiling,
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

            AppState.appContext.GetContext<VkContext>().vk.GetImageMemoryRequirements(
                VkDevices.device, image, out MemoryRequirements memRequirements);

            MemoryAllocateInfo allocInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memRequirements.Size,
                MemoryTypeIndex = VkDevices.FindMemoryType(memRequirements.MemoryTypeBits, properties),
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
        }

        public unsafe void TransitionImageLayout(ImageLayout oldLayout, ImageLayout newLayout)
        {
            CommandBuffer commandBuffer = new CommandBuffer();
            commandBuffer.Create();
            commandBuffer.BeginOneUse();

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
                    //TODO make this a parameter
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },
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
                Debug.Error("Unsupported layout transition");
                throw new Exception("Unsupported layout transition");
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

            commandBuffer.End();
            commandBuffer.Submit();
            commandBuffer.Clean();
        }
        
        public static unsafe void TransitionImageLayout(
            CommandBuffer commandBuffer,
            Image image,
            ImageLayout oldLayout,
            ImageLayout newLayout,
            AccessFlags srcAccessMask,
            AccessFlags dstAccessMask,
            PipelineStageFlags srcStage,
            PipelineStageFlags dstStage,
            ImageAspectFlags aspectFlags
        )
        {

            ImageMemoryBarrier barrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,
                OldLayout = oldLayout,
                NewLayout = newLayout,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                Image = image,
                SubresourceRange = new()
                {
                    AspectMask     = aspectFlags,
                    BaseMipLevel   = 0,
                    LevelCount     = 1,
                    BaseArrayLayer = 0,
                    LayerCount     = 1
                },
                SrcAccessMask = srcAccessMask,
                DstAccessMask = dstAccessMask
            };

            AppState.appContext.GetContext<VkContext>().vk.CmdPipelineBarrier(
                ((VkCommandBuffer)commandBuffer.commandBuffer!).commandBuffer,
                srcStage,
                dstStage,
                0,
                0,
                null,
                0,
                null,
                1,
                in barrier
            );
        }

        /// <summary>
        /// Internal vulkaneedDescriptorUpdaten use
        /// </summary>
        public unsafe void CreateImageView(ImageAspectFlags imageAspectFlags)
        {
            ImageViewCreateInfo createInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = image,
                ViewType = ImageViewType.Type2D,
                Format = format,
                Components =
                {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity,
                },
                SubresourceRange =
                {
                    AspectMask = imageAspectFlags,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }

            };

            Result result = AppState.appContext.GetContext<VkContext>().vk
                .CreateImageView(VkDevices.device, in createInfo, null, out imageView);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to create image view {result}");
                throw new Exception($"Failed to create image view {result}");
            }
        }

        public unsafe void CreateSampler()
        {
            SamplerCreateInfo samplerInfo = new()
            {
                SType = StructureType.SamplerCreateInfo,
                MagFilter = filter,
                MinFilter = filter,
                AddressModeU = SamplerAddressMode.Repeat,
                AddressModeV = SamplerAddressMode.Repeat,
                AddressModeW = SamplerAddressMode.Repeat,
                AnisotropyEnable = true,
                MaxAnisotropy = VkDevices.properties.Limits.MaxSamplerAnisotropy,
                BorderColor = BorderColor.IntOpaqueBlack,
                UnnormalizedCoordinates = false,
                CompareEnable = false,
                CompareOp = CompareOp.Always,
                MipmapMode = SamplerMipmapMode.Linear,
            };

            fixed (Sampler* textureSamplerPtr = &sampler)
            {
                Result result = AppState.appContext.GetContext<VkContext>().vk
                    .CreateSampler(VkDevices.device, in samplerInfo, null, textureSamplerPtr);
                if (result != Result.Success)
                {
                    Debug.Error($"Failed to create texture sampler {result}");
                    throw new Exception($"Failed to create texture sampler {result}");
                }
            }
        }
        
        public unsafe void SetTextureToDescriptorSet(VkDescriptor descriptor, int binding)
        {
            DescriptorImageInfo imageInfo = new()
            {
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
                ImageView = imageView,
                Sampler = sampler
            };
            
            WriteDescriptorSet descriptorWrite = new()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptor.descriptorSet,
                //SdlGpu has the binding as i and the array element as 0 look into why
                DstBinding = 0,
                DstArrayElement = (uint)binding,
                DescriptorType = DescriptorType.CombinedImageSampler,
                DescriptorCount = 1,
                PImageInfo = &imageInfo
            };
            
            AppState.appContext.GetContext<VkContext>().vk.UpdateDescriptorSets(
                VkDevices.device,
                1,
                in descriptorWrite,
                0,
                null);
        }

        public unsafe void Clean()
        {
            AppState.appContext.GetContext<VkContext>().vk.DestroySampler(VkDevices.device, sampler, null);
            AppState.appContext.GetContext<VkContext>().vk.DestroyImageView(VkDevices.device, imageView, null);
            AppState.appContext.GetContext<VkContext>().vk.DestroyImage(VkDevices.device, image, null);
            AppState.appContext.GetContext<VkContext>().vk.FreeMemory(VkDevices.device, memory, null);
        }
    }
}