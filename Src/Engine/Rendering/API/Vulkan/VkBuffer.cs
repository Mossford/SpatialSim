using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public unsafe class VkBuffer<T> : IBufferDevice<T> where T : unmanaged
    {
        public Silk.NET.Vulkan.Buffer buffer;
        public DeviceMemory bufferMemory;
        public ulong size;
        //will be a nullptr if buffer is on gpu side
        public byte* memoryMap;
        public uint memoryOffset;
        public uint drawOffset;
        public BufferMemoryUsage memoryUsage;
        public BufferUsage usage;
        
        public void Create(in Span<T> data, BufferUsage usage, BufferMemoryUsage memoryUsage)
        {
            this.memoryUsage = memoryUsage;
            this.usage = usage;
            if (data.Length == 0)
            {
                Debug.Warning($"Buffer of type {typeof(T).Name} created with no data, skipping creation (Will be Null)");
                return;
            }
            
            BufferUsageFlags bufferUsage = BufferUsageFlags.StorageBufferBit;
            switch (usage)
            {
                case BufferUsage.Vertex:
                {
                    bufferUsage = BufferUsageFlags.VertexBufferBit;
                    break;
                }
                case BufferUsage.Index:
                {
                    bufferUsage = BufferUsageFlags.IndexBufferBit;
                    break;
                }
                case BufferUsage.Storage:
                {

                    bufferUsage = BufferUsageFlags.StorageBufferBit;
                    break;
                }
                case BufferUsage.Uniform:
                {
                    bufferUsage = BufferUsageFlags.UniformBufferBit;
                    break;
                }
                case BufferUsage.Transfer:
                {
                    bufferUsage = BufferUsageFlags.TransferSrcBit;
                    break;
                }
            }
            
            bufferUsage |= BufferUsageFlags.TransferSrcBit;
            bufferUsage |= BufferUsageFlags.TransferDstBit;

            size = (ulong)(sizeof(T) * data.Length);
            BufferCreateInfo bufferInfo = new()
            {
                SType = StructureType.BufferCreateInfo,
                Size = size,
                Usage = bufferUsage,
                SharingMode = SharingMode.Exclusive,
            };

            fixed (Silk.NET.Vulkan.Buffer* bufferPtr = &buffer)
            {
                if (AppState.appContext.GetContext<VkContext>().vk.CreateBuffer(VkDevices.device, in bufferInfo, null, bufferPtr) != Result.Success)
                {
                    Debug.Error($"Failed to create buffer with type {typeof(T).Name}");
                    throw new Exception($"Failed to create buffer with type {typeof(T).Name}");
                }
            }

            MemoryRequirements memRequirements = new();
            AppState.appContext.GetContext<VkContext>().vk.GetBufferMemoryRequirements(VkDevices.device, buffer, out memRequirements);
            
            MemoryPropertyFlags memoryUsageFlag = MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit;
            switch (memoryUsage)
            {
                case BufferMemoryUsage.Cpu:
                {
                    memoryUsageFlag = MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit;
                    break;
                }
                case BufferMemoryUsage.Gpu:
                {
                    memoryUsageFlag = MemoryPropertyFlags.DeviceLocalBit;
                    break;
                }
            }
            
            MemoryAllocateInfo allocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memRequirements.Size,
                MemoryTypeIndex = FindMemoryType(memRequirements.MemoryTypeBits, memoryUsageFlag),
            };

            fixed (DeviceMemory* bufferMemoryPtr = &bufferMemory)
            {
                // TODO This has a limit so create a system so that we offset multiple buffers into the same allocation
                if (AppState.appContext.GetContext<VkContext>().vk.AllocateMemory(VkDevices.device, in allocateInfo, null, bufferMemoryPtr) != Result.Success)
                {
                    Debug.Error($"Failed to allocate buffer memory for type {typeof(T).Name}");
                    throw new Exception($"Failed to allocate buffer memory for type {typeof(T).Name}");
                }
            }

            AppState.appContext.GetContext<VkContext>().vk.BindBufferMemory(VkDevices.device, buffer, bufferMemory, 0);

            //only copy if on cpu
            if (memoryUsage == BufferMemoryUsage.Cpu)
            {
                void* ptr = memoryMap;
                AppState.appContext.GetContext<VkContext>().vk.MapMemory(VkDevices.device, bufferMemory, 0,  (ulong)(sizeof(T) * data.Length), 0, ref ptr);
                memoryMap = (byte*)ptr;
                data.CopyTo(new Span<T>(memoryMap, data.Length));
            }
            else if (memoryUsage == BufferMemoryUsage.Gpu)
            {
                //create a staging buffer
                VkBuffer<T> stagingBuffer = new VkBuffer<T>();
                stagingBuffer.Create(data, usage, BufferMemoryUsage.Cpu);
                stagingBuffer.CopyTo(this);
                stagingBuffer.Clean();
            }
        }
        
        public void Create(uint dataLength, BufferUsage usage, BufferMemoryUsage memoryUsage)
        {
            this.memoryUsage = memoryUsage;
            this.usage = usage;
            BufferUsageFlags bufferUsage = BufferUsageFlags.StorageBufferBit;
            switch (usage)
            {
                case BufferUsage.Vertex:
                {
                    bufferUsage = BufferUsageFlags.VertexBufferBit;
                    break;
                }
                case BufferUsage.Index:
                {
                    bufferUsage = BufferUsageFlags.IndexBufferBit;
                    break;
                }
                case BufferUsage.Storage:
                {

                    bufferUsage = BufferUsageFlags.StorageBufferBit;
                    break;
                }
                case BufferUsage.Uniform:
                {
                    bufferUsage = BufferUsageFlags.UniformBufferBit;
                    break;
                }
            }
            
            bufferUsage |= BufferUsageFlags.TransferSrcBit;
            bufferUsage |= BufferUsageFlags.TransferDstBit;

            size = (ulong)(sizeof(T) * dataLength);
            BufferCreateInfo bufferInfo = new()
            {
                SType = StructureType.BufferCreateInfo,
                Size = size,
                Usage = bufferUsage,
                SharingMode = SharingMode.Exclusive,
            };

            fixed (Silk.NET.Vulkan.Buffer* bufferPtr = &buffer)
            {
                if (AppState.appContext.GetContext<VkContext>().vk.CreateBuffer(VkDevices.device, in bufferInfo, null, bufferPtr) != Result.Success)
                {
                    Debug.Error($"Failed to create buffer with type {typeof(T).Name}");
                    throw new Exception($"Failed to create buffer with type {typeof(T).Name}");
                }
            }

            MemoryRequirements memRequirements = new();
            AppState.appContext.GetContext<VkContext>().vk.GetBufferMemoryRequirements(VkDevices.device, buffer, out memRequirements);
            
            MemoryPropertyFlags memoryUsageFlag = MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit;
            switch (memoryUsage)
            {
                case BufferMemoryUsage.Cpu:
                {
                    memoryUsageFlag = MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit;
                    break;
                }
                case BufferMemoryUsage.Gpu:
                {
                    memoryUsageFlag = MemoryPropertyFlags.DeviceLocalBit;
                    break;
                }
            }
            
            MemoryAllocateInfo allocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memRequirements.Size,
                MemoryTypeIndex = FindMemoryType(memRequirements.MemoryTypeBits, memoryUsageFlag),
            };

            fixed (DeviceMemory* bufferMemoryPtr = &bufferMemory)
            {
                // TODO This has a limit so create a system so that we offset multiple buffers into the same allocation
                if (AppState.appContext.GetContext<VkContext>().vk.AllocateMemory(VkDevices.device, in allocateInfo, null, bufferMemoryPtr) != Result.Success)
                {
                    Debug.Error($"Failed to allocate buffer memory for type {typeof(T).Name}");
                    throw new Exception($"Failed to allocate buffer memory for type {typeof(T).Name}");
                }
            }

            AppState.appContext.GetContext<VkContext>().vk.BindBufferMemory(VkDevices.device, buffer, bufferMemory, 0);
            
            //only copy if on cpu
            if (memoryUsage == BufferMemoryUsage.Cpu)
            {
                void* ptr = memoryMap;
                AppState.appContext.GetContext<VkContext>().vk.MapMemory(VkDevices.device, bufferMemory, 0,  (ulong)(sizeof(T) * dataLength), 0, ref ptr);
                memoryMap = (byte*)ptr;
                //AppState.appContext.GetContext<VkContext>().vk.UnmapMemory(VkDevices.device, bufferMemory);
            }
        }

        public void BindVertexBuffer(ICommandBufferDevice commandBufferDevice)
        {
            commandBufferDevice.BindVertexBuffers(this);
        }

        public void BindBuffer(ICommandBufferDevice commandBufferDevice)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IBufferDevice<T> dest)
        {
            CommandBuffer commandBuffer = new CommandBuffer();
            commandBuffer.Create();

            VkCommandBuffer vkCommandBuffer = (VkCommandBuffer)commandBuffer.commandBuffer!;
            commandBuffer.BeginOneUse();

            BufferCopy copyRegion = new()
            {
                Size = size,
            };

            AppState.appContext.GetContext<VkContext>().vk.CmdCopyBuffer(vkCommandBuffer.commandBuffer, buffer, ((VkBuffer<T>)dest).buffer, 1, in copyRegion);

            commandBuffer.End();

            SubmitInfo submitInfo;
            fixed (Silk.NET.Vulkan.CommandBuffer* cmdBufPtr = &vkCommandBuffer.commandBuffer)
            {
                submitInfo = new()
                {
                    SType = StructureType.SubmitInfo,
                    CommandBufferCount = 1,
                    PCommandBuffers = cmdBufPtr,
                };
            }

            // TODO Check if this should be in the graphics queue
            AppState.appContext.GetContext<VkContext>().vk.QueueSubmit(VkDevices.graphicsQueue, 1, in submitInfo, default);
            AppState.appContext.GetContext<VkContext>().vk.QueueWaitIdle(VkDevices.graphicsQueue);

            commandBuffer.Clean();
        }

        public void CopyToTexture(ITextureDevice dest, in TextureData srcData)
        {
            CommandBuffer commandBuffer = new CommandBuffer();
            commandBuffer.Create();
            commandBuffer.BeginOneUse();

            BufferImageCopy region = new()
            {
                BufferOffset = 0,
                BufferRowLength = 0,
                BufferImageHeight = 0,
                ImageSubresource =
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    MipLevel = 0,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },
                ImageOffset = new Offset3D(0, 0, 0),
                ImageExtent = new Extent3D(srcData.info.width, srcData.info.height, 1),
            };

            AppState.appContext.GetContext<VkContext>().vk.CmdCopyBufferToImage(
                ((VkCommandBuffer)commandBuffer.commandBuffer!).commandBuffer,
                buffer,
                ((VkTexture)dest).image,
                ImageLayout.TransferDstOptimal,
                1,
                in region);

            commandBuffer.EndSubmitClean();
        }

        /// <summary>
        /// Will use information stored in the destination data
        /// </summary>
        public void TextureToCopy(ITextureDevice src, in TextureData destData)
        {
            CommandBuffer commandBuffer = new CommandBuffer();
            commandBuffer.Create();
            commandBuffer.BeginOneUse();
            
            BufferImageCopy region = new()
            {
                BufferOffset = 0,
                BufferRowLength = 0,
                BufferImageHeight = 0,
                ImageSubresource =
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    MipLevel = 0,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                },
                ImageOffset = new Offset3D(0, 0, 0),
                ImageExtent = new Extent3D(destData.info.width, destData.info.height, 1),
            };
            
            AppState.appContext.GetContext<VkContext>().vk.CmdCopyImageToBuffer(
                ((VkCommandBuffer)commandBuffer.commandBuffer!).commandBuffer,
                ((VkTexture)src).image,
                ImageLayout.TransferSrcOptimal,
                buffer,
                1,
                in region);
            
            commandBuffer.EndSubmitClean();
        }

        public T[] CopyToArray()
        {
            T[] data = new T[size / (ulong)sizeof(T)];
            if (memoryUsage == BufferMemoryUsage.Gpu)
            {
                VkBuffer<T> stagingBuffer = new VkBuffer<T>();
                stagingBuffer.Create(new Span<T>(data), BufferUsage.Transfer, BufferMemoryUsage.Cpu);
                CopyTo(stagingBuffer);
                fixed (T* dst = data)
                {
                    System.Buffer.MemoryCopy((T*)stagingBuffer.memoryMap,dst, size,size);
                }
                stagingBuffer.Clean();
            }
            else
            {
                fixed (T* dst = data)
                {
                    System.Buffer.MemoryCopy((T*)memoryMap,dst, size,size);
                }
            }

            return data;
        }

        uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties)
        {
            AppState.appContext.GetContext<VkContext>().vk.GetPhysicalDeviceMemoryProperties(VkDevices.physicalDevice, out PhysicalDeviceMemoryProperties memProperties);

            for (int i = 0; i < memProperties.MemoryTypeCount; i++)
            {
                if ((typeFilter & (1 << i)) != 0 &&
                    (memProperties.MemoryTypes[i].PropertyFlags & properties) == properties)
                {
                    return (uint)i;
                }
            }
            
            Debug.Error($"Failed to find suitable memory type for type {typeof(T).Name}");
            throw new Exception($"Failed to find suitable memory type for type {typeof(T).Name}");
        }

        public void UpdateData(in Span<T> data)
        {
            if (data.Length > (int)size / sizeof(T))
            {
                Debug.Warning($"Could not upload type {typeof(T).Name} array to buffer, past maximum allocated size of {(int)size / sizeof(T)}");
                return;
            }

            if (memoryUsage == BufferMemoryUsage.Cpu)
            {
                data.CopyTo(new Span<T>(memoryMap, data.Length));
            }
        }
        
        public void UpdateUniformData(in Span<T> data)
        {
            uint blockSize = VkUniformManager.PadUniformSize((uint)(data.Length * sizeof(T)));
            if (blockSize + memoryOffset > (int)size)
            {
                Debug.Warning($"Could not upload type {typeof(T).Name} array to buffer, past maximum allocated size of {(int)size / sizeof(T)}");
                return;
            }

            drawOffset = memoryOffset;
            
            byte* dst = memoryMap + memoryOffset;
            data.CopyTo(new Span<T>(dst, data.Length));
            memoryOffset += blockSize;
        }

        public ulong GetBufferDeviceAddress()
        {
            BufferDeviceAddressInfo info = new()
            {
                SType = StructureType.BufferDeviceAddressInfo,
                Buffer = buffer
            };

            return AppState.appContext.GetContext<VkContext>().vk.GetBufferDeviceAddress(VkDevices.device, &info);
        }

        public void Clean()
        {
            AppState.appContext.GetContext<VkContext>().vk.DeviceWaitIdle(VkDevices.device);
            
            //if this buffer is visible to the cpu unmap the pointer to the memory
            if(memoryUsage == BufferMemoryUsage.Cpu)
                AppState.appContext.GetContext<VkContext>().vk.UnmapMemory(VkDevices.device, bufferMemory);
            AppState.appContext.GetContext<VkContext>().vk.DestroyBuffer(VkDevices.device, buffer, null);
            AppState.appContext.GetContext<VkContext>().vk.FreeMemory(VkDevices.device, bufferMemory, null);
        }
    }
}