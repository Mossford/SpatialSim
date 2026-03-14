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
        public VkUniformManager uniformManager;

        public Dictionary<ShaderDescriptorDef, DescriptorSetLayout> descriptorSetLayouts;
        
        public unsafe void Create(in Shader vertex, in Shader fragment)
        {
            descriptorSetLayouts = new Dictionary<ShaderDescriptorDef, DescriptorSetLayout>();
            for (int i = 0; i < vertex.settings.descriptorDef.Length; i++)
            {
                DescriptorType type = DescriptorType.UniformBufferDynamic;
                switch (vertex.settings.descriptorDef[i].usage)
                {
                    case ShaderDescriptorUsage.Uniform:
                    {
                        type = DescriptorType.UniformBufferDynamic;
                        break;
                    }
                    case ShaderDescriptorUsage.Sampler:
                    {
                        type = DescriptorType.CombinedImageSampler;
                        break;
                    }
                }
                
                if (!descriptorSetLayouts.TryAdd(vertex.settings.descriptorDef[i], CreateDescriptorSetLayout(ShaderStageFlags.VertexBit, type, vertex.settings.descriptorDef[i].binding)))
                {
                    Debug.Warning($"Tried to add a descriptor set layout {vertex.settings.descriptorDef[i].set} {vertex.settings.descriptorDef[i].binding} {vertex.settings.descriptorDef[i].usage} that already exists, skipping");
                }
            }
            
            for (int i = 0; i < fragment.settings.descriptorDef.Length; i++)
            {
                DescriptorType type = DescriptorType.UniformBufferDynamic;
                switch (fragment.settings.descriptorDef[i].usage)
                {
                    case ShaderDescriptorUsage.Uniform:
                    {
                        type = DescriptorType.UniformBufferDynamic;
                        break;
                    }
                    case ShaderDescriptorUsage.Sampler:
                    {
                        type = DescriptorType.CombinedImageSampler;
                        break;
                    }
                }
                
                if (!descriptorSetLayouts.TryAdd(fragment.settings.descriptorDef[i], CreateDescriptorSetLayout(ShaderStageFlags.FragmentBit, type, fragment.settings.descriptorDef[i].binding)))
                {
                    Debug.Warning($"Tried to add a descriptor set layout {fragment.settings.descriptorDef[i].set} {fragment.settings.descriptorDef[i].binding} {fragment.settings.descriptorDef[i].usage} that already exists, skipping");
                }
            }
            
            uniformManager = new VkUniformManager();
            uniformManager.Init(this);
            
            PipelineShaderStageCreateInfo vertShaderStageInfo = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.VertexBit,
                Module = ((VkShader)vertex.shader!).program,
                PName = (byte*)SilkMarshal.StringToPtr("main")
            };

            PipelineShaderStageCreateInfo fragShaderStageInfo = new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.FragmentBit,
                Module = ((VkShader)fragment.shader!).program,
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
            DescriptorSetLayout[] layouts = descriptorSetLayouts.Values.ToArray();
            fixed (DescriptorSetLayout* layoutPtr = layouts)
            {
                pipelineLayoutInfo = new()
                {
                    SType = StructureType.PipelineLayoutCreateInfo,
                    SetLayoutCount = (uint)layouts.Length,
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
            
            Debug.LogInfo("Successful pipeline creation");
        }

        public void UpdateUniforms(in Shader shader, int frame)
        {
            uniformManager.GetBuffer(shader.uniformData.Count)
                .UpdateUniformData(new Span<byte>(shader.uniformData.ToArray()));
        }

        unsafe DescriptorSetLayout CreateDescriptorSetLayout(ShaderStageFlags stage, DescriptorType type, int binding)
        {
            DescriptorSetLayoutBinding layoutBindings = new()
            {
                Binding = (uint)binding,
                DescriptorCount = 1,
                DescriptorType = type,
                PImmutableSamplers = null,
                StageFlags = stage,
            };

            DescriptorSetLayoutCreateInfo layoutInfo = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 1,
                PBindings = &layoutBindings,
            };

            Result result = AppState.appContext.GetContext<VkContext>().vk
                .CreateDescriptorSetLayout(VkDevices.device, in layoutInfo, null, out DescriptorSetLayout layout);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to create descriptor set layout {result}");
                throw new Exception($"Failed to create descriptor set layout {result}");
            }

            return layout;
        }

        public void Bind()
        {
            
        }

        public unsafe void Clean()
        {
            uniformManager.Clean();
            
            DescriptorSetLayout[] layouts = descriptorSetLayouts.Values.ToArray();
            for (int i = 0; i < layouts.Length; i++)
            {
                AppState.appContext.GetContext<VkContext>().vk.DestroyDescriptorSetLayout(VkDevices.device, layouts[i], null);
            }
            
            AppState.appContext.GetContext<VkContext>().vk.DestroyPipeline(VkDevices.device, pipeline, null);
            AppState.appContext.GetContext<VkContext>().vk.DestroyPipelineLayout(VkDevices.device, pipelineLayout, null);
            Debug.LogInfo("Cleaned up Pipeline");
        }
    }    
}
