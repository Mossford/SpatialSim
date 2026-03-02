using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    public class VkPipeline : IPipelineDevice
    {
        public PipelineLayout pipelineLayout;
        public Silk.NET.Vulkan.Pipeline pipeline;
        public DescriptorSetLayout descriptorSetLayout;
        public DescriptorSet[] descriptorSets;
        public DescriptorPool descriptorPool;
        public Buffer<byte>[] uniformBuffers;
        
        public unsafe void Create(in Shader vertex, in Shader fragment)
        {
            // TODO Make the shader provide this information
            DescriptorSetLayoutBinding uboLayoutBinding = new()
            {
                Binding = 0,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBuffer,
                PImmutableSamplers = null,
                StageFlags = ShaderStageFlags.VertexBit,
            };

            DescriptorSetLayoutCreateInfo layoutInfo = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 1,
                PBindings = &uboLayoutBinding,
            };

            fixed (DescriptorSetLayout* descriptorSetLayoutPtr = &descriptorSetLayout)
            {
                if (AppState.appContext.GetContext<VkContext>().vk.CreateDescriptorSetLayout(VkDevices.device, in layoutInfo, null, descriptorSetLayoutPtr) != Result.Success)
                {
                    Debug.Error("Failed to create descriptor set layout");
                    throw new Exception("Failed to create descriptor set layout");
                }
            }
            
            PipelineShaderStageCreateInfo vertShaderStageInfo = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.VertexBit,
                Module = ((VkShader)vertex.shader).program,
                PName = (byte*)SilkMarshal.StringToPtr("main")
            };

            PipelineShaderStageCreateInfo fragShaderStageInfo = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.FragmentBit,
                Module = ((VkShader)fragment.shader).program,
                PName = (byte*)SilkMarshal.StringToPtr("main")
            };

            PipelineShaderStageCreateInfo* shaderStages = stackalloc[]
            {
                vertShaderStageInfo,
                fragShaderStageInfo
            };

            // TODO move this out of here

            VertexInputBindingDescription bindingDescription = new()
            {
                Binding = 0,
                Stride = (uint)Unsafe.SizeOf<Vertex>(),
                InputRate = VertexInputRate.Vertex,
            };
            
            VertexInputAttributeDescription[] attributeDescriptions = new[]
            {
                new VertexInputAttributeDescription()
                {
                    Binding = 0,
                    Location = 0,
                    Format = Format.R32G32B32Sfloat,
                    Offset = (uint)Marshal.OffsetOf<Vertex>(nameof(Vertex.position)),
                },
                new VertexInputAttributeDescription()
                {
                    Binding = 0,
                    Location = 1,
                    Format = Format.R32G32B32Sfloat,
                    Offset = (uint)Marshal.OffsetOf<Vertex>(nameof(Vertex.normal)),
                },
                new VertexInputAttributeDescription()
                {
                    Binding = 0,
                    Location = 2,
                    Format = Format.R32G32Sfloat,
                    Offset = (uint)Marshal.OffsetOf<Vertex>(nameof(Vertex.uv)),
                }
            };

            PipelineVertexInputStateCreateInfo vertexInputInfo;
            fixed (VertexInputAttributeDescription* attributeDescriptionsPtr = attributeDescriptions)
            {
                vertexInputInfo = new()
                {
                    SType = StructureType.PipelineVertexInputStateCreateInfo,
                    VertexBindingDescriptionCount = 1,
                    VertexAttributeDescriptionCount = (uint)attributeDescriptions.Length,
                    PVertexBindingDescriptions = &bindingDescription,
                    PVertexAttributeDescriptions = attributeDescriptionsPtr,
                };
            }

            PipelineInputAssemblyStateCreateInfo inputAssembly = new()
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = PrimitiveTopology.TriangleList,
                PrimitiveRestartEnable = false,
            };

            Viewport viewport = new()
            {
                X = 0,
                Y = 0,
                Width = VkSwapChain.swapChainExtent.Width,
                Height = VkSwapChain.swapChainExtent.Height,
                MinDepth = 0,
                MaxDepth = 1,
            };

            Rect2D scissor = new()
            {
                Offset =
                {
                    X = 0, 
                    Y = 0
                },
                Extent = VkSwapChain.swapChainExtent,
            };

            PipelineViewportStateCreateInfo viewportState = new()
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                PViewports = &viewport,
                ScissorCount = 1,
                PScissors = &scissor,
            };

            PipelineRasterizationStateCreateInfo rasterizer = new()
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                DepthClampEnable = false,
                RasterizerDiscardEnable = false,
                PolygonMode = PolygonMode.Fill,
                LineWidth = 1,
                CullMode = CullModeFlags.BackBit,
                FrontFace = FrontFace.Clockwise,
                DepthBiasEnable = false,
            };

            PipelineMultisampleStateCreateInfo multisampling = new()
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                SampleShadingEnable = false,
                RasterizationSamples = SampleCountFlags.Count1Bit,
            };

            PipelineColorBlendAttachmentState colorBlendAttachment = new()
            {
                ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit,
                BlendEnable = false,
            };

            PipelineColorBlendStateCreateInfo colorBlending = new()
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                LogicOpEnable = false,
                LogicOp = LogicOp.Copy,
                AttachmentCount = 1,
                PAttachments = &colorBlendAttachment,
            };

            colorBlending.BlendConstants[0] = 0;
            colorBlending.BlendConstants[1] = 0;
            colorBlending.BlendConstants[2] = 0;
            colorBlending.BlendConstants[3] = 0;

            PipelineLayoutCreateInfo pipelineLayoutInfo;
            fixed (DescriptorSetLayout* layoutPtr = &descriptorSetLayout)
            {
                pipelineLayoutInfo = new()
                {
                    SType = StructureType.PipelineLayoutCreateInfo,
                    SetLayoutCount = 1,
                    PushConstantRangeCount = 0,
                    PSetLayouts = layoutPtr
                };
            }

            if (AppState.appContext.GetContext<VkContext>().vk.CreatePipelineLayout(VkDevices.device, in pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
            {
                Debug.Error("Failed to create pipeline layout");
                throw new Exception("Failed to create pipeline layout");
            }
                
            GraphicsPipelineCreateInfo pipelineInfo = new()
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2,
                PStages = shaderStages,
                PVertexInputState = &vertexInputInfo,
                PInputAssemblyState = &inputAssembly,
                PViewportState = &viewportState,
                PRasterizationState = &rasterizer,
                PMultisampleState = &multisampling,
                PColorBlendState = &colorBlending,
                Layout = pipelineLayout,
                RenderPass = ((VkRenderPass)AppState.appContext.renderPass).renderPass,
                Subpass = 0,
                BasePipelineHandle = default
            };

            if (AppState.appContext.GetContext<VkContext>().vk.CreateGraphicsPipelines(VkDevices.device, default, 1, in pipelineInfo, null, out pipeline) != Result.Success)
            {
                Debug.Error("Failed to create graphics pipeline");
                throw new Exception("Failed to create graphics pipeline");
            }

            SilkMarshal.Free((nint)vertShaderStageInfo.PName);
            SilkMarshal.Free((nint)fragShaderStageInfo.PName);

            CreateUniformBuffers();
            CreateDescriptorSets();
            
            Debug.LogInfo("Successful pipeline creation");
        }

        public void CreateUniformBuffers()
        {
            //create a copy for each swapchain we have
            uniformBuffers = new Buffer<byte>[VkSwapChain.swapChainImages.Length];
            for (int i = 0; i < uniformBuffers.Length; i++)
            {
                uniformBuffers[i] = new Buffer<byte>();
                uniformBuffers[i].Create(Shader.MaxUniformMemory, BufferUsage.Uniform, BufferMemoryUsage.Cpu);
            }
        }

        unsafe void CreateDescriptorSets()
        {
            //create a copy for each swapchain we have
            descriptorSets = new DescriptorSet[VkSwapChain.swapChainImages.Length];
            
            DescriptorPoolSize poolSize = new()
            {
                Type = DescriptorType.UniformBuffer,
                DescriptorCount = (uint)descriptorSets.Length,
            };

            DescriptorPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = 1,
                PPoolSizes = &poolSize,
                MaxSets = (uint)descriptorSets.Length,
            };

            fixed (DescriptorPool* descriptorPoolPtr = &descriptorPool)
            {
                if (AppState.appContext.GetContext<VkContext>().vk.CreateDescriptorPool(
                        VkDevices.device, in poolInfo, null, descriptorPoolPtr) != Result.Success)
                {
                    throw new Exception("failed to create descriptor pool!");
                }

            }
            
            DescriptorSetLayout[] layouts = new DescriptorSetLayout[descriptorSets.Length];
            Array.Fill(layouts, descriptorSetLayout);

            fixed (DescriptorSetLayout* layoutsPtr = layouts)
            {
                DescriptorSetAllocateInfo allocateInfo = new()
                {
                    SType = StructureType.DescriptorSetAllocateInfo,
                    DescriptorPool = descriptorPool,
                    DescriptorSetCount = (uint)descriptorSets.Length,
                    PSetLayouts = layoutsPtr,
                };

                descriptorSets = new DescriptorSet[descriptorSets.Length];
                fixed (DescriptorSet* descriptorSetsPtr = descriptorSets)
                {
                    if (AppState.appContext.GetContext<VkContext>().vk.AllocateDescriptorSets(
                            VkDevices.device, in allocateInfo, descriptorSetsPtr) != Result.Success)
                    {
                        throw new Exception("failed to allocate descriptor sets!");
                    }
                }
            }


            for (int i = 0; i < descriptorSets.Length; i++)
            {
                DescriptorBufferInfo bufferInfo = new()
                {
                    Buffer = ((VkBuffer<byte>)uniformBuffers[i].buffer!).buffer,
                    Offset = 0,
                    Range = uniformBuffers[i].size,

                };

                WriteDescriptorSet descriptorWrite = new()
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSets[i],
                    DstBinding = 0,
                    DstArrayElement = 0,
                    DescriptorType = DescriptorType.UniformBuffer,
                    DescriptorCount = 1,
                    PBufferInfo = &bufferInfo,
                };

                AppState.appContext.GetContext<VkContext>().vk.UpdateDescriptorSets(VkDevices.device, 1, in descriptorWrite, 0, null);
            }
        }

        public void UpdateUniforms(in Shader shader, int frame)
        {
            uniformBuffers[frame].UpdateData(new Span<byte>(shader.uniformData.ToArray()));
        }

        public void Bind()
        {
            
        }

        public unsafe void Clean()
        {
            for (int i = 0; i < uniformBuffers.Length; i++)
            {
                uniformBuffers[i].Clean();
            }
            
            AppState.appContext.GetContext<VkContext>().vk.DestroyDescriptorPool(VkDevices.device, descriptorPool, null);
            
            AppState.appContext.GetContext<VkContext>().vk.DestroyPipeline(VkDevices.device, pipeline, null);
            AppState.appContext.GetContext<VkContext>().vk.DestroyPipelineLayout(VkDevices.device, pipelineLayout, null);
            AppState.appContext.GetContext<VkContext>().vk.DestroyDescriptorSetLayout(VkDevices.device, descriptorSetLayout, null);
            Debug.LogInfo("Cleaned up Pipeline");
        }
    }    
}
