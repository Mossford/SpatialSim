using System.Numerics;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
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
            AppState.appContext.GetContext<VkContext>().vk.DeviceWaitIdle(VkDevices.device);
            
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

        public void Begin()
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
        
        public void BeginOneUse()
        {
            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmitBit
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

        public unsafe void Submit()
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

        public unsafe void BeingRendering(int frame)
        {
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
            
            RenderingAttachmentInfo colorAttachmentInfo = new()
            {
                SType = StructureType.RenderingAttachmentInfo,
                ImageView = VkSwapChain.swapChainImageViews[frame],
                ImageLayout = ImageLayout.ColorAttachmentOptimal,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                ClearValue = clearValues[0],
                ResolveMode = ResolveModeFlags.None
            };

            RenderingAttachmentInfo depthAttachmentInfo = new()
            {
                SType = StructureType.RenderingAttachmentInfo,
                ImageView = VkDepthBuffer.texture.imageView,
                ImageLayout = ImageLayout.DepthStencilAttachmentOptimal,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.DontCare,
                ClearValue = clearValues[1],
                ResolveMode = ResolveModeFlags.None
            };
            
            RenderingInfo renderingInfo = new()
            {
                SType = StructureType.RenderingInfo,
                RenderArea = new Rect2D(new Offset2D(0, 0), VkSwapChain.swapChainExtent),
                LayerCount = 1,
                ColorAttachmentCount = 1,
                PColorAttachments = &colorAttachmentInfo,
                PDepthAttachment = &depthAttachmentInfo
            };
            
            VkDevices.dynamicRendering.CmdBeginRendering(commandBuffer, &renderingInfo);
        }

        public void EndRendering()
        {
            VkDevices.dynamicRendering.CmdEndRendering(commandBuffer);
        }

        public void BindPipeLine(Pipeline pipeline)
        {
            AppState.appContext.GetContext<VkContext>().vk.CmdBindPipeline(
                commandBuffer, 
                PipelineBindPoint.Graphics, 
                ((VkPipeline)pipeline.pipeline!).pipeline);
        }

        public void SetViewport(Vector2 size)
        {
            Viewport viewport = new()
            {
                X = 0,
                Y = 0,
                Width = size.X,
                Height = size.Y,
                MaxDepth = 1,
                MinDepth = 0
            };
            AppState.appContext.GetContext<VkContext>().vk.CmdSetViewport(commandBuffer, 0, [viewport]);
        }

        public void SetScissor(Vector2 size)
        {
            Rect2D scissor = new()
            {
                Extent =
                {
                    Width = (uint)size.X,
                    Height = (uint)size.Y
                },
                Offset =
                {
                    X = 0,
                    Y = 0
                }
            };
            AppState.appContext.GetContext<VkContext>().vk.CmdSetScissor(commandBuffer, 0, [scissor]);
        }

        public unsafe void BindVertexUniforms(Pipeline pipeline, int binding)
        {
            VkPipeline vkPipeLine = ((VkPipeline)pipeline.pipeline!);

            ShaderDescriptorDef def = new ShaderDescriptorDef(
                RendererSettings.VertexUniformSet, 
                [binding], 
                ShaderDescriptorUsage.Uniform, 
                ShaderType.Vertex);
            DescriptorSet set = vkPipeLine.uniformManager.GetDescriptorSet(def);
            //data will be 0 so we just grab the current buffer
            uint dynamicOffset = ((VkBuffer<byte>)vkPipeLine.uniformManager.GetBuffer(def).buffer!).drawOffset;

            //bind only the descriptor set with the one buffer attached vulkan will auto increment
            AppState.appContext.GetContext<VkContext>().vk.CmdBindDescriptorSets(
                commandBuffer,
                PipelineBindPoint.Graphics,
                vkPipeLine.pipelineLayout,
                RendererSettings.VertexUniformSet,
                1,
                &set,
                1,
                in dynamicOffset);
        }
        
        public unsafe void BindFragmentUniforms(Pipeline pipeline, int binding)
        {
            VkPipeline vkPipeLine = ((VkPipeline)pipeline.pipeline!);
            
            ShaderDescriptorDef def = new ShaderDescriptorDef(
                RendererSettings.FragmentUniformSet, 
                [binding], 
                ShaderDescriptorUsage.Uniform, 
                ShaderType.Fragment);
            DescriptorSet set = vkPipeLine.uniformManager.GetDescriptorSet(def);
            uint dynamicOffset = ((VkBuffer<byte>)vkPipeLine.uniformManager.GetBuffer(def).buffer!).drawOffset;

            //bind only the descriptor set with the one buffer attached vulkan will auto increment
            AppState.appContext.GetContext<VkContext>().vk.CmdBindDescriptorSets(
                commandBuffer,
                PipelineBindPoint.Graphics,
                vkPipeLine.pipelineLayout,
                RendererSettings.FragmentUniformSet,
                1,
                &set,
                1,
                in dynamicOffset);
        }
        
        public unsafe void BindSamplers(Pipeline pipeline, Texture[] textures, int[] bindings, ShaderType shaderType)
        {
            VkPipeline vkPipeLine = ((VkPipeline)pipeline.pipeline!);
            
            for (int i = 0; i < textures.Length; i++)
            {
                // TODO this binds to the fragment sampler set which should change based on the shader type

                ShaderDescriptorDef def = new (RendererSettings.FragmentSamplerSet, bindings, ShaderDescriptorUsage.Sampler, shaderType);
                
                VkTexture texture = ((VkTexture)textures[i].texture);
                VkDescriptor set = vkPipeLine.GetDescriptor(def);
                texture.SetTextureToDescriptorSet(set, bindings[i]);
                
                AppState.appContext.GetContext<VkContext>().vk.CmdBindDescriptorSets(
                    commandBuffer,
                    PipelineBindPoint.Graphics,
                    vkPipeLine.pipelineLayout,
                    RendererSettings.FragmentSamplerSet,
                    1,
                    ref set.descriptorSet,
                    0,
                    null);
            }
        }

        /// <summary>
        /// Currently only resets the uniform buffers
        /// </summary>
        public void ResetPipeLine(Pipeline pipeline)
        {
            VkPipeline vkPipeLine = ((VkPipeline)pipeline.pipeline!);
            vkPipeLine.uniformManager.ResetUniforms();
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