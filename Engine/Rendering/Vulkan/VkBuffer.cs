using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public class VkBuffer<T> : IBufferDevice<T> where T : unmanaged
    {
        public Silk.NET.Vulkan.Buffer buffer;
        public DeviceMemory bufferMemory;
        public ulong size;
        
        public unsafe void Create(in Span<T> data, BufferUsage usage, BufferMemoryUsage memoryUsage)
        {
            BufferUsageFlags bufferUsage = BufferUsageFlags.StorageBufferBit;
            switch (usage)
            {
                case BufferUsage.Vertex:
                {
                    switch (memoryUsage)
                    {
                        case BufferMemoryUsage.Cpu:
                        {
                            //if we are on cpu this buffer will always act as a source buffer
                            bufferUsage = BufferUsageFlags.VertexBufferBit | BufferUsageFlags.TransferSrcBit;
                            break;
                        }
                        case BufferMemoryUsage.Gpu:
                        {
                            //if we are on gpu this buffer will always be able to be written too
                            bufferUsage = BufferUsageFlags.VertexBufferBit | BufferUsageFlags.TransferDstBit;
                            break;
                        }
                    }
                    break;
                }
                case BufferUsage.Index:
                {
                    switch (memoryUsage)
                    {
                        case BufferMemoryUsage.Cpu:
                        {
                            //if we are on cpu this buffer will always act as a source buffer
                            bufferUsage = BufferUsageFlags.IndexBufferBit | BufferUsageFlags.TransferSrcBit;
                            break;
                        }
                        case BufferMemoryUsage.Gpu:
                        {
                            //if we are on gpu this buffer will always be able to be written too
                            bufferUsage = BufferUsageFlags.IndexBufferBit | BufferUsageFlags.TransferDstBit;
                            break;
                        }
                    }
                    break;
                }
                case BufferUsage.Storage:
                {
                    switch (memoryUsage)
                    {
                        case BufferMemoryUsage.Cpu:
                        {
                            //if we are on cpu this buffer will always act as a source buffer
                            bufferUsage = BufferUsageFlags.StorageBufferBit | BufferUsageFlags.TransferSrcBit;
                            break;
                        }
                        case BufferMemoryUsage.Gpu:
                        {
                            //if we are on gpu this buffer will always be able to be written too
                            bufferUsage = BufferUsageFlags.StorageBufferBit | BufferUsageFlags.TransferDstBit;
                            break;
                        }
                    }
                    break;
                }
            }

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
                void* bufferData;
                AppState.appContext.GetContext<VkContext>().vk.MapMemory(VkDevices.device, bufferMemory, 0, bufferInfo.Size, 0, &bufferData);
                data.CopyTo(new Span<T>(bufferData, data.Length));
                AppState.appContext.GetContext<VkContext>().vk.UnmapMemory(VkDevices.device, bufferMemory);
            }
            
            Debug.LogInfo($"Created buffer of type {typeof(T).Name}");
        }
        
        public unsafe void Create(int dataLength, BufferUsage usage, BufferMemoryUsage memoryUsage)
        {
            BufferUsageFlags bufferUsage = BufferUsageFlags.StorageBufferBit;
            switch (usage)
            {
                case BufferUsage.Vertex:
                {
                    switch (memoryUsage)
                    {
                        case BufferMemoryUsage.Cpu:
                        {
                            //if we are on cpu this buffer will always act as a source buffer
                            bufferUsage = BufferUsageFlags.VertexBufferBit | BufferUsageFlags.TransferSrcBit;
                            break;
                        }
                        case BufferMemoryUsage.Gpu:
                        {
                            //if we are on gpu this buffer will always be able to be written too
                            bufferUsage = BufferUsageFlags.VertexBufferBit | BufferUsageFlags.TransferDstBit;
                            break;
                        }
                    }
                    break;
                }
                case BufferUsage.Index:
                {
                    switch (memoryUsage)
                    {
                        case BufferMemoryUsage.Cpu:
                        {
                            //if we are on cpu this buffer will always act as a source buffer
                            bufferUsage = BufferUsageFlags.IndexBufferBit | BufferUsageFlags.TransferSrcBit;
                            break;
                        }
                        case BufferMemoryUsage.Gpu:
                        {
                            //if we are on gpu this buffer will always be able to be written too
                            bufferUsage = BufferUsageFlags.IndexBufferBit | BufferUsageFlags.TransferDstBit;
                            break;
                        }
                    }
                    break;
                }
                case BufferUsage.Storage:
                {
                    switch (memoryUsage)
                    {
                        case BufferMemoryUsage.Cpu:
                        {
                            //if we are on cpu this buffer will always act as a source buffer
                            bufferUsage = BufferUsageFlags.StorageBufferBit | BufferUsageFlags.TransferSrcBit;
                            break;
                        }
                        case BufferMemoryUsage.Gpu:
                        {
                            //if we are on gpu this buffer will always be able to be written too
                            bufferUsage = BufferUsageFlags.StorageBufferBit | BufferUsageFlags.TransferDstBit;
                            break;
                        }
                    }
                    break;
                }
            }

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
            
            Debug.LogInfo($"Created buffer of type {typeof(T).Name}");
        }

        public void BindVertexBuffer(ICommandBufferDevice commandBufferDevice)
        {
            commandBufferDevice.BindVertexBuffers(this);
        }

        public void BindBuffer(ICommandBufferDevice commandBufferDevice)
        {
            throw new NotImplementedException();
        }

        public unsafe void CopyTo(IBufferDevice<T> dest)
        {
            CommandBuffer commandBuffer = new CommandBuffer();
            commandBuffer.Create();

            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
            };

            VkCommandBuffer vkCommandBuffer = (VkCommandBuffer)commandBuffer.commandBuffer!;
            AppState.appContext.GetContext<VkContext>().vk.BeginCommandBuffer(vkCommandBuffer.commandBuffer, in beginInfo);

            BufferCopy copyRegion = new()
            {
                Size = size,
            };

            AppState.appContext.GetContext<VkContext>().vk.CmdCopyBuffer(vkCommandBuffer.commandBuffer, buffer, ((VkBuffer<T>)dest).buffer, 1, in copyRegion);

            AppState.appContext.GetContext<VkContext>().vk.EndCommandBuffer(vkCommandBuffer.commandBuffer);

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

            AppState.appContext.GetContext<VkContext>().vk.QueueSubmit(VkDevices.graphicsQueue, 1, in submitInfo, default);
            AppState.appContext.GetContext<VkContext>().vk.QueueWaitIdle(VkDevices.graphicsQueue);

            commandBuffer.Clean();
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

        public void Bind()
        {
            throw new NotImplementedException();
        }

        public unsafe void Clean()
        {
            AppState.appContext.GetContext<VkContext>().vk.DestroyBuffer(VkDevices.device, buffer, null);
            AppState.appContext.GetContext<VkContext>().vk.FreeMemory(VkDevices.device, bufferMemory, null);
            
            Debug.LogInfo("Cleaned up Buffer");
        }
    }
}