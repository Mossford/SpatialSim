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
        public Dictionary<ShaderDescriptorDef, VkDescriptor> descriptorSets;
        
        public unsafe void Create(in Shader vertex, in Shader fragment, in PipelineSettings settings)
        {
            CreateDescriptors(vertex, fragment);
            
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

            Result result = AppState.appContext.GetContext<VkContext>().vk
                .CreatePipelineLayout(VkDevices.device, in pipelineLayoutInfo, null, out pipelineLayout);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to create pipeline layout {result}");
                throw new Exception($"Failed to create pipeline layout {result}");
            }
            
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
                    Format = Format.R32G32B32Sfloat,
                    Offset = (uint)Marshal.OffsetOf<Vertex>(nameof(Vertex.tangent)),
                },
                new VertexInputAttributeDescription()
                {
                    Binding = 0,
                    Location = 3,
                    Format = Format.R32G32B32Sfloat,
                    Offset = (uint)Marshal.OffsetOf<Vertex>(nameof(Vertex.biTangent)),
                },
                new VertexInputAttributeDescription()
                {
                    Binding = 0,
                    Location = 4,
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

            PipelineViewportStateCreateInfo viewportState = new()
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                ScissorCount = 1
            };

            PipelineRasterizationStateCreateInfo rasterizer = new()
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                DepthClampEnable = false,
                RasterizerDiscardEnable = false,
                PolygonMode = settings.rasterizationMode switch
                {
                    RasterizationMode.Triangle => PolygonMode.Fill,
                    RasterizationMode.Line => PolygonMode.Line,
                    _ => PolygonMode.Fill
                },
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

            PipelineDepthStencilStateCreateInfo depthStencil = new()
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = settings.depthTest,
                DepthWriteEnable = settings.depthTest,
                DepthCompareOp = CompareOp.Less,
                DepthBoundsTestEnable = false,
                StencilTestEnable = false,
            };
            
            PipelineColorBlendAttachmentState colorBlendAttachment = new()
            {
                BlendEnable = settings.blendColor,
                SrcColorBlendFactor = BlendFactor.SrcAlpha,
                DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
                ColorBlendOp = BlendOp.Add,
                SrcAlphaBlendFactor = BlendFactor.One,
                DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
                AlphaBlendOp = BlendOp.Add,
                ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit,
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

            Format swapChainFormat = VkSwapChain.swapChainImageFormat;
            PipelineRenderingCreateInfo pipelineRenderingCreateInfo = new()
            {
                SType = StructureType.PipelineRenderingCreateInfo,
                ColorAttachmentCount = 1,
                PColorAttachmentFormats = &swapChainFormat,
                DepthAttachmentFormat = VkDepthBuffer.FindDepthFormat()
            };

            DynamicState[] dynamicStates =
            {
                DynamicState.Viewport,
                DynamicState.Scissor,
                
            };

            PipelineDynamicStateCreateInfo pipelineDynamicStateCreateInfo;
            fixed (DynamicState* states = dynamicStates)
            {
                pipelineDynamicStateCreateInfo = new()
                {
                    SType = StructureType.PipelineDynamicStateCreateInfo,
                    DynamicStateCount = (uint)dynamicStates.Length,
                    PDynamicStates = states
                };
            }
            
            GraphicsPipelineCreateInfo pipelineInfo = new()
            {
                PNext = &pipelineRenderingCreateInfo,
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2,
                PStages = shaderStages,
                PVertexInputState = &vertexInputInfo,
                PInputAssemblyState = &inputAssembly,
                PViewportState = &viewportState,
                PRasterizationState = &rasterizer,
                PMultisampleState = &multisampling,
                PColorBlendState = &colorBlending,
                PDepthStencilState = &depthStencil,
                PDynamicState = &pipelineDynamicStateCreateInfo,
                Layout = pipelineLayout,
                BasePipelineHandle = default,
            };

             result = AppState.appContext.GetContext<VkContext>().vk
                .CreateGraphicsPipelines(VkDevices.device, default, 1, in pipelineInfo, null, out pipeline);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to create graphics pipeline {result}");
                throw new Exception($"Failed to create graphics pipeline {result}");
            }

            SilkMarshal.Free((nint)vertShaderStageInfo.PName);
            SilkMarshal.Free((nint)fragShaderStageInfo.PName);
            
            Debug.LogInfo("Successful pipeline creation");
        }

        public void UpdateUniforms(in Shader shader, int binding)
        {
            int set = RendererSettings.VertexUniformSet;
            if (shader.settings.type == ShaderType.Fragment)
            {
                set = RendererSettings.FragmentUniformSet;
            }
            
            ShaderDescriptorDef def = new ShaderDescriptorDef(set, [binding], ShaderDescriptorUsage.Uniform, shader.settings.type);
            
            uniformManager.GetBuffer(def, shader.uniformData.Count)
                .UpdateUniformData(new Span<byte>(shader.uniformData[def].ToArray()));
        }

        unsafe DescriptorSetLayout CreateDescriptorSetLayout(ShaderStageFlags stage, DescriptorType type, int[] bindings)
        {
            DescriptorSetLayoutBinding[] layoutBindings = new DescriptorSetLayoutBinding[bindings.Length];
            for (int i = 0; i < bindings.Length; i++)
            {
                layoutBindings[i] = new()
                {
                    Binding = (uint)bindings[i],
                    //we start at one but this will have to change as more descriptors get added to some binding point
                    DescriptorCount = 1,
                    DescriptorType = type,
                    PImmutableSamplers = null,
                    StageFlags = stage,
                };
                
                if (type == DescriptorType.CombinedImageSampler)
                {
                    layoutBindings[i].DescriptorCount = VkSettings.MaxSamplers;
                }
            }

            DescriptorSetLayoutCreateInfo layoutInfo;
            fixed (DescriptorSetLayoutBinding* layouts = layoutBindings)
            {
                layoutInfo = new()
                {
                    SType = StructureType.DescriptorSetLayoutCreateInfo,
                    BindingCount = (uint)layoutBindings.Length,
                    PBindings = layouts,
                };
            }

            if (type != DescriptorType.UniformBufferDynamic)
            {
                DescriptorBindingFlags[] bindingFlags = new DescriptorBindingFlags[layoutBindings.Length];

                for (int i = 0; i < bindingFlags.Length; i++)
                {
                    bindingFlags[i] = DescriptorBindingFlags.PartiallyBoundBit | 
                                      DescriptorBindingFlags.UpdateAfterBindBit | 
                                      DescriptorBindingFlags.UpdateUnusedWhilePendingBit;
                }

                fixed (DescriptorBindingFlags* flagsPtr = bindingFlags)
                {
                    DescriptorSetLayoutBindingFlagsCreateInfo flagsInfo = new()
                    {
                        SType = StructureType.DescriptorSetLayoutBindingFlagsCreateInfo,
                        BindingCount = (uint)bindingFlags.Length,
                        PBindingFlags = flagsPtr
                    };

                    layoutInfo.PNext = &flagsInfo;
                }

                layoutInfo.Flags = DescriptorSetLayoutCreateFlags.UpdateAfterBindPoolBit;
            }

            Result result = AppState.appContext.GetContext<VkContext>().vk
                .CreateDescriptorSetLayout(VkDevices.device, in layoutInfo, null, out DescriptorSetLayout layout);
            if (result != Result.Success)
            {
                Debug.Error($"Failed to create descriptor set layout {result}");
                throw new Exception($"Failed to create descriptor set layout {result}");
            }

            return layout;
        }

        unsafe void CreateDescriptors(in Shader vertex, in Shader fragment)
        {
            descriptorSetLayouts = new Dictionary<ShaderDescriptorDef, DescriptorSetLayout>();
            descriptorSets = new Dictionary<ShaderDescriptorDef, VkDescriptor>();

            List<ShaderDescriptorDef> uniformDefs = new();
            CreateShaderDescriptors(vertex, uniformDefs);
            CreateShaderDescriptors(fragment, uniformDefs);

            uniformManager = new VkUniformManager();
            uniformManager.Init(
                this,
                [ShaderType.Vertex, ShaderType.Fragment],
                uniformDefs.ToArray());
        }

        unsafe void CreateShaderDescriptors(in Shader shader, in List<ShaderDescriptorDef> uniformDefs)
        {
            for (int i = 0; i < shader.settings.descriptorDef.Length; i++)
            {
                ShaderDescriptorDef def = shader.settings.descriptorDef[i];

                DescriptorType type = def.usage switch
                {
                    ShaderDescriptorUsage.Uniform => DescriptorType.UniformBufferDynamic,
                    ShaderDescriptorUsage.Sampler => DescriptorType.CombinedImageSampler,
                    _ => DescriptorType.UniformBufferDynamic
                };

                DescriptorSetLayout layout = CreateDescriptorSetLayout(
                    ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
                    type,
                    def.bindings
                );

                if (!descriptorSetLayouts.TryAdd(def, layout))
                {
                    Debug.Error($"Tried to add a descriptor set layout to vertex " +
                                $"{def} " + "that already exists, skipping");
                    
                    AppState.appContext.GetContext<VkContext>().vk.DestroyDescriptorSetLayout(VkDevices.device, layout, null);
                }

                if (def.usage == ShaderDescriptorUsage.Uniform)
                    uniformDefs.Add(def);
            }
        }

        public VkDescriptor GetDescriptor(ShaderDescriptorDef def)
        {
            if (descriptorSets.TryGetValue(def, out VkDescriptor? descriptor))
                return descriptor;

            descriptor = new VkDescriptor();
            descriptor.Create(this, def);

            descriptorSets[def] = descriptor;
            return descriptor;
        }

        public void Bind()
        {
            
        }

        public unsafe void Clean()
        {
            uniformManager.Clean();
            
            AppState.appContext.GetContext<VkContext>().vk.DeviceWaitIdle(VkDevices.device);
            
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
