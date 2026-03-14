using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public class VkCommandBuffer : ICommandBufferDevice
    {
        public CommandPool commandPool;
        bool ownsCommandPool;
        public Silk.NET.Vulkan.CommandBuffer commandBuffer;
        bool submittedCommandBuffer;
        
        public unsafe void Create()
        {
            CreateCommandPool();
            commandBuffer = new Silk.NET.Vulkan.CommandBuffer();

            CommandBufferAllocateInfo allocInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = commandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1,
            };

            fixed (Silk.NET.Vulkan.CommandBuffer* commandBuffersPtr = &commandBuffer)
            {
                Result result = AppState.appContext.GetContext<VkContext>().vk
                    .AllocateCommandBuffers(VkDevices.device, in allocInfo, commandBuffersPtr);
                if (result != Result.Success)
                {
                    Debug.Error($"Failed to allocate command buffers {result}");
                    throw new Exception($"Failed to allocate command buffers {result}");
                }
            }
            
            ownsCommandPool = true;

            Ticks.commandBufferCount.created++;
        }
        
        public unsafe void Create(CommandPool commandPool)
        {
            commandBuffer = new Silk.NET.Vulkan.CommandBuffer();

            this.commandPool = commandPool;

            CommandBufferAllocateInfo allocInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = commandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1,
            };

            fixed (Silk.NET.Vulkan.CommandBuffer* commandBuffersPtr = &commandBuffer)
            {
                Result result = AppState.appContext.GetContext<VkContext>().vk
                    .AllocateCommandBuffers(VkDevices.device, in allocInfo, commandBuffersPtr);
                if (result != Result.Success)
                {
                    Debug.Error($"Failed to allocate command buffers {result}");
                    throw new Exception($"Failed to allocate command buffers {result}");
                }
            }
            
            Ticks.commandBufferCount.created++;

            ownsCommandPool = false;
        }
        
        public unsafe void Clean()
        {
            if (!submittedCommandBuffer)
            {
                fixed (Silk.NET.Vulkan.CommandBuffer* commandBuffersPtr = &commandBuffer)
                {
                    AppState.appContext.GetContext<VkContext>().vk.FreeCommandBuffers(
                        VkDevices.device,
                        commandPool, 
                        1, 
                        commandBuffersPtr);
                }
            }
            
            if(ownsCommandPool)
                AppState.appContext.GetContext<VkContext>().vk.DestroyCommandPool(VkDevices.device, commandPool, null);
            
            Ticks.commandBufferCount.deleted++;
        }

        public unsafe void BindVertexBuffers<T>(IBufferDevice<T> bufferDevice) where T : unmanaged
        {
            ulong[] offsets = new ulong[] { 0 };

            fixed (ulong* offsetsPtr = offsets)
            {
                fixed (Silk.NET.Vulkan.Buffer* bufferPtr = &((VkBuffer<T>)bufferDevice).buffer)
                {
                    AppState.appContext.GetContext<VkContext>().vk.CmdBindVertexBuffers(commandBuffer, 0, 1, bufferPtr, offsetsPtr);
                }
            }
        }
        
        public unsafe void BindIndexBuffers<T>(IBufferDevice<T> bufferDevice) where T : unmanaged
        {
            ulong[] offsets = new ulong[] { 0 };

            fixed (ulong* offsetsPtr = offsets)
            {
                AppState.appContext.GetContext<VkContext>().vk.CmdBindIndexBuffer(commandBuffer, ((VkBuffer<T>)bufferDevice).buffer, 0, IndexType.Uint32);
            }
        }

        public unsafe void CreateCommandPool()
        {
            ownsCommandPool = true;
            
            VkDevices.QueueFamilyIndices queueFamiliyIndicies = VkDevices.FindQueueFamilies(VkDevices.physicalDevice);

            CommandPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = queueFamiliyIndicies.GraphicsFamily!.Value,
                Flags = CommandPoolCreateFlags.ResetCommandBufferBit
            };

            Result result = AppState.appContext.GetContext<VkContext>().vk
                .CreateCommandPool(VkDevices.device, in poolInfo, null, out commandPool);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to create command pool {result}");
                throw new Exception($"Failed to create command pool {result}");
            }
        }

        public void BeginCommandBuffer()
        {
            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.SimultaneousUseBit
            };

            Result result = AppState.appContext.GetContext<VkContext>().vk
                .BeginCommandBuffer(commandBuffer, in beginInfo);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to begin recording command buffer {result}");
                throw new Exception($"Failed to begin recording command buffer {result}");
            }
        }

        public void EndCommandBuffer()
        {
            Result result = AppState.appContext.GetContext<VkContext>().vk.EndCommandBuffer(commandBuffer);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to record command buffer {result}");
                throw new Exception($"Failed to record command buffer {result}");
            }
        }

        public unsafe void SubmitCommandBuffer()
        {
            submittedCommandBuffer = true;
            
            fixed (Silk.NET.Vulkan.CommandBuffer* cmdPtr = &commandBuffer)
            {
                SubmitInfo submitInfo = new()
                {
                    SType = StructureType.SubmitInfo,
                    CommandBufferCount = 1,
                    PCommandBuffers = cmdPtr,
                };
                
                AppState.appContext.GetContext<VkContext>().vk.QueueSubmit(VkDevices.graphicsQueue, 1, in submitInfo, default);
                AppState.appContext.GetContext<VkContext>().vk.QueueWaitIdle(VkDevices.graphicsQueue);

                AppState.appContext.GetContext<VkContext>().vk.FreeCommandBuffers(VkDevices.device, commandPool, 1, in commandBuffer);
            }
        }

        public unsafe void BeginRenderPass(int frame)
        {
            RenderPassBeginInfo renderPassInfo = new()
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = ((VkRenderPass)AppState.appContext.renderPass).renderPass,
                Framebuffer = VkSwapChain.swapChainFramebuffers[frame],
                RenderArea =
                {
                    Offset = { X = 0, Y = 0 },
                    Extent = VkSwapChain.swapChainExtent,
                }
            };
            
            ClearValue[] clearValues = new ClearValue[]
            {
                new()
                {
                    Color = new ()
                    {
                        Float32_0 = 0, 
                        Float32_1 = 0, 
                        Float32_2 = 0, 
                        Float32_3 = 1
                    },
                },
                new()
                {
                    DepthStencil = new ()
                    {
                        Depth = 1, 
                        Stencil = 0
                    }
                }
            };

            renderPassInfo.ClearValueCount = (uint)clearValues.Length;
            fixed (ClearValue* clearValuesPtr = clearValues)
                renderPassInfo.PClearValues = clearValuesPtr;
            
            AppState.appContext.GetContext<VkContext>().vk.CmdBeginRenderPass(commandBuffer, &renderPassInfo, SubpassContents.Inline);
        }

        public void BindPipeLine(Pipeline pipeline)
        {
            AppState.appContext.GetContext<VkContext>().vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, ((VkPipeline)pipeline.pipeline!).pipeline);
        }

        public unsafe void BindUniforms(Pipeline pipeline)
        {
            VkPipeline vkPipeLine = ((VkPipeline)pipeline.pipeline!);
            
            DescriptorSet set = vkPipeLine.uniformManager.GetDescriptorSet();
            uint dynamicOffset = ((VkBuffer<byte>)vkPipeLine.uniformManager.uniformBuffers[vkPipeLine.uniformManager.currentBuffer].buffer!).drawOffset;

            //bind only the descriptor set with the one buffer attached vulkan will auto increment
            AppState.appContext.GetContext<VkContext>().vk.CmdBindDescriptorSets(
                commandBuffer,
                PipelineBindPoint.Graphics,
                vkPipeLine.pipelineLayout,
                VkSettings.UniformSet,
                1,
                &set,
                1,
                &dynamicOffset);
        }
        
        public unsafe void BindTexture(Pipeline pipeline, Texture texture)
        {
            VkPipeline vkPipeLine = ((VkPipeline)pipeline.pipeline!);
            VkTexture vkTexture = ((VkTexture)texture.texture);
            
            DescriptorSet set = vkTexture.descriptor.descriptorSet;

            //bind only the descriptor set with the one buffer attached vulkan will auto increment
            AppState.appContext.GetContext<VkContext>().vk.CmdBindDescriptorSets(
                commandBuffer,
                PipelineBindPoint.Graphics,
                vkPipeLine.pipelineLayout,
                VkSettings.SamplerSet,
                1,
                &set,
                0,
                null);
        }

        /// <summary>
        /// Currently only resets the uniform buffers
        /// </summary>
        public void ResetPipeLine(Pipeline pipeline)
        {
            VkPipeline vkPipeLine = ((VkPipeline)pipeline.pipeline!);
            for (int i = 0; i < vkPipeLine.uniformManager.uniformBuffers.Count; i++)
            {
                ((VkBuffer<byte>)vkPipeLine.uniformManager.uniformBuffers[i].buffer!).drawOffset = 0;
                ((VkBuffer<byte>)vkPipeLine.uniformManager.uniformBuffers[i].buffer!).memoryOffset = 0;
            }

            vkPipeLine.uniformManager.currentBuffer = 0;
        }

        public void Draw(int indexCount)
        {
            AppState.appContext.GetContext<VkContext>().vk.CmdDrawIndexed(commandBuffer, (uint)indexCount, 1, 0, 0, 0);
        }

        public void EndRenderPass()
        {
            AppState.appContext.GetContext<VkContext>().vk.CmdEndRenderPass(commandBuffer);
        }
    }
}