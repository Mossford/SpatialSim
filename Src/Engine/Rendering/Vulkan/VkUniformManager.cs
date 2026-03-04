using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using SpatialSim.Engine.Core;
using SpatialSim.Engine.Core.Vulkan;

namespace SpatialSim.Engine.Rendering.Vulkan
{
    // TODO Needs uniform wrapping if go over memory limit of one buffer

    /// <summary>
    /// One for each pipeline
    /// </summary>
    public class VkUniformManager
    {
        public DescriptorSetLayout descriptorSetLayout;
        public DescriptorSet descriptorSet;
        public DescriptorPool descriptorPool;
        //store the blocks of each shaders sections
        public List<Buffer<byte>> uniformBuffers;
        
        public unsafe void Init()
        {
            // TODO Make the shader provide this information
            DescriptorSetLayoutBinding uboLayoutBinding = new()
            {
                Binding = 0,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBufferDynamic,
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
        }
        
        public void CreateUniformBuffers()
        {
            //create a copy for each swapchain we have
            uniformBuffers = new List<Buffer<byte>>();
            uniformBuffers.Add(new Buffer<byte>());
            for (int i = 0; i < uniformBuffers.Count; i++)
            {
                uniformBuffers[i].Create(VkSettings.MaxUniformSize, BufferUsage.Uniform, BufferMemoryUsage.Cpu);
            }
            
        }
        
        public unsafe void CreateDescriptorSets()
        {
            descriptorSet = new DescriptorSet();
            
            DescriptorPoolSize poolSize = new()
            {
                Type = DescriptorType.UniformBufferDynamic,
                DescriptorCount = 1,
            };

            DescriptorPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = 1,
                PPoolSizes = &poolSize,
                MaxSets = 1,
            };

            fixed (DescriptorPool* descriptorPoolPtr = &descriptorPool)
            {
                if (AppState.appContext.GetContext<VkContext>().vk.CreateDescriptorPool(
                        VkDevices.device, in poolInfo, null, descriptorPoolPtr) != Result.Success)
                {
                    throw new Exception("failed to create descriptor pool!");
                }

            }
            
            DescriptorSetLayout[] layouts = new DescriptorSetLayout[1];
            Array.Fill(layouts, descriptorSetLayout);

            fixed (DescriptorSetLayout* layoutsPtr = layouts)
            {
                DescriptorSetAllocateInfo allocateInfo = new()
                {
                    SType = StructureType.DescriptorSetAllocateInfo,
                    DescriptorPool = descriptorPool,
                    DescriptorSetCount = 1,
                    PSetLayouts = layoutsPtr,
                };
                
                fixed (DescriptorSet* descriptorSetsPtr = &descriptorSet)
                {
                    if (AppState.appContext.GetContext<VkContext>().vk.AllocateDescriptorSets(
                            VkDevices.device, in allocateInfo, descriptorSetsPtr) != Result.Success)
                    {
                        throw new Exception("failed to allocate descriptor sets!");
                    }
                }
            }


            for (int i = 0; i < uniformBuffers.Count; i++)
            {
                DescriptorBufferInfo bufferInfo = new()
                {
                    Buffer = ((VkBuffer<byte>)uniformBuffers[i].buffer!).buffer,
                    Offset = 0,
                    //Set the range to be the maximum block for a shader
                    Range = Shader.MaxBlockUniformMemory,
                };

                WriteDescriptorSet descriptorWrite = new()
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstSet = descriptorSet,
                    DstBinding = 0,
                    DstArrayElement = 0,
                    DescriptorType = DescriptorType.UniformBufferDynamic,
                    DescriptorCount = 1,
                    PBufferInfo = &bufferInfo,
                };

                AppState.appContext.GetContext<VkContext>().vk.UpdateDescriptorSets(VkDevices.device, 1, in descriptorWrite, 0, null);
            }
        }

        public unsafe void Clean()
        {
            for (int i = 0; i < uniformBuffers.Count; i++)
            {
                for (int j = 0; j < uniformBuffers.Count; j++)
                {
                    uniformBuffers[i].Clean();
                }
            }
            
            AppState.appContext.GetContext<VkContext>().vk.DestroyDescriptorPool(VkDevices.device, descriptorPool, null);
        }
        
        public static uint PadUniformSize(uint originalSize)
        {
            uint minUboAlignment = (uint)VkDevices.properties.Limits.MinUniformBufferOffsetAlignment;
            uint alignedSize = originalSize;
            if (minUboAlignment > 0)
            {
                alignedSize = (alignedSize + minUboAlignment - 1) & ~(minUboAlignment - 1);
            }
            return alignedSize;
        }

    }
}